using System.ComponentModel.DataAnnotations;

namespace HolographicFoil;

public enum HolographicFoilAlphaSource
{
    [Display(Name = nameof(Texts.AlphaSourceAlpha), ResourceType = typeof(Texts))]
    Alpha,
    [Display(Name = nameof(Texts.AlphaSourceLuminance), ResourceType = typeof(Texts))]
    Luminance,
    [Display(Name = nameof(Texts.AlphaSourceAlphaLuminance), ResourceType = typeof(Texts))]
    AlphaLuminance,
}
