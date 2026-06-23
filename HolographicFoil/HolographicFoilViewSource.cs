using System.ComponentModel.DataAnnotations;

namespace HolographicFoil;

public enum HolographicFoilViewSource
{
    [Display(Name = nameof(Texts.ViewSourceCamera), ResourceType = typeof(Texts))]
    Camera,
    [Display(Name = nameof(Texts.ViewSourceItemRotation), ResourceType = typeof(Texts))]
    ItemRotation,
    [Display(Name = nameof(Texts.ViewSourceManual), ResourceType = typeof(Texts))]
    Manual,
}
