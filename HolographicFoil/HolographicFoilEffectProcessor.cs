using System.Numerics;
using System.Windows.Media;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Player.Video.Effects;
using YukkuriMovieMaker.Plugin;

namespace HolographicFoil;

internal sealed class HolographicFoilEffectProcessor(IGraphicsDevicesAndContext devices, HolographicFoilEffect item) : VideoEffectProcessorBase(devices)
{
    readonly IGraphicsDevicesAndContext devices = devices;
    readonly HolographicFoilEffect item = item;
    HolographicFoilCustomEffect? effect;
    Flood? fallbackAtlas;
    IImageFileSource? atlasImage;
    string loadedAtlasPath = string.Empty;
    bool isFirst = true;

    public override DrawDescription Update(EffectDescription effectDescription)
    {
        if (IsPassThroughEffect || effect is null)
            return effectDescription.DrawDescription;

        var frame = effectDescription.ItemPosition.Frame;
        var length = effectDescription.ItemDuration.Frame;
        var fps = effectDescription.FPS;
        var drawDescription = effectDescription.DrawDescription;

        RefreshAtlas(item.RevealMode == HolographicFoilRevealMode.ImageAtlas ? item.RevealImage : null);

        var angle = ComputeAngle(drawDescription, frame, length, fps);
        var time = fps > 0 ? (float)frame / fps : 0f;

        effect.Amount = Percent(item.Amount.GetValue(frame, length, fps), 0f, 1f);
        effect.Angle = angle;
        effect.FoilColor = ToVector4(item.FoilColor);
        effect.Rainbow = Percent(item.Rainbow.GetValue(frame, length, fps), 0f, 2f);
        effect.Relief = Percent(item.Relief.GetValue(frame, length, fps), 0f, 3f);
        effect.PatternScale = Percent(item.PatternScale.GetValue(frame, length, fps), 0.1f, 5f);
        effect.PatternAngle = Finite(item.PatternAngle.GetValue(frame, length, fps), -180f, 180f, 0f);
        effect.Sparkle = Percent(item.Sparkle.GetValue(frame, length, fps), 0f, 2f);
        effect.Motion = Percent(item.Motion.GetValue(frame, length, fps), -2f, 2f);
        effect.Time = time;
        effect.AlphaSource = (int)item.AlphaSource;
        effect.RevealMode = (int)item.RevealMode;
        effect.RevealStrength = Percent(item.RevealStrength.GetValue(frame, length, fps), 0f, 2f);
        effect.RevealRange = Finite(item.RevealRange.GetValue(frame, length, fps), 1f, 180f, 90f);
        effect.RevealBlend = Finite(item.RevealBlend.GetValue(frame, length, fps), 0f, 1f, 0.35f);
        effect.AtlasFrameCount = Math.Clamp(item.RevealFrames, 2, 16);

        isFirst = false;

        return drawDescription;
    }

    float ComputeAngle(DrawDescription drawDescription, int frame, int length, int fps)
    {
        var value = item.ViewSource switch
        {
            HolographicFoilViewSource.Camera => HolographicFoilMath.ComputeCameraYawDegrees(drawDescription.Camera) + drawDescription.Rotation.Y,
            HolographicFoilViewSource.ItemRotation => drawDescription.Rotation.Y,
            HolographicFoilViewSource.Manual => item.ManualAngle.GetValue(frame, length, fps),
            _ => 0d
        };

        if (item.ViewSource != HolographicFoilViewSource.Manual)
            value *= item.ViewSensitivity.GetValue(frame, length, fps) / 100d;

        return (float)HolographicFoilMath.NormalizeAngleDegrees(value);
    }

    void RefreshAtlas(string? path)
    {
        path ??= string.Empty;
        if (!isFirst && string.Equals(path, loadedAtlasPath, StringComparison.Ordinal))
            return;

        if (atlasImage is not null)
            disposer.RemoveAndDispose(ref atlasImage);

        loadedAtlasPath = path;

        if (!string.IsNullOrWhiteSpace(path))
        {
            atlasImage = ImageFileSourceFactory.Create(devices, path);
            if (atlasImage is not null)
            {
                disposer.Collect(atlasImage);
                effect!.AtlasWidth = atlasImage.Output.PixelSize.Width;
                effect.AtlasHeight = atlasImage.Output.PixelSize.Height;
                effect.SetAtlasInput(atlasImage.Output);
                return;
            }
        }

        effect!.AtlasWidth = 1f;
        effect.AtlasHeight = 1f;
        if (fallbackAtlas is not null)
        {
            using var output = fallbackAtlas.Output;
            effect.SetAtlasInput(output);
        }
    }

    protected override ID2D1Image? CreateEffect(IGraphicsDevicesAndContext devices)
    {
        effect = new HolographicFoilCustomEffect(devices);
        if (!effect.IsEnabled)
        {
            effect.Dispose();
            effect = null;
            return null;
        }

        fallbackAtlas = new Flood(devices.DeviceContext) { Color = Vector4.Zero };
        disposer.Collect(effect);
        disposer.Collect(fallbackAtlas);

        using (var output = fallbackAtlas.Output)
            effect.SetAtlasInput(output);

        var result = effect.Output;
        disposer.Collect(result);
        return result;
    }

    protected override void setInput(ID2D1Image? input)
    {
        effect?.SetSourceInput(input);
    }

    protected override void ClearEffectChain()
    {
        effect?.SetSourceInput(null);
        effect?.SetAtlasInput(null);
        isFirst = true;
    }

    static float Percent(double value, float minimum, float maximum) => Finite(value / 100d, minimum, maximum, minimum);

    static float Finite(double value, float minimum, float maximum, float fallback)
    {
        if (!double.IsFinite(value))
            return fallback;
        return (float)Math.Clamp(value, minimum, maximum);
    }

    static Vector4 ToVector4(Color color) => new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
}
