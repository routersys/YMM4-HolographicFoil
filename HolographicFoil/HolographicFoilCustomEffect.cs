using System.Numerics;
using System.Runtime.InteropServices;
using Vortice;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace HolographicFoil;

internal sealed class HolographicFoilCustomEffect(IGraphicsDevicesAndContext devices) : D2D1CustomShaderEffectBase(Create<EffectImpl>(devices))
{
    public float Amount { get => GetFloatValue((int)EffectImpl.Properties.Amount); set => SetValue((int)EffectImpl.Properties.Amount, value); }
    public float Angle { get => GetFloatValue((int)EffectImpl.Properties.Angle); set => SetValue((int)EffectImpl.Properties.Angle, value); }
    public Vector4 FoilColor { set => SetValue((int)EffectImpl.Properties.FoilColor, value); }
    public float Rainbow { get => GetFloatValue((int)EffectImpl.Properties.Rainbow); set => SetValue((int)EffectImpl.Properties.Rainbow, value); }
    public float Relief { get => GetFloatValue((int)EffectImpl.Properties.Relief); set => SetValue((int)EffectImpl.Properties.Relief, value); }
    public float PatternScale { get => GetFloatValue((int)EffectImpl.Properties.PatternScale); set => SetValue((int)EffectImpl.Properties.PatternScale, value); }
    public float PatternAngle { get => GetFloatValue((int)EffectImpl.Properties.PatternAngle); set => SetValue((int)EffectImpl.Properties.PatternAngle, value); }
    public float Sparkle { get => GetFloatValue((int)EffectImpl.Properties.Sparkle); set => SetValue((int)EffectImpl.Properties.Sparkle, value); }
    public float Motion { get => GetFloatValue((int)EffectImpl.Properties.Motion); set => SetValue((int)EffectImpl.Properties.Motion, value); }
    public float Time { get => GetFloatValue((int)EffectImpl.Properties.Time); set => SetValue((int)EffectImpl.Properties.Time, value); }
    public int AlphaSource { get => GetIntValue((int)EffectImpl.Properties.AlphaSource); set => SetValue((int)EffectImpl.Properties.AlphaSource, value); }
    public int RevealMode { get => GetIntValue((int)EffectImpl.Properties.RevealMode); set => SetValue((int)EffectImpl.Properties.RevealMode, value); }
    public float RevealStrength { get => GetFloatValue((int)EffectImpl.Properties.RevealStrength); set => SetValue((int)EffectImpl.Properties.RevealStrength, value); }
    public float RevealRange { get => GetFloatValue((int)EffectImpl.Properties.RevealRange); set => SetValue((int)EffectImpl.Properties.RevealRange, value); }
    public float RevealBlend { get => GetFloatValue((int)EffectImpl.Properties.RevealBlend); set => SetValue((int)EffectImpl.Properties.RevealBlend, value); }
    public float AtlasWidth { get => GetFloatValue((int)EffectImpl.Properties.AtlasWidth); set => SetValue((int)EffectImpl.Properties.AtlasWidth, value); }
    public float AtlasHeight { get => GetFloatValue((int)EffectImpl.Properties.AtlasHeight); set => SetValue((int)EffectImpl.Properties.AtlasHeight, value); }
    public int AtlasFrameCount { get => GetIntValue((int)EffectImpl.Properties.AtlasFrameCount); set => SetValue((int)EffectImpl.Properties.AtlasFrameCount, value); }

    public void SetSourceInput(ID2D1Image? image) => SetInput(0, image, true);
    public void SetAtlasInput(ID2D1Image? image) => SetInput(1, image, true);

    [CustomEffect(2)]
    sealed class EffectImpl : D2D1CustomShaderEffectImplBase<EffectImpl>
    {
        ConstantBuffer constants = new()
        {
            Amount = 1f,
            FoilColor = Vector4.One,
            Rainbow = 1f,
            Relief = 0.8f,
            PatternScale = 1.2f,
            PatternAngle = 35f,
            Sparkle = 0.45f,
            Motion = 0.2f,
            RevealRange = 90f,
            RevealBlend = 0.35f,
            AtlasInfo = new Vector4(1f, 1f, 3f, 0f)
        };

