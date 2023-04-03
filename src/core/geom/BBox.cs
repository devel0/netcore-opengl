namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Bounding box of a set of points.<br/>
/// It can be an oriented bounding box if created with custom coordinate system, <see cref="WCS"/> elsewhere is used as default.
/// </summary>
/// <remarks>
/// If a custom CS is given at the constructor min and max point of the bbox evaluates extension increments
/// from the cs point of view.
/// </remarks>
public class BBox
{

    Vector3? _Min = null;

    /// <summary>
    /// Minimum coordinate [wcs].
    /// </summary>    
    public Vector3 Min
    {
        get
        {
            if (_Min is null) throw new Exception("empty bbox");
            return _Min.Value;
        }
        private set
        {
            _Min = value;
        }
    }

    /// <summary>
    /// Minimum coordinate [cs].
    /// </summary>
    public Vector3 MinCS => Min.ToUCS(CS is null ? WCS : CS.Value);

    Vector3? _Max = null;
    /// <summary>
    /// Maximum coordinate [wcs].
    /// </summary>    
    public Vector3 Max
    {
        get
        {
            if (_Max is null) throw new Exception("empty bbox");
            return _Max.Value;
        }
        private set
        {
            _Max = value;
        }
    }

    /// <summary>
    /// Maximum coordinate [cs].
    /// </summary>
    public Vector3 MaxCS => Max.ToUCS(CS is null ? WCS : CS.Value);

    /// <summary>
    /// States if bounding box is empty. This is the default state at construct time and mean no point added yet.
    /// </summary>
    public bool IsEmpty => _Min is null;

    /// <summary>
    /// Bounding box middle coordinate [wcs].
    /// </summary>    
    public Vector3 Middle => (Min + Max) / 2;

    /// <summary>
    /// Bounding box size [wcs].
    /// </summary>
    public Vector3 Size => Max - Min;

    /// <summary>
    /// Bounding box size [cs].
    /// </summary>
    public Vector3 SizeCS => MaxCS - MinCS;

    /// <summary>
    /// Coordinate system related to the bounding box or null if <see cref="WCS"/> is considered.
    /// </summary>    
    public Matrix4x4? CS { get; private set; } = null;

    /// <summary>
    /// Create bounding box object with optional coordinate system.
    /// </summary>
    /// <param name="cs">Optional coordinate system ( if null <see cref="WCS"/> is considered )</param>
    public BBox(Matrix4x4? cs = null)
    {
        CS = cs;
    }

    /// <summary>
    /// Create a wcs bounding box of given set of points.
    /// </summary>    
    public BBox(params Vector3[] pts) : this((IEnumerable<Vector3>)pts) { }

    /// <summary>
    /// Create a cs oriented bounding box of given set of points.
    /// </summary>    
    public BBox(Matrix4x4? cs, params Vector3[] pts) : this((IEnumerable<Vector3>)pts) { }

    /// <summary>
    /// Create a wcs bounding box of given points
    /// </summary>    
    public BBox(IEnumerable<Vector3> pts) => ApplyUnion(pts);

    /// <summary>
    /// Create a cs oriente bounding box of given set of points.
    /// </summary>    
    public BBox(Matrix4x4 cs, IEnumerable<Vector3> pts) : this(cs) => ApplyUnion(pts);

    /// <summary>
    /// Create a wcs bounding box adding given bboxes wcs extensions.
    /// </summary>    
    public BBox(IEnumerable<BBox> bboxes) =>
        ApplyUnion(bboxes.Where(bbox => !bbox.IsEmpty).SelectMany(bbox => bbox.Points));

    /// <summary>
    /// Create a cs oriented bounding box adding given bboxes wcs extensions.
    /// </summary>    
    public BBox(Matrix4x4 cs, IEnumerable<BBox> bboxes) : this(cs) =>
        ApplyUnion(bboxes.Where(bbox => !bbox.IsEmpty).SelectMany(bbox => bbox.Points));

    /// <summary>
    /// (side effect) Modify current bounding box by adding given other bbox wcs extensions.
    /// </summary>
    /// <param name="other">Other bbox to add.</param>
    /// <returns>True if bbox changed.</returns>
    public bool ApplyUnion(BBox other) => ApplyUnion(other.Points);

    /// <summary>
    /// (side effect) Modify current bounding box by adding given other set ot wcs points.
    /// </summary>
    /// <param name="pts">Other wcs points to add.</param>
    /// <returns>True if bbox changed.</returns>
    public bool ApplyUnion(params Vector3[] pts) => ApplyUnion((IEnumerable<Vector3>)pts);

