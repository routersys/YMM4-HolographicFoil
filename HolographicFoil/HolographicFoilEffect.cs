using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Settings;

namespace HolographicFoil;

[VideoEffect(nameof(Texts.EffectName), [VideoEffectCategories.Filtering, VideoEffectCategories.Decoration], [nameof(Texts.TagHologram), nameof(Texts.TagFoil), nameof(Texts.TagSecurity), nameof(Texts.TagReveal)], IsAviUtlSupported = false, ResourceType = typeof(Texts))]
public sealed class HolographicFoilEffect : VideoEffectBase, IFileItem, IResourceItem
{
    public override string Label => Texts.EffectName;

    [Display(GroupName = nameof(Texts.BasicGroup), Name = nameof(Texts.AmountName), Description = nameof(Texts.AmountDesc), Order = 0, ResourceType = typeof(Texts))]
    [AnimationSlider("F1", "%", 0d, 100d)]
    public Animation Amount { get; } = new(100, 0, 100);

    [Display(GroupName = nameof(Texts.BasicGroup), Name = nameof(Texts.AlphaSourceName), Description = nameof(Texts.AlphaSourceDesc), Order = 1, ResourceType = typeof(Texts))]
    [EnumComboBox]
    public HolographicFoilAlphaSource AlphaSource { get => alphaSource; set => Set(ref alphaSource, value); }
    HolographicFoilAlphaSource alphaSource = HolographicFoilAlphaSource.Alpha;

    [Display(GroupName = nameof(Texts.ViewGroup), Name = nameof(Texts.ViewSourceName), Description = nameof(Texts.ViewSourceDesc), Order = 10, ResourceType = typeof(Texts))]
    [EnumComboBox]
    public HolographicFoilViewSource ViewSource { get => viewSource; set => Set(ref viewSource, value); }
    HolographicFoilViewSource viewSource = HolographicFoilViewSource.Camera;

    [Display(GroupName = nameof(Texts.ViewGroup), Name = nameof(Texts.ViewSensitivityName), Description = nameof(Texts.ViewSensitivityDesc), Order = 11, ResourceType = typeof(Texts))]
    [AnimationSlider("F1", "%", -400d, 400d)]
    [AutomaticAngleVisible]
    public Animation ViewSensitivity { get; } = new(100, -400, 400);

    [Display(GroupName = nameof(Texts.ViewGroup), Name = nameof(Texts.ManualAngleName), Description = nameof(Texts.ManualAngleDesc), Order = 12, ResourceType = typeof(Texts))]
    [AnimationSlider("F1", "°", -180d, 180d)]
    [ManualAngleVisible]
    public Animation ManualAngle { get; } = new(0, -180, 180);

    [Display(GroupName = nameof(Texts.FoilGroup), Name = nameof(Texts.FoilColorName), Description = nameof(Texts.FoilColorDesc), Order = 20, ResourceType = typeof(Texts))]
    [ColorPicker]
    public Color FoilColor { get => foilColor; set => Set(ref foilColor, value); }
    Color foilColor = Color.FromRgb(232, 244, 255);

    [Display(GroupName = nameof(Texts.FoilGroup), Name = nameof(Texts.RainbowName), Description = nameof(Texts.RainbowDesc), Order = 21, ResourceType = typeof(Texts))]
    [AnimationSlider("F1", "%", 0d, 200d)]
    public Animation Rainbow { get; } = new(100, 0, 200);

    [Display(GroupName = nameof(Texts.FoilGroup), Name = nameof(Texts.ReliefName), Description = nameof(Texts.ReliefDesc), Order = 22, ResourceType = typeof(Texts))]
    [AnimationSlider("F1", "%", 0d, 300d)]
    public Animation Relief { get; } = new(80, 0, 300);

    [Display(GroupName = nameof(Texts.FoilGroup), Name = nameof(Texts.PatternScaleName), Description = nameof(Texts.PatternScaleDesc), Order = 23, ResourceType = typeof(Texts))]
    [AnimationSlider("F1", "%", 10d, 500d)]
    public Animation PatternScale { get; } = new(120, 10, 500);

    [Display(GroupName = nameof(Texts.FoilGroup), Name = nameof(Texts.PatternAngleName), Description = nameof(Texts.PatternAngleDesc), Order = 24, ResourceType = typeof(Texts))]
    [AnimationSlider("F1", "°", -180d, 180d)]
    public Animation PatternAngle { get; } = new(35, -180, 180);

