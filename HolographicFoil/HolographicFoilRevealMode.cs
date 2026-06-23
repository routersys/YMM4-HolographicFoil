using System.ComponentModel.DataAnnotations;

namespace HolographicFoil;

public enum HolographicFoilRevealMode
{
    [Display(Name = nameof(Texts.RevealModeNone), ResourceType = typeof(Texts))]
    None,
    [Display(Name = nameof(Texts.RevealModeGuilloche), ResourceType = typeof(Texts))]
    Guilloche,
    [Display(Name = nameof(Texts.RevealModeImageAtlas), ResourceType = typeof(Texts))]
    ImageAtlas,
}