        [CustomEffectProperty(PropertyType.Float, (int)Properties.Amount)]
        public float Amount { get => constants.Amount; set { constants.Amount = Clamp(value, 0f, 1f, 1f); UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Float, (int)Properties.Angle)]
        public float Angle { get => constants.Angle; set { constants.Angle = Clamp(value, -180f, 180f, 0f); UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Vector4, (int)Properties.FoilColor)]
        public Vector4 FoilColor { get => constants.FoilColor; set { constants.FoilColor = Clamp01(value); UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Float, (int)Properties.Rainbow)]
        public float Rainbow { get => constants.Rainbow; set { constants.Rainbow = Clamp(value, 0f, 2f, 1f); UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Float, (int)Properties.Relief)]
        public float Relief { get => constants.Relief; set { constants.Relief = Clamp(value, 0f, 3f, 0f); UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Float, (int)Properties.PatternScale)]
        public float PatternScale { get => constants.PatternScale; set { constants.PatternScale = Clamp(value, 0.1f, 5f, 1f); UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Float, (int)Properties.PatternAngle)]
        public float PatternAngle { get => constants.PatternAngle; set { constants.PatternAngle = Clamp(value, -180f, 180f, 0f); UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Float, (int)Properties.Sparkle)]
        public float Sparkle { get => constants.Sparkle; set { constants.Sparkle = Clamp(value, 0f, 2f, 0f); UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Float, (int)Properties.Motion)]
        public float Motion { get => constants.Motion; set { constants.Motion = Clamp(value, -2f, 2f, 0f); UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Float, (int)Properties.Time)]
        public float Time { get => constants.Time; set { constants.Time = float.IsFinite(value) ? value : 0f; UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Int32, (int)Properties.AlphaSource)]
        public int AlphaSource { get => constants.AlphaSource; set { constants.AlphaSource = Math.Clamp(value, 0, 2); UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Int32, (int)Properties.RevealMode)]
        public int RevealMode { get => constants.RevealMode; set { constants.RevealMode = Math.Clamp(value, 0, 2); UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Float, (int)Properties.RevealStrength)]
        public float RevealStrength { get => constants.RevealStrength; set { constants.RevealStrength = Clamp(value, 0f, 2f, 1f); UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Float, (int)Properties.RevealRange)]
        public float RevealRange { get => constants.RevealRange; set { constants.RevealRange = Clamp(value, 1f, 180f, 90f); UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Float, (int)Properties.RevealBlend)]
        public float RevealBlend { get => constants.RevealBlend; set { constants.RevealBlend = Clamp(value, 0f, 1f, 0.35f); UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Float, (int)Properties.AtlasWidth)]
        public float AtlasWidth { get => constants.AtlasInfo.X; set { constants.AtlasInfo.X = Clamp(value, 1f, 32768f, 1f); UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Float, (int)Properties.AtlasHeight)]
        public float AtlasHeight { get => constants.AtlasInfo.Y; set { constants.AtlasInfo.Y = Clamp(value, 1f, 32768f, 1f); UpdateConstants(); } }

        [CustomEffectProperty(PropertyType.Int32, (int)Properties.AtlasFrameCount)]
        public int AtlasFrameCount { get => (int)constants.AtlasInfo.Z; set { constants.AtlasInfo.Z = Math.Clamp(value, 2, 16); UpdateConstants(); } }

        public EffectImpl() : base(ShaderResourceUri.Get("HolographicFoil"))
        {
        }

        public override void SetDrawInfo(ID2D1DrawInfo drawInfo)
        {
            base.SetDrawInfo(drawInfo);
            drawInfo?.SetInputDescription(1, new InputDescription
            {
                Filter = Filter.MinMagMipLinear,
                LevelOfDetailCount = 0,
            });
        }

        protected override void UpdateConstants()
        {
            drawInformation?.SetPixelShaderConstantBuffer(constants);
        }

        public override void MapInputRectsToOutputRect(RawRect[] inputRects, RawRect[] inputOpaqueSubRects, out RawRect outputRect, out RawRect outputOpaqueSubRect)
        {
            inputRect = inputRects.Length > 0 ? ClampInputRect(inputRects[0]) : default;
            constants.InputBounds = new Vector4(inputRect.Left, inputRect.Top, inputRect.Right, inputRect.Bottom);
            UpdateConstants();
            outputRect = inputRect;
            outputOpaqueSubRect = default;
        }

        public override void MapOutputRectToInputRects(RawRect outputRect, RawRect[] inputRects)
        {
            if (inputRects.Length > 0)
                inputRects[0] = new RawRect(outputRect.Left - 1, outputRect.Top - 1, outputRect.Right + 1, outputRect.Bottom + 1);
            if (inputRects.Length > 1)
                inputRects[1] = new RawRect(0, 0, (int)Math.Ceiling(constants.AtlasInfo.X), (int)Math.Ceiling(constants.AtlasInfo.Y));
        }

        static float Clamp(float value, float minimum, float maximum, float fallback)
        {
            if (!float.IsFinite(value))
                return fallback;
            return Math.Clamp(value, minimum, maximum);
        }

        static Vector4 Clamp01(Vector4 value)
        {
            return new Vector4(
                Clamp(value.X, 0f, 1f, 1f),
                Clamp(value.Y, 0f, 1f, 1f),
                Clamp(value.Z, 0f, 1f, 1f),
                Clamp(value.W, 0f, 1f, 1f));
        }

        [StructLayout(LayoutKind.Sequential)]
        struct ConstantBuffer
        {
            public Vector4 FoilColor;
            public Vector4 InputBounds;
            public Vector4 AtlasInfo;
            public float Amount;
            public float Angle;
            public float Rainbow;
            public float Relief;
            public float PatternScale;
            public float PatternAngle;
            public float Sparkle;
            public float Motion;
            public float Time;
            public float RevealStrength;
            public float RevealBlend;
            public float RevealRange;
            public int AlphaSource;
            public int RevealMode;
            public float Pad0;
            public float Pad1;
        }

        public enum Properties
        {
            Amount,
            Angle,
            FoilColor,
            Rainbow,
            Relief,
            PatternScale,
            PatternAngle,
            Sparkle,
            Motion,
            Time,
            AlphaSource,
            RevealMode,
            RevealStrength,
            RevealRange,
            RevealBlend,
            AtlasWidth,
            AtlasHeight,
            AtlasFrameCount,
        }
    }
}
