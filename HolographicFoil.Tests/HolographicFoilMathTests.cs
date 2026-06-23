using System.Numerics;

namespace HolographicFoil.Tests;

public sealed class HolographicFoilMathTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(180, 180)]
    [InlineData(181, -179)]
    [InlineData(-181, 179)]
    [InlineData(540, 180)]
    [InlineData(double.NaN, 0)]
    public void NormalizeAngleDegrees_ReturnsPrincipalValue(double angle, double expected)
    {
        Assert.Equal(expected, HolographicFoilMath.NormalizeAngleDegrees(angle), 6);
    }

    [Fact]
    public void ComputeCameraYawDegrees_UsesTransformedForwardVector()
    {
        Assert.Equal(0d, HolographicFoilMath.ComputeCameraYawDegrees(Matrix4x4.Identity), 6);

        var rotated = Matrix4x4.CreateRotationY(MathF.PI / 2f);
        Assert.Equal(90d, HolographicFoilMath.ComputeCameraYawDegrees(rotated), 4);
    }
}
