namespace SearchAThing.OpenGL.Core;

public static partial class Ext
{

    /// <summary>
    /// Extract basex (m1[1..3]) from the given world matrix.
    /// </summary>    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 BaseX(this in Matrix4x4 m) => new Vector3(m.M11, m.M12, m.M13);

    /// <summary>
    /// Extract basey (m2[1..3]) from the given world matrix.
    /// </summary>    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 BaseY(this in Matrix4x4 m) => new Vector3(m.M21, m.M22, m.M23);

    /// <summary>
    /// Extract basez (m3[1..3]) from the given world matrix.
    /// </summary>    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 BaseZ(this in Matrix4x4 m) => new Vector3(m.M31, m.M32, m.M33);

    /// <summary>
    /// Extract the origin (m4[1..3]) from the given world matrix.
    /// </summary>    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Origin(this in Matrix4x4 m) => new Vector3(m.M41, m.M42, m.M43);

    /// <summary>
    /// Create a new world matrix copy of the given one but with origin set to given one.
    /// </summary>
    /// <param name="m">Input world matrix.</param>
    /// <param name="origin">New origin.</param>
    /// <returns>Origin repositioned world matrix.</returns>
    public static Matrix4x4 SetOrigin(this in Matrix4x4 m, Vector3 origin) =>
        MakeCS(origin, m.BaseX(), m.BaseY(), m.BaseZ());

    /// <summary>
    /// Create a new world matrix copy of the given one but with origin moved of given delta.
    /// </summary>
    /// <param name="m">Input world matrix.</param>
    /// <param name="delta">Delta to apply to the origin.</param>
    /// <returns>Origin moved world matrix.</returns>
    public static Matrix4x4 Move(this Matrix4x4 m, Vector3? delta = null) =>
        delta is null ? m : MakeCS(m.Origin() + delta.Value, m.BaseX(), m.BaseY(), m.BaseZ());

    /// <summary>
    /// Create a new world matrix copy of the given one but with origin moved of given delta.
    /// </summary>
    /// <param name="m">Input world matrix.</param>        
    /// <param name="dx">Delta (x) to apply to the origin.</param>
    /// <param name="dy">Delta (y) to apply to the origin.</param>
    /// <param name="dz">Delta (z) to apply to the origin.</param>
    /// <returns>Origin moved world matrix.</returns>
    public static Matrix4x4 Move(this Matrix4x4 m, float dx, float dy, float dz) =>
        MakeCS(m.Origin() + new Vector3(dx, dy, dz), m.BaseX(), m.BaseY(), m.BaseZ());

    /// <summary>
    /// Create a new world matrix copy of the given one but with x flipped ( then also the z ).
    /// </summary>
    /// <param name="m">Input world matrix.</param>    
    public static Matrix4x4 FlipX(this Matrix4x4 m) =>
        MakeCS(m.Origin(), -m.BaseX(), m.BaseY(), -m.BaseZ());

    /// <summary>
    /// Create a new world matrix copy of the given one but with y flipped ( then also the z ).
    /// </summary>
    /// <param name="m">Input world matrix.</param>    
    public static Matrix4x4 FlipY(this Matrix4x4 m) =>
        MakeCS(m.Origin(), m.BaseX(), -m.BaseY(), -m.BaseZ());

    /// <summary>
    /// Create a new world matrix copy of the given one but with z flipped ( then also the x ).
    /// </summary>
    /// <param name="m">Input world matrix.</param>    
    public static Matrix4x4 FlipZ(this Matrix4x4 m) =>
        MakeCS(m.Origin(), -m.BaseX(), m.BaseY(), -m.BaseZ());

    /// <summary>
    /// Compute the inverse of the given 4x4 matrix.
    /// </summary>    
    /// <param name="m">Input matrix.</param>
    /// <returns>Inverse matrix.</returns>
    public static Matrix4x4 Inverse(this Matrix4x4 m)
    {
        Matrix4x4 invMat;
        if (!Matrix4x4.Invert(m, out invMat))
            throw new System.Exception($"unable to get inverse matrix");
        return invMat;
    }