    /// <summary>
    /// (side effect)
    /// Apply union of given wcs points.
    /// If bbox provided with custom CS then bbox aligned comparision will be done.
    /// </summary>
    /// <param name="pts">Other points to add.</param>
    /// <returns>True if bbox changed.</returns>
    public bool ApplyUnion(IEnumerable<Vector3> pts)
    {
        float xmin, ymin, zmin, xmax, ymax, zmax;
        var wasEmpty = IsEmpty;

        bool assignFromFirstPt;

        if (IsEmpty)
        {
            assignFromFirstPt = true;
            xmin = ymin = zmin = xmax = ymax = zmax = 0;
        }
        else
        {
            assignFromFirstPt = false;
            xmin = Min.X; ymin = Min.Y; zmin = Min.Z;
            xmax = Max.X; ymax = Max.Y; zmax = Max.Z;
        }

        foreach (var p in pts)
        {
            if (assignFromFirstPt)
            {
                xmin = xmax = p.X;
                ymin = ymax = p.Y;
                zmin = zmax = p.Z;
                assignFromFirstPt = false;
            }
            else
            {
                if (CS is not null)
                {
                    var minCS = new Vector3(xmin, ymin, zmin).ToUCS(CS.Value);
                    var maxCS = new Vector3(xmax, ymax, zmax).ToUCS(CS.Value);

                    var pCS = p.ToUCS(CS.Value);

                    var xminCS = Min(minCS.X, pCS.X);
                    var yminCS = Min(minCS.Y, pCS.Y);
                    var zminCS = Min(minCS.Z, pCS.Z);

                    var xmaxCS = Max(maxCS.X, pCS.X);
                    var ymaxCS = Max(maxCS.Y, pCS.Y);
                    var zmaxCS = Max(maxCS.Z, pCS.Z);

                    var min = new Vector3(xminCS, yminCS, zminCS).ToWCS(CS.Value);
                    var max = new Vector3(xmaxCS, ymaxCS, zmaxCS).ToWCS(CS.Value);

                    xmin = min.X;
                    ymin = min.Y;
                    zmin = min.Z;

                    xmax = max.X;
                    ymax = max.Y;
                    zmax = max.Z;
                }
                else
                {
                    xmin = Min(xmin, p.X);
                    ymin = Min(ymin, p.Y);
                    zmin = Min(zmin, p.Z);

                    xmax = Max(xmax, p.X);
                    ymax = Max(ymax, p.Y);
                    zmax = Max(zmax, p.Z);
                }
            }
        }

        var changed = false;

        if (IsEmpty)
            changed = !wasEmpty;
        else
            changed =
                xmin != Min.X || ymin != Min.Y || zmin != Min.Z ||
                xmax != Max.X || ymax != Max.Y || zmax != Max.Z;

        Min = new Vector3(xmin, ymin, zmin);
        Max = new Vector3(xmax, ymax, zmax);

        return changed;
    }

    /// <summary>
    /// Reset bbox to empty state.
    /// </summary>
    public void Clear()
    {
        _Min = _Max = null;
    }

    /// <summary>
    /// Create a copy of given bbox.
    /// </summary>
    /// <param name="other">Other bbox to copy.</param>
    public BBox(BBox other)
    {
        if (!other.IsEmpty)
        {
            Min = other.Min;
            Max = other.Max;
        }
    }

    /// <summary>
    /// Create a new bounding box that is the result of the current with addition of given wcs point.
    /// </summary>
    /// <param name="p">WCS point to add.</param>
    /// <returns>Extended bbox.</returns>
    public BBox Union(Vector3 p)
    {
        var res = new BBox(this);

        res.ApplyUnion(p);

        return res;
    }

    /// <summary>
    /// Create a new bounding box that is the result of the current with addition of given wcs point set.
    /// </summary>
    /// <param name="pts">WCS point set to add.</param>
    /// <returns>Extended bbox.</returns>
    public BBox Union(IEnumerable<Vector3> pts)
    {
        var res = new BBox(this);

        res.ApplyUnion(pts);

        return res;
    }

    /// <summary>
    /// Create a new bounding box that is the result of the current with addition of the extensions of given other bbox.
    /// </summary>
    /// <param name="other">Other bbox to add.</param>
    /// <returns>Extended bbox.</returns>
    public BBox Union(BBox other)
    {
        if (IsEmpty) return other;
        if (other.IsEmpty) return this;

        return this.Union(other.Min).Union(other.Max);
    }

    /// <summary>
    /// States if this bbox equals (with tolerance) to another.
    /// </summary>
    /// <param name="tol">Comparision tolerance.</param>
    /// <param name="other">Other bbox to compare to this one.</param>
    /// <returns>True if two bbox are equals (with tolerance).</returns>
    public bool EqualsTol(float tol, BBox other)
    {
        if (IsEmpty) return other.IsEmpty;
        if (other.IsEmpty) return false;
        return Min.EqualsTol(tol, other.Min) && Max.EqualsTol(tol, other.Max);
    }

    /// <summary>
    /// Create a new bounding box with Min, Max moved by the given wcs delta.
    /// </summary>
    /// <param name="delta">WCS delta to apply.</param>
    /// <returns>Moved bounding box.</returns>
    public BBox Move(Vector3 delta) => new BBox(CS, Min + delta, Max + delta);

    /// <summary>
    /// WCS tests if given other bounding box is contained (with tolerance) to this one.
    /// </summary>
    /// <param name="tol">Comparision tolerance.</param>
    /// <param name="other">Other bbox to test if contained into this one.</param>
    /// <param name="strictly">If true, test will fails if one wcs ordinate of other bbox extensions equals to this one.</param>
    /// <param name="testZ">If false tests acts only for x, y bounding box extension components.</param>
    /// <returns>True if test succeeded.</returns>
    public bool Contains(float tol, BBox other, bool strictly = false, bool testZ = false)
    {
        if (IsEmpty) return false;

        if (other.IsEmpty) return true;

        return
            strictly
            ?
            other.Min.X.GreatThanTol(tol, Min.X) &&
            other.Min.Y.GreatThanTol(tol, Min.Y) &&
            (testZ ? other.Min.Z.GreatThanTol(tol, Min.Z) : true) &&
            other.Max.X.LessThanTol(tol, Max.X) &&
            other.Max.Y.LessThanTol(tol, Max.Y) &&
            (testZ ? other.Max.Z.LessThanTol(tol, Max.Z) : true)
            :
            other.Min.X.GreatThanOrEqualsTol(tol, Min.X) &&
            other.Min.Y.GreatThanOrEqualsTol(tol, Min.Y) &&
            (testZ ? other.Min.Z.GreatThanOrEqualsTol(tol, Min.Z) : true) &&
            other.Max.X.LessThanOrEqualsTol(tol, Max.X) &&
            other.Max.Y.LessThanOrEqualsTol(tol, Max.Y) &&
            (testZ ? other.Max.Z.LessThanOrEqualsTol(tol, Max.Z) : true);
    }

    /// <summary>
    /// Enumerate bounding box eight points.
    /// </summary>
    /// <remarks>
    /// It doesn't check for duplicates.
    /// </remarks>    
    public IEnumerable<Vector3> Points
    {
        get
        {
            yield return new Vector3(Min.X, Min.Y, Min.Z);
            yield return new Vector3(Max.X, Min.Y, Min.Z);
            yield return new Vector3(Max.X, Max.Y, Min.Z);
            yield return new Vector3(Min.X, Max.Y, Min.Z);

            yield return new Vector3(Min.X, Min.Y, Max.Z);
            yield return new Vector3(Max.X, Min.Y, Max.Z);
            yield return new Vector3(Max.X, Max.Y, Max.Z);
            yield return new Vector3(Min.X, Max.Y, Max.Z);
        }
    }

    /// <summary>
    /// Create a new bbox that is the given one with extensions transformed by the given transform matrix.
    /// </summary>
    /// <param name="m">Transform matrix.</param>
    /// <returns>Transformed bbox.</returns>
    public BBox Transform(Matrix4x4 m) => new BBox(Points.Select(pt => Vector3.Transform(pt, m)));

    public override string ToString() => IsEmpty ? "Empty" : $"Min:{Min} Max:{Max} Δ:{Size}";

}

public static partial class Ext
{

    /// <summary>
    /// Create wcs lines figure that represents the given bounding box.
    /// </summary>
    /// <param name="bbox">Bounding box for which create line figures.</param>
    /// <param name="color">(optional) color to apply created figure.</param>
    /// <returns>Lines figure.</returns>
    public static GLLineFigure MakeFigure(this BBox bbox, Color? color) =>
        new GLLineFigure(bbox.Lines(color));

    /// <summary>
    /// Create wcs gl lines that represents the given bounding box.
    /// </summary>
    /// <param name="bbox">Bounding box for which create gl lines.</param>
    /// <param name="color">(optional) color to apply created gl lines.</param>
    /// <returns>GL lines.</returns>
    public static IEnumerable<GLLine> Lines(this BBox bbox, Color? color = null)
    {
        GLLine fromTo(Vector3 from, Vector3 to) => from.LineTo(to, color);

        if (bbox.Size.X == 0)
        {
            if (bbox.Size.Y == 0 || bbox.Size.Z == 0)
                yield return fromTo(bbox.Min, bbox.Max);

            else // yz face
            {
                var p = bbox.Min;
                yield return fromTo(p, p += bbox.Size.Y * Vector3.UnitY);
                yield return fromTo(p, p += bbox.Size.Z * Vector3.UnitZ);
                yield return fromTo(p, p -= bbox.Size.Y * Vector3.UnitY);
                yield return fromTo(p, p -= bbox.Size.Z * Vector3.UnitZ);
            }
        }

        else if (bbox.Size.Y == 0)
        {
            if (bbox.Size.X == 0 || bbox.Size.Z == 0)
                yield return fromTo(bbox.Min, bbox.Max);

            else // xz face
            {
                var p = bbox.Min;
                yield return fromTo(p, p += bbox.Size.X * Vector3.UnitX);
                yield return fromTo(p, p += bbox.Size.Z * Vector3.UnitZ);
                yield return fromTo(p, p -= bbox.Size.X * Vector3.UnitX);
                yield return fromTo(p, p -= bbox.Size.Z * Vector3.UnitZ);
            }
        }

        else if (bbox.Size.Z == 0)
        {
            if (bbox.Size.X == 0 || bbox.Size.Y == 0)
                yield return fromTo(bbox.Min, bbox.Max);

            else // xy face
            {
                var p = bbox.Min;
                yield return fromTo(p, p += bbox.Size.X * Vector3.UnitX);
                yield return fromTo(p, p += bbox.Size.Y * Vector3.UnitY);
                yield return fromTo(p, p -= bbox.Size.X * Vector3.UnitX);
                yield return fromTo(p, p -= bbox.Size.Y * Vector3.UnitY);
            }
        }

        else
        {
            var p = bbox.Min;

            // xy(z-) face

            yield return fromTo(p, p += bbox.Size.X * Vector3.UnitX);
            yield return fromTo(p, p += bbox.Size.Y * Vector3.UnitY);
            yield return fromTo(p, p -= bbox.Size.X * Vector3.UnitX);
            yield return fromTo(p, p -= bbox.Size.Y * Vector3.UnitY);

            // xz(x-) face

            yield return fromTo(p, p += bbox.Size.X * Vector3.UnitX);
            yield return fromTo(p, p += bbox.Size.Z * Vector3.UnitZ);
            yield return fromTo(p, p -= bbox.Size.X * Vector3.UnitX);
            yield return fromTo(p, p -= bbox.Size.Z * Vector3.UnitZ);

            // yz(x-) face

            yield return fromTo(p, p += bbox.Size.Y * Vector3.UnitY);
            yield return fromTo(p, p += bbox.Size.Z * Vector3.UnitZ);
            yield return fromTo(p, p -= bbox.Size.Y * Vector3.UnitY);
            yield return fromTo(p, p -= bbox.Size.Z * Vector3.UnitZ);

            p = bbox.Max;

            // xy(z+) face

            yield return fromTo(p, p -= bbox.Size.X * Vector3.UnitX);
            yield return fromTo(p, p -= bbox.Size.Y * Vector3.UnitY);
            yield return fromTo(p, p += bbox.Size.X * Vector3.UnitX);
            yield return fromTo(p, p += bbox.Size.Y * Vector3.UnitY);

            // xz(x+) face

            yield return fromTo(p, p -= bbox.Size.X * Vector3.UnitX);
            yield return fromTo(p, p -= bbox.Size.Z * Vector3.UnitZ);
            yield return fromTo(p, p += bbox.Size.X * Vector3.UnitX);
            yield return fromTo(p, p += bbox.Size.Z * Vector3.UnitZ);

            // yz(x+) face

            yield return fromTo(p, p -= bbox.Size.Y * Vector3.UnitY);
            yield return fromTo(p, p -= bbox.Size.Z * Vector3.UnitZ);
            yield return fromTo(p, p += bbox.Size.Y * Vector3.UnitY);
            yield return fromTo(p, p += bbox.Size.Z * Vector3.UnitZ);
        }
    }

}