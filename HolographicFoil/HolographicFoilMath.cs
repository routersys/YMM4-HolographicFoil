using System.Numerics;

namespace HolographicFoil;

internal static class HolographicFoilMath
{
    public static double NormalizeAngleDegrees(double angle)
    {
        if (!double.IsFinite(angle))
            return 0d;
        var normalized = angle % 360d;
        if (normalized > 180d)
            normalized -= 360d;
        if (normalized < -180d)
            normalized += 360d;
        return normalized;
    }

    public static double ComputeRevealCoordinate(double angleDegrees, double rangeDegrees, int frameCount)
    {
        if (frameCount <= 1)
            return 0d;

        var range = Math.Max(Math.Abs(rangeDegrees), 1e-6d);
        var half = range * 0.5d;
        var normalized = (NormalizeAngleDegrees(angleDegrees) + half) / range;
        return Math.Clamp(normalized, 0d, 1d) * (frameCount - 1);
    }

    public static void ComputeFrameWeights(double frameCoordinate, int frameCount, double blendWidth, Span<float> weights)
    {
        weights.Clear();
        if (frameCount <= 0 || weights.Length < frameCount)
            return;
        if (frameCount == 1)
        {
            weights[0] = 1f;
            return;
        }

        var coordinate = double.IsFinite(frameCoordinate) ? Math.Clamp(frameCoordinate, 0d, frameCount - 1d) : 0d;
        if (blendWidth <= 0d)
        {
            weights[(int)Math.Round(coordinate)] = 1f;
            return;
        }

        var lower = (int)Math.Floor(coordinate);
        var upper = Math.Min(lower + 1, frameCount - 1);
        if (lower == upper)
        {
            weights[lower] = 1f;
            return;
        }

        var fraction = coordinate - lower;
        var width = Math.Clamp(blendWidth, 1e-6d, 1d);
        var t = SmoothStep(0.5d - width * 0.5d, 0.5d + width * 0.5d, fraction);
        weights[lower] = (float)(1d - t);
        weights[upper] = (float)t;
    }

    public static Vector3 EvaluateRgbCycle(double phase)
    {
        if (!double.IsFinite(phase))
            phase = 0d;

        var p = phase - Math.Floor(phase);
        return new Vector3(
            (float)(0.5d + 0.5d * Math.Cos(Math.Tau * p)),
            (float)(0.5d + 0.5d * Math.Cos(Math.Tau * (p - 1d / 3d))),
            (float)(0.5d + 0.5d * Math.Cos(Math.Tau * (p - 2d / 3d))));
    }

    public static double ComputeCameraYawDegrees(Matrix4x4 camera)
    {
        var forward = Vector3.TransformNormal(Vector3.UnitZ, camera);
        if (forward.LengthSquared() <= 1e-12f)
            return 0d;
        forward = Vector3.Normalize(forward);
        return NormalizeAngleDegrees(Math.Atan2(forward.X, forward.Z) * 180d / Math.PI);
    }

    static double SmoothStep(double edge0, double edge1, double value)
    {
        if (edge0 == edge1)
            return value < edge0 ? 0d : 1d;
        var x = Math.Clamp((value - edge0) / (edge1 - edge0), 0d, 1d);
        return x * x * (3d - 2d * x);
    }
}
