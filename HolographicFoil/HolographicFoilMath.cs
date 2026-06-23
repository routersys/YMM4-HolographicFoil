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

    public static double ComputeCameraYawDegrees(Matrix4x4 camera)
    {
        var forward = Vector3.TransformNormal(Vector3.UnitZ, camera);
        if (forward.LengthSquared() <= 1e-12f)
            return 0d;
        forward = Vector3.Normalize(forward);
        return NormalizeAngleDegrees(Math.Atan2(forward.X, forward.Z) * 180d / Math.PI);
    }
}