    [Display(GroupName = nameof(Texts.FoilGroup), Name = nameof(Texts.SparkleName), Description = nameof(Texts.SparkleDesc), Order = 25, ResourceType = typeof(Texts))]
    [AnimationSlider("F1", "%", 0d, 200d)]
    public Animation Sparkle { get; } = new(45, 0, 200);

    [Display(GroupName = nameof(Texts.FoilGroup), Name = nameof(Texts.MotionName), Description = nameof(Texts.MotionDesc), Order = 26, ResourceType = typeof(Texts))]
    [AnimationSlider("F1", "%", -200d, 200d)]
    public Animation Motion { get; } = new(20, -200, 200);

    [Display(GroupName = nameof(Texts.RevealGroup), Name = nameof(Texts.RevealModeName), Description = nameof(Texts.RevealModeDesc), Order = 30, ResourceType = typeof(Texts))]
    [EnumComboBox]
    public HolographicFoilRevealMode RevealMode { get => revealMode; set => Set(ref revealMode, value); }
    HolographicFoilRevealMode revealMode = HolographicFoilRevealMode.None;

    [Display(GroupName = nameof(Texts.RevealGroup), Name = nameof(Texts.RevealStrengthName), Description = nameof(Texts.RevealStrengthDesc), Order = 31, ResourceType = typeof(Texts))]
    [AnimationSlider("F1", "%", 0d, 200d)]
    [AngleRevealVisible]
    public Animation RevealStrength { get; } = new(100, 0, 200);

    [Display(GroupName = nameof(Texts.RevealGroup), Name = nameof(Texts.RevealRangeName), Description = nameof(Texts.RevealRangeDesc), Order = 32, ResourceType = typeof(Texts))]
    [AnimationSlider("F1", "°", 1d, 180d)]
    [ImageAtlasVisible]
    public Animation RevealRange { get; } = new(90, 1, 180);

    [Display(GroupName = nameof(Texts.RevealGroup), Name = nameof(Texts.RevealBlendName), Description = nameof(Texts.RevealBlendDesc), Order = 33, ResourceType = typeof(Texts))]
    [AnimationSlider("F2", "", 0d, 1d)]
    [ImageAtlasVisible]
    public Animation RevealBlend { get; } = new(0.35, 0, 1);

    [Display(GroupName = nameof(Texts.AtlasGroup), Name = nameof(Texts.RevealImageName), Description = nameof(Texts.RevealImageDesc), Order = 40, ResourceType = typeof(Texts))]
    [FileSelector(FileGroupType.Texture, ShowThumbnail = true, CustomFilterName = nameof(Texts.ImageFileFilter), CustomFilterValue = "*.png;*.jpg;*.jpeg;*.bmp;*.webp;*.gif;*.tif;*.tiff", ResourceType = typeof(Texts))]
    [ImageAtlasVisible]
    public string? RevealImage { get => revealImage; set => Set(ref revealImage, value); }
    string? revealImage;

    [Display(GroupName = nameof(Texts.AtlasGroup), Name = nameof(Texts.RevealFramesName), Description = nameof(Texts.RevealFramesDesc), Order = 41, ResourceType = typeof(Texts))]
    [Range(2, 16)]
    [DefaultValue(3)]
    [TextBoxSlider("F0", "", 2, 16)]
    [ImageAtlasVisible]
    public int RevealFrames { get => revealFrames; set => Set(ref revealFrames, Math.Clamp(value, 2, 16)); }
    int revealFrames = 3;

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];

    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices) => new HolographicFoilEffectProcessor(devices, this);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Amount, ViewSensitivity, ManualAngle, Rainbow, Relief, PatternScale, PatternAngle, Sparkle, Motion, RevealStrength, RevealRange, RevealBlend];

    public override IEnumerable<string> GetFiles()
    {
        if (!string.IsNullOrWhiteSpace(RevealImage))
            yield return RevealImage;
    }

    public override void ReplaceFile(string from, string to)
    {
        if (RevealImage == from)
            RevealImage = to;
    }

    public override IEnumerable<TimelineResource> GetResources()
    {
        if (TimelineResource.TryParseFromPath(RevealImage, TimelineResourceType.Image, out var resource))
            yield return resource;
    }
}