    /// <summary>
    /// Format to string given matrix.
    /// </summary>
    /// <param name="m">Input matrix.</param>
    /// <param name="dec">Decimal places to show in formatted string.</param>
    /// <param name="width">Nr. of characters foreach component.</param>
    /// <returns>String formatted representation of the matrix.</returns>
    public static string Fmt(this Matrix4x4 m, int dec, int width = 10)
    {
        var sb = new StringBuilder();

        var fmt = $"{{0,{width}: 0." + "0".Repeat(dec) + ";-0." + "0".Repeat(dec) + "}";

        sb.AppendFormat(fmt, m.M11); sb.Append(" ");
        sb.AppendFormat(fmt, m.M12); sb.Append(" ");
        sb.AppendFormat(fmt, m.M13); sb.Append(" ");
        sb.AppendFormat(fmt, m.M14); sb.AppendLine();

        sb.AppendFormat(fmt, m.M21); sb.Append(" ");
        sb.AppendFormat(fmt, m.M22); sb.Append(" ");
        sb.AppendFormat(fmt, m.M23); sb.Append(" ");
        sb.AppendFormat(fmt, m.M24); sb.AppendLine();

        sb.AppendFormat(fmt, m.M31); sb.Append(" ");
        sb.AppendFormat(fmt, m.M32); sb.Append(" ");
        sb.AppendFormat(fmt, m.M33); sb.Append(" ");
        sb.AppendFormat(fmt, m.M34); sb.AppendLine();

        sb.AppendFormat(fmt, m.M41); sb.Append(" ");
        sb.AppendFormat(fmt, m.M42); sb.Append(" ");
        sb.AppendFormat(fmt, m.M43); sb.Append(" ");
        sb.AppendFormat(fmt, m.M44); sb.AppendLine();

        return sb.ToString();
    }
}

public static partial class Toolkit
{

    /// <summary>
    /// Build world matrix using Arbitrary Axis Algorithm.
    /// </summary>
    /// <remarks>
    /// Origin has to be specified apart ( <see cref="MakeCS"/> ).<br/>
    /// References: https://images.autodesk.com/adsk/files/autocad_2012_pdf_dxf-reference_enu.pdf ( page 252 )
    /// </remarks>
    /// <param name="n">Normalized normal vector to which based the AAA construction.</param>
    /// <param name="x">Computed xbase..</param>
    /// <param name="y">Computed ybase.</param>
    /// <param name="z">Computed zbase.</param>
    public static void ArbitraryAxisAlghorithm(Vector3 n, out Vector3 x, out Vector3 y, out Vector3 z)
    {
        if (Abs(n.X) < aaaSmall && Abs(n.Y) < aaaSmall)
            x = Vector3.Cross(Vector3.UnitY, n);
        else
            x = Vector3.Cross(Vector3.UnitZ, n);

        y = Vector3.Normalize(Vector3.Cross(n, x));
        z = Vector3.Normalize(Vector3.Cross(x, y));
    }

    /// <summary>
    /// Build world matrix using <see cref="ArbitraryAxisAlghorithm"/> and sets the origin to the given one.
    /// </summary>
    /// <param name="origin">Origin to set to the created world matrix.</param>
    /// <param name="N">Normalized normal vector.</param>
    /// <returns>World matrix.</returns>
    public static Matrix4x4 MakeCS(Vector3 origin, Vector3 N)
    {
        ArbitraryAxisAlghorithm(N, out var baseX, out var baseY, out var baseZ);

        return new Matrix4x4(
            baseX.X, baseX.Y, baseX.Z, 0, // WorldAxisX
            baseY.X, baseY.Y, baseY.Z, 0, // WorldAxisY        
            baseZ.X, baseZ.Y, baseZ.Z, 0, // WorldAxisZ
            origin.X, origin.Y, origin.Z, 1); // WorldPos
    }

    /// <summary>
    /// Create a right-hand rule world matrix with given origin and normalized orthonormal x, y base vectors.
    /// </summary>
    /// <param name="origin">World origin.</param>
    /// <param name="baseX">World basex.</param>
    /// <param name="baseY">World basey.</param>
    /// <returns>World matrix.</returns>
    public static Matrix4x4 MakeCS(Vector3 origin, Vector3 baseX, Vector3 baseY)
    {
        var baseZ = Vector3.Normalize(Vector3.Cross(baseX, baseY));

        return new Matrix4x4(
            baseX.X, baseX.Y, baseX.Z, 0, // WorldAxisX
            baseY.X, baseY.Y, baseY.Z, 0, // WorldAxisY        
            baseZ.X, baseZ.Y, baseZ.Z, 0, // WorldAxisZ
            origin.X, origin.Y, origin.Z, 1); // WorldPos
    }

    /// <summary>
    /// Create world matrix with given origin and normalized orthonormal x, y, z base vectors.
    /// </summary>
    /// <param name="origin">World origin.</param>
    /// <param name="baseX">World basex.</param>
    /// <param name="baseY">World basey.</param>
    /// <param name="baseZ">World basez.</param>
    /// <returns>World matrix.</returns>
    public static Matrix4x4 MakeCS(Vector3 origin, Vector3 baseX, Vector3 baseY, Vector3 baseZ) =>
        new Matrix4x4(
            baseX.X, baseX.Y, baseX.Z, 0, // WorldAxisX
            baseY.X, baseY.Y, baseY.Z, 0, // WorldAxisY        
            baseZ.X, baseZ.Y, baseZ.Z, 0, // WorldAxisZ
            origin.X, origin.Y, origin.Z, 1); // WorldPos    

    /// <summary>
    /// Create world matrix using given origin and basex=(1,0,0), basey=(0,1,0), basez=(0,0,1).
    /// </summary>
    /// <param name="origin">World origin.</param>
    /// <returns>World matrix.</returns>
    public static Matrix4x4 MakeWCS(Vector3? origin = null) =>
        MakeCS(origin is null ? Vector3.Zero : origin.Value, Vector3.UnitX, Vector3.UnitY);

    /// <summary>
    /// Create world matrix using given origin and basex=(1,0,0), basey=(0,1,0), basez=(0,0,1).
    /// </summary>        
    /// <param name="ox">World x origin.</param>
    /// <param name="oy">World y origin.</param>
    /// <param name="oz">World z origin.</param>
    /// <returns>World matrix.</returns>        
    public static Matrix4x4 MakeWCS(float ox, float oy, float oz) =>
        MakeCS(new Vector3(ox, oy, oz), Vector3.UnitX, Vector3.UnitY);

    /// <summary>
    /// Create a screen matrix using given origin.<br/>
    /// That is the same of a world matrix with y flipped like the screen coordinates are with (0,0) on (left,top).
    /// </summary>
    /// <param name="origin">Screen origin.</param>
    /// <returns>Screen matrix.</returns>
    public static Matrix4x4 MakeScreenCS(Vector3? origin = null) => WCS.Move(origin).FlipY();

    /// <summary>
    /// Detect basex from given normalized basey and basez vectors using right-hand rule.
    /// </summary>
    /// <param name="y">Basey versor.</param>
    /// <param name="z">Basez versor.</param>
    /// <returns>Basex versor.</returns>
    public static Vector3 RightHandXFromYZ(Vector3 y, Vector3 z) => Vector3.Cross(y, z);

    /// <summary>
    /// Make a world matrix that represent the one used by the camera [world].<br/>
    /// Basey = Normalized(cameraUp)<br/>
    /// Basez = Normalized(cameraPos - cameraTarget)<br/>
    /// </summary>
    /// <param name="cameraPos">Camera position [world].</param>
    /// <param name="cameraTarget">Camera target [world].</param>
    /// <param name="cameraUp">Camera up versor [world].</param>
    /// <returns></returns>
    public static Matrix4x4 MakeCCS(Vector3 cameraPos, Vector3 cameraTarget, Vector3 cameraUp)
    {
        var baseZ = Vector3.Normalize(cameraPos - cameraTarget);
        var baseY = Vector3.Normalize(cameraUp);
        var baseX = RightHandXFromYZ(baseY, baseZ);
        var origin = cameraPos;

        return new Matrix4x4(
            baseX.X, baseX.Y, baseX.Z, 0, // WorldAxisX
            baseY.X, baseY.Y, baseY.Z, 0, // WorldAxisY        
            baseZ.X, baseZ.Y, baseZ.Z, 0, // WorldAxisZ
            origin.X, origin.Y, origin.Z, 1); // WorldPos
    }

}