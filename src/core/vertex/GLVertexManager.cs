namespace SearchAThing.OpenGL.Core;

public class GLVertexManager : IGLVertexManager
{

    #region property changed

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// invoke this method to forward propertchanged event notification.
    /// note: not needed to specify propertyName set by compiler service to called property.
    /// </summary>        
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    static object lck = new Object();

    List<GLVertexStruct> Vertexes = new List<GLVertexStruct>();

    #region VertexCount

    private int _VertexCount = 0;

    public int VertexCount
    {
        get => _VertexCount;
        set
        {
            var changed = value != _VertexCount;
            if (changed)
            {
                _VertexCount = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region TriangleCount

    private int _TriangleCount = 0;

    public int TriangleCount
    {
        get => _TriangleCount;
        set
        {
            var changed = value != _TriangleCount;
            if (changed)
            {
                _TriangleCount = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    /// <summary>
    /// Vertexes[i-th] element associated primitives
    /// </summary>    
    List<int> VertexMemberCount = new List<int>();

    HashSet<uint> RemovedVertexes = new HashSet<uint>();

    /// <summary>
    /// map vertexSignature to vertex index
    /// </summary>
    Dictionary<string, uint> VertexMap = new Dictionary<string, uint>();

    public event VertexManagerInvalidatedDelegate? Invalidated;

    /// <summary>
    /// Create a new gl vertex manager that can be added to the gl model through <see cref="GLModel.AddCustomVertexManager"/>.
    /// </summary>
    /// <param name="expandModelBBox">If false adding vertexes to this vertex manager doesn't affects the gl model bbox. (Default: true).</param>
    public GLVertexManager(bool expandModelBBox = true)
    {
        ExpandModelBBox = expandModelBBox;
    }

    GLVertexStruct[]? VboArray = null;

    public GLVertexStruct[] GetVboArray()
    {
        if (VboArray is null) VboArray = Vertexes.ToArray();

        return VboArray;
    }

    #region BBox

    private BBox _LBBox = new BBox();

    public BBox LBBox
    {
        get => _LBBox;
        private set
        {
            var changed = value != _LBBox;
            if (changed)
            {
                _LBBox = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion   

    /// <summary>
    /// Recompute bbox [local] of all figure/primitive vertexes upading <see cref="LBBox"/>.<br/>
    /// Used internally after figure/primitive deletion.
    /// REVIEW: may improved
    /// </summary>
    public BBox RecomputeLBBox()
    {
        var bbox = new BBox();

        foreach (var figure in Figures)
        {
            if (figure.ScreenCoordMode) continue;

            if (figure.ExpandBBox)
            {
                var om = figure.ObjectMatrix;

                if (!om.IsIdentity)
                {
                    foreach (var vertex in figure.Vertexes())
                        bbox.ApplyUnion(Vector3.Transform(vertex.Position, om));
                }
                else
                {
                    foreach (var vertex in figure.Vertexes())
                        bbox.ApplyUnion(vertex.Position);
                }
            }
        }

        LBBox = bbox;

        return bbox;
    }

    public bool ExpandModelBBox { get; private set; }

    public void Clear()
    {
        var prevFigures = FiguresHS.ToArray();

        foreach (var figure in FiguresHS)
        {
            figure.ParentVertexManager = null;
        }

        Vertexes = new List<GLVertexStruct>();
        VertexMemberCount = new List<int>();
        RemovedVertexes = new HashSet<uint>();
        VertexMap = new Dictionary<string, uint>();
        VboArray = null;
        FiguresHS = new HashSet<GLFigureBase>();
        LBBox = new BBox();
        VertexCount = 0;
        TriangleCount = 0;
        FigureCount = 0;

        FiguresCleared?.Invoke(this, prevFigures);
        Invalidated?.Invoke(this);
    }

    #region vertex

    internal void AddVertex(GLVertex vertex, bool computeNormal = true)
    {
        if (vertex.Index is not null) throw new Exception($"vertex already added");

        if (computeNormal && vertex.ParentPrimitive is GLTriangle tri && vertex.ParentFigure is GLTriangleFigure triFig)
        {
            var normal = triFig.ComputeNormal(tri, vertex);
            vertex.SetNormal(normal);
        }

        var vertexSignature = vertex.Signature;

        lock (lck)
        {
            if (!VertexMap.TryGetValue(vertexSignature, out var vertexIndex))
            {
                if (RemovedVertexes.Count > 0)
                {
                    vertexIndex = RemovedVertexes.First();
                    RemovedVertexes.Remove(vertexIndex);
                    VertexMemberCount[(int)vertexIndex] = 1;
                    Vertexes[(int)vertexIndex] = vertex.ToStruct();
                }
                else
                {
                    vertexIndex = (uint)Vertexes.Count;
                    Vertexes.Add(vertex.ToStruct());
                    VertexMemberCount.Add(1);
                }
                VertexMap.Add(vertexSignature, vertexIndex);
                if (Vertexes.Count != VertexCount)
                {
                    VertexCount = Vertexes.Count;
                }
            }
            else
            {
                ++VertexMemberCount[(int)vertexIndex];
            }
            vertex.Index = vertexIndex;
        }

        if (!vertex.ScreenCoordMode)
        {
            var bbox_changed = false;

            var fig = vertex.ParentFigure;
            if (fig is not null)
            {
                if (fig.ExpandBBox)
                {
                    var om = vertex.ParentFigure?.ObjectMatrix;

                    if (om is not null)
                        bbox_changed = LBBox.ApplyUnion(Vector3.Transform(vertex.Position, om.Value));
                }
            }
            else
                bbox_changed = LBBox.ApplyUnion(vertex.Position);

            if (bbox_changed) OnPropertyChanged(nameof(LBBox));
        }

        VboArray = null;
        Invalidated?.Invoke(this);
    }

    internal void RemoveVertex(GLVertex vertex)
    {
        if (vertex.Index is null) throw new Exception($"vertex already removed");

        var vertexSignature = vertex.Signature;

        lock (lck)
        {
            if (--VertexMemberCount[(int)vertex.Index.Value] == 0)
            {
                RemovedVertexes.Add(vertex.Index.Value);
                VertexMap.Remove(vertexSignature);
                VertexCount = Vertexes.Count;
            }
            vertex.Index = null;
        }

        VboArray = null;
        Invalidated?.Invoke(this);
    }

    #endregion

    #region figure

    HashSet<GLFigureBase> FiguresHS = new HashSet<GLFigureBase>();

    public event FigureEvent? FigureAdded;

    public event FigureEvent? FigureRemoved;

    public event FiguresEvent? FiguresCleared;

    public IEnumerable<GLFigureBase> Figures => FiguresHS;

    #region FigureCount

    private int _FigureCount = 0;

    public int FigureCount
    {
        get => _FigureCount;
        set
        {
            var changed = value != _FigureCount;
            if (changed)
            {
                _FigureCount = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    /// <summary>
    /// Adds given figures to the vertex manager.<br/>
    /// All vertexes belonging to the figures primitives will be added.
    /// Emits <see cref="FigureAdded"/> event.
    /// </summary>
    /// <param name="figures">Figures to add.</param>
    public void AddFigure(params GLFigureBase[] figures) => AddFigure(figures.AsEnumerable());

    /// <summary>
    /// Adds given figures to the vertex manager.<br/>
    /// All vertexes belonging to the figures primitives will be added.
    /// Emits <see cref="FigureAdded"/> event.
    /// </summary>
    /// <param name="figures">Figures to add.</param>
    public void AddFigure(IEnumerable<GLFigureBase> figures)
    {
        foreach (var figure in figures) AddFigure(figure);
    }

    public void AddFigure(GLFigureBase figure)
    {
        figure.ParentVertexManager = this;

        foreach (var primitive in figure.Primitives)
        {
            AddPrimitive(primitive, figure);
        }

        if (figure is GLTriangleFigure triFig && triFig.ComputeNormalMean)
            triFig.RebuildNormal(onlyMean: true);

        FiguresHS.Add(figure);
        FigureAdded?.Invoke(this, figure);
        FigureCount = FiguresHS.Count;
    }

    /// <summary>
    /// Remove given figures from the vertex manager.<br/>
    /// All vertexes belonging to the figures primitives will be removed.
    /// Emits <see cref="FigureRemoved"/> event.
    /// </summary>
    /// <param name="figures">Figures to remove.</param>  
    /// <returns>Count of figures effectively removed.</returns>
    public int RemoveFigure(params GLFigureBase[] figures) => RemoveFigure(figures.AsEnumerable());

    /// <summary>
    /// Remove given figures from the vertex manager.<br/>
    /// All vertexes belonging to the figures primitives will be removed.
    /// Emits <see cref="FigureRemoved"/> event.
    /// </summary>
    /// <param name="figures">Figures to remove.</param>  
    /// <returns>Count of figures effectively removed.</returns>
    public int RemoveFigure(IEnumerable<GLFigureBase> figures)
    {
        var removedFigureCount = 0;

        foreach (var figure in figures)
        {
            if (RemoveFigure(figure))
                ++removedFigureCount;
        }

        return removedFigureCount;
    }

    /// <summary>
    /// Remove given figure from the vertex manager.<br/>
    /// All vertexes belonging to the figures primitives will be removed.
    /// Emits <see cref="FigureRemoved"/> event.
    /// </summary>
    /// <param name="figure">Figure to remove.</param>  
    /// <returns>True if figure effectively removed.</returns>
    public bool RemoveFigure(GLFigureBase figure)
    {
        if (figure.ParentVertexManager is null)
            return false;

        foreach (var primitive in figure.Primitives)
        {
            RemovePrimitive(primitive);
        }

        figure.ParentVertexManager = null;

        FiguresHS.Remove(figure);
        FigureCount = FiguresHS.Count;
        FigureRemoved?.Invoke(this, figure);

        return true;
    }

    #endregion

    #region primitive

    internal void AddPrimitive(GLPrimitiveBase primitive, IGLFigure parentFigure)
    {
        primitive.ParentFigure = parentFigure;

        foreach (var vertex in primitive.Vertexes)
        {
            vertex.ParentPrimitive = primitive;

            AddVertex(vertex);
        }

        if (primitive.PrimitiveType == GLPrimitiveType.Triangle) ++TriangleCount;
    }

    internal void RemovePrimitive(GLPrimitiveBase primitive)
    {
        foreach (var vertex in primitive.Vertexes)
        {
            RemoveVertex(vertex);

            vertex.ParentPrimitive = null;
        }        
        
        primitive.ParentFigure = null;
        if (primitive.PrimitiveType == GLPrimitiveType.Triangle) --TriangleCount;
    }

    #endregion

    /// <summary>
    /// for debug purpose
    /// </summary>
    internal string Dump()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Vertexes Count : {Vertexes.Count}");
        sb.AppendLine($"Vertexes Removed : {RemovedVertexes.Count}");

        var table = new List<List<string>>();

        for (int i = 0; i < Vertexes.Count; ++i)
        {
            var vertex = Vertexes[i];

            var removed = RemovedVertexes.Contains((uint)i);

            var vertexSignature = removed ? "(removed)" : this.VertexMap.First(w => w.Value == i).Key;

            table.Add(new List<string>
            {
                i.ToString(),
                VertexMemberCount[i].ToString(),
                Invariant($"{vertex.Position}"),
                Invariant($"{vertex.Normal}"),
                vertexSignature
            });
        }

        sb.Append(table.TableFormat(headers: new List<string>()
        {
            "Index",
            "Members",
            "Position",
            "Normal",
            "Signature"
        }));

        return sb.ToString();
    }

}