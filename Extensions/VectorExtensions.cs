using System;
using System.Numerics;

public static class VectorExtensions
{
    public static (byte R, byte G, byte B) ToSRGB(this in Vector3 _c)
    {
        const float GAMMA = 1 / 2.2f;
        var rngLocal = Random.ThreadLocal;
        return (
            (byte)Math.Min(255, MathF.Pow(_c.X, GAMMA) * 255f + rngLocal.NextDouble()),
            (byte)Math.Min(255, MathF.Pow(_c.Y, GAMMA) * 255f + rngLocal.NextDouble()),
            (byte)Math.Min(255, MathF.Pow(_c.Z, GAMMA) * 255f + rngLocal.NextDouble()));
    }

    public static (Vector3 Right, Vector3 Up) ToCoordinateSystem(this Vector3 _zAxis)
    {
        //Tom Duff, James Burgess, Per Christensen, Christophe Hery, Andrew Kensler,
        //Max Liani, and Ryusuke Villemin, Building an Orthonormal Basis, Revisited,
        //Journal of Computer Graphics Techniques(JCGT), vol. 6, no. 1, 1 - 8, 2017
        //Available online http://jcgt.org/published/0006/01/01/

        var sign = MathF.CopySign(1.0f, _zAxis.Z);
        var a = -1.0f / (sign + _zAxis.Z);
        var b = _zAxis.X * _zAxis.Y * a;

        return (
            new Vector3(-1.0f - sign * _zAxis.X * _zAxis.X * a, -sign * b, sign * _zAxis.X),
            new Vector3(-b, -sign - _zAxis.Y * _zAxis.Y * a, _zAxis.Y)
        );
    }

    public static float ComputeLuminance(this Vector3 _color)
        => Vector3.Dot(_color, new Vector3(0.2126f, 0.7152f, 0.0722f));
}
