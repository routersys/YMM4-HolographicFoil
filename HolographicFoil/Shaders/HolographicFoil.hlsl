Texture2D SourceTexture : register(t0);
Texture2D AtlasTexture : register(t1);
SamplerState SourceSampler : register(s0);

cbuffer Constants : register(b0)
{
    float4 foilColor : packoffset(c0);
    float4 inputBounds : packoffset(c1);
    float4 atlasInfo : packoffset(c2);
    float amount : packoffset(c3.x);
    float angle : packoffset(c3.y);
    float rainbow : packoffset(c3.z);
    float relief : packoffset(c3.w);
    float patternScale : packoffset(c4.x);
    float patternAngle : packoffset(c4.y);
    float sparkle : packoffset(c4.z);
    float motion : packoffset(c4.w);
    float time : packoffset(c5.x);
    float revealStrength : packoffset(c5.y);
    float revealBlend : packoffset(c5.z);
    float revealRange : packoffset(c5.w);
    int alphaSource : packoffset(c6.x);
    int revealMode : packoffset(c6.y);
};

float4 SampleSource(float2 uv, float2 scenePosition)
{
    if (scenePosition.x < inputBounds.x || scenePosition.y < inputBounds.y || scenePosition.x >= inputBounds.z || scenePosition.y >= inputBounds.w)
        return (float4)0;
    return SourceTexture.SampleLevel(SourceSampler, uv, 0);
}

float Luminance(float3 color)
{
    return dot(color, float3(0.2126, 0.7152, 0.0722));
}

float MaskValue(float4 color)
{
    float luminance = Luminance(color.rgb);
    if (alphaSource == 1)
        return saturate(luminance);
    if (alphaSource == 2)
        return saturate(color.a * luminance);
    return saturate(color.a);
}

float Hash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float3 CycleColor(float phase)
{
    float3 offsets = float3(0.0, -0.33333334, -0.6666667);
    return 0.5 + 0.5 * cos(6.2831853 * (phase + offsets));
}

float LineMask(float phase)
{
    float distance = abs(frac(phase) - 0.5) * 2.0;
    return 1.0 - smoothstep(0.08, 0.55, distance);
}

float Guilloche(float2 localUv, float angleRad, float scale)
{
    float2 p = localUv * 2.0 - 1.0;
    float radius = length(p);
    float theta = atan2(p.y, p.x);
    float a = sin((theta * 9.0 + radius * 24.0 + angleRad * 2.0) * scale);
    float b = sin((p.x * 34.0 - p.y * 21.0 + sin(theta * 5.0 + angleRad) * 2.0) * scale);
    return smoothstep(0.86, 0.98, abs(a * b));
}

float4 SampleAtlas(float2 localUv, float angleValue)
{
    float frames = clamp(round(atlasInfo.z), 2.0, 16.0);
    float range = max(revealRange, 1.0);
    float coordinate = saturate((angleValue + range * 0.5) / range) * (frames - 1.0);
    float lower = floor(coordinate);
    float upper = min(lower + 1.0, frames - 1.0);
    float fraction = coordinate - lower;
    float width = clamp(revealBlend, 0.0001, 1.0);
    float blend = smoothstep(0.5 - width * 0.5, 0.5 + width * 0.5, fraction);
    float2 uv0 = float2((localUv.x + lower) / frames, localUv.y);
    float2 uv1 = float2((localUv.x + upper) / frames, localUv.y);
    float4 c0 = AtlasTexture.SampleLevel(SourceSampler, uv0, 0);
    float4 c1 = AtlasTexture.SampleLevel(SourceSampler, uv1, 0);
    return lerp(c0, c1, blend);
}

float4 main(
    float4 position : SV_POSITION,
    float4 scenePosition : SCENE_POSITION,
    float4 uv0 : TEXCOORD0,
    float4 uv1 : TEXCOORD1
) : SV_TARGET
{
    float4 source = SampleSource(uv0.xy, scenePosition.xy);
    if (amount <= 0.0 || source.a <= 0.0)
        return source;

    float2 inputSize = max(inputBounds.zw - inputBounds.xy, float2(1.0, 1.0));
    float2 localUv = saturate((scenePosition.xy - inputBounds.xy) / inputSize);
    float mask = MaskValue(source);

    float2 pixel = uv0.zw;
    float left = MaskValue(SampleSource(uv0.xy - float2(pixel.x, 0.0), scenePosition.xy - float2(1.0, 0.0)));
    float right = MaskValue(SampleSource(uv0.xy + float2(pixel.x, 0.0), scenePosition.xy + float2(1.0, 0.0)));
    float top = MaskValue(SampleSource(uv0.xy - float2(0.0, pixel.y), scenePosition.xy - float2(0.0, 1.0)));
    float bottom = MaskValue(SampleSource(uv0.xy + float2(0.0, pixel.y), scenePosition.xy + float2(0.0, 1.0)));
    float2 gradient = float2(right - left, bottom - top);
    float3 normal = normalize(float3(-gradient * relief * 8.0, 1.0));

    float angleRad = radians(angle);
    float patternRad = radians(patternAngle);
    float2 direction = float2(cos(patternRad), sin(patternRad));
    float phase = dot(scenePosition.xy, direction) * 0.035 * patternScale + angle * 0.012 + time * motion;
    float microline = LineMask(phase);
    float3 cycle = CycleColor(phase + angle * 0.004);

    float3 light = normalize(float3(sin(angleRad * 0.83 + 0.7), cos(angleRad * 0.61 + 1.1), 0.85));
    float specular = pow(saturate(dot(normal, light)), 24.0) * relief;
    float broad = pow(saturate(0.5 + 0.5 * sin(phase * 0.45 + angleRad)), 3.0);

    float2 sparkleCell = floor(scenePosition.xy / 3.0);
    float sparkleNoise = Hash21(sparkleCell + floor(time * max(abs(motion), 0.05) * 18.0));
    float sparkleValue = step(0.985 - saturate(sparkle) * 0.02, sparkleNoise) * sparkle * (0.35 + 0.65 * microline);

    float3 foil = foilColor.rgb * (0.22 * broad + 0.48 * microline + specular) + cycle * rainbow * (0.45 * microline + 0.2 * broad) + sparkleValue;

    float3 base = source.rgb;

    if (revealMode == 1)
    {
        float latent = Guilloche(localUv, angleRad, patternScale);
        foil += latent * revealStrength * (foilColor.rgb * 0.8 + cycle * 0.5);
    }
    else if (revealMode == 2)
    {
        float4 atlas = SampleAtlas(localUv, angle);
        float k = saturate(revealStrength) * mask;
        base = source.rgb * (1.0 - k * atlas.a) + source.a * atlas.rgb * k;
    }

    float strength = mask * foilColor.a;
    float3 result = base + source.a * strength * foil;
    result = min(saturate(result), source.a.xxx);
    return float4(lerp(source.rgb, result, amount), source.a);
}
