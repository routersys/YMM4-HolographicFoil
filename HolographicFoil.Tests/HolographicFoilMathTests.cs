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

    [Theory]
    [InlineData(-90, 90, 5, 0)]
    [InlineData(-45, 90, 5, 0)]
    [InlineData(0, 90, 5, 2)]
    [InlineData(45, 90, 5, 4)]
    [InlineData(90, 90, 5, 4)]
    public void ComputeRevealCoordinate_ClampsToConfiguredRange(double angle, double range, int frames, double expected)
    {
        Assert.Equal(expected, HolographicFoilMath.ComputeRevealCoordinate(angle, range, frames), 6);
    }

    [Fact]
    public void ComputeFrameWeights_ProducesPartitionOfUnity()
    {
        Span<float> weights = stackalloc float[7];

        for (var coordinate = -1d; coordinate <= 7d; coordinate += 0.125d)
        {
            HolographicFoilMath.ComputeFrameWeights(coordinate, 7, 0.6d, weights);
            var sum = 0d;
            for (var i = 0; i < 7; i++)
            {
                Assert.InRange(weights[i], 0f, 1f);
                sum += weights[i];
            }
            Assert.Equal(1d, sum, 6);
        }
    }

    [Fact]
    public void ComputeFrameWeights_UsesNearestFrameWithoutBlend()
    {
        Span<float> weights = stackalloc float[4];
        HolographicFoilMath.ComputeFrameWeights(1.6d, 4, 0d, weights);

        Assert.Equal(0f, weights[0]);
        Assert.Equal(0f, weights[1]);
        Assert.Equal(1f, weights[2]);
        Assert.Equal(0f, weights[3]);
    }

    [Fact]
    public void EvaluateRgbCycle_IsBoundedAndPeriodic()
    {
        for (var i = -32; i <= 32; i++)
        {
            var phase = i / 8d;
            var color = HolographicFoilMath.EvaluateRgbCycle(phase);
            Assert.InRange(color.X, 0f, 1f);
            Assert.InRange(color.Y, 0f, 1f);
            Assert.InRange(color.Z, 0f, 1f);
            Assert.Equal(color.X, HolographicFoilMath.EvaluateRgbCycle(phase + 1d).X, 6);
            Assert.Equal(color.Y, HolographicFoilMath.EvaluateRgbCycle(phase + 1d).Y, 6);
            Assert.Equal(color.Z, HolographicFoilMath.EvaluateRgbCycle(phase + 1d).Z, 6);
        }
    }

    [Fact]
    public void ComputeCameraYawDegrees_UsesTransformedForwardVector()
    {
        Assert.Equal(0d, HolographicFoilMath.ComputeCameraYawDegrees(Matrix4x4.Identity), 6);

        var rotated = Matrix4x4.CreateRotationY(MathF.PI / 2f);
        Assert.Equal(90d, HolographicFoilMath.ComputeCameraYawDegrees(rotated), 4);
    }
}
