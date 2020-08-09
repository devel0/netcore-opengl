using System.Collections.Generic;
using System.Numerics;
using QuantumConcepts.Formats.StereoLithography;
using System.Linq;
using System;
using Silk.NET.OpenGL;
using static System.Math;

namespace SearchAThing
{

    /// <summary>
    /// Helper to manage gl vertexes for figures such as triangles, lines
    /// </summary>
    public class VertexManager
    {

        /// <summary>
        /// tolerance used for dictionarized vertex storage
        /// </summary>        
        public double Tol { get; set; }

        // ------------------- vtx

        /// <summary>
        /// list of points
        /// entry i-th is coordinate of indexes i-th
        /// </summary>
        List<GLVertex> vtxList = new List<GLVertex>();

        /// <summary>
        /// list of inserted points
        /// </summary>
        public IReadOnlyList<GLVertex> VtxList => vtxList;

        GLVertex[] vtxs = null;

        /// <summary>
        /// points that figure indexes relates;
        /// updated when access to this property if vertex manager is invalidated due to some
        /// figure operation
        /// </summary>
        public GLVertex[] Vtxs
        {
            get
            {
                if (vtxs == null) vtxs = vtxList.Select(w => w).ToArray();
                return vtxs;
            }
        }

        // --------------------- vtxWithNormal

        GLVertexWithNormal[] vtxWithNormals = null;

        /// <summary>
        /// list of all inserted vertexes with normals;
        /// normals updated when access to this property if vertex manager is invalidated due to some
        /// figure operation
        /// </summary>
        public GLVertexWithNormal[] VtxWithNormals
        {
            get
            {
                if (vtxWithNormals == null) BuildGLVertexWithNormal();
                return vtxWithNormals;
            }
        }

        // -------------

        /// <summary>
        /// dictionary key:vertex.ToString() value:vertex index
        /// </summary>
        Dictionary<string, uint> idxs = new Dictionary<string, uint>();

        /// <summary>
        /// readonly list of all indexes subdivided for figureNames
        /// </summary>
        public IReadOnlyDictionary<string, uint> Idxs => idxs;

        /// <summary>
        /// list of point indexes foreach named figure
        /// </summary>
        Dictionary<string, List<uint>> figureIdxs = new Dictionary<string, List<uint>>();

        /// <summary>
        /// list of all figure names
        /// </summary>
        public IEnumerable<string> FigureNames => figureIdxs.Keys;

        /// <summary>
        /// list of figure names belonging to AddTriangles methods
        /// </summary>
        HashSet<string> triangleFigures = new HashSet<string>();

        /// <summary>
        /// bbox of all inserted points, updated after each Add methods
        /// </summary>
        public BBox3D BBox { get; private set; } = new BBox3D();

        /// <summary>
        /// construct a vertex manager that stores given points with given tolerance for indexing
        /// </summary>
        public VertexManager(double tol)
        {
            Tol = tol;
        }

        /// <summary>
        /// points added ( duplicate included )
        /// </summary>
        public int Points { get; private set; }

        /// <summary>
        /// retrieve idx of given point and update bbox.
        /// </summary>
        /// <param name="v">point coordinate</param>
        /// <returns>index of given point</returns>        
        uint AddPoint(Vector3D v)
        {
            uint res = 0;
            var str = v.ToString(Tol);
            if (!idxs.TryGetValue(str, out res))
            {
                res = (uint)vtxList.Count;
                vtxList.Add(new GLVertex { Position = v });
                idxs.Add(str, res);

                BBox.ApplyUnion(v);
            }

            ++Points;

            vtxs = null;
            vtxWithNormals = null;

            return res;
        }

        /// <summary>
        /// add given figures points array to the given figureName set
        /// </summary>
        /// <param name="figureName">set name for the figure points</param>
        /// <param name="figuresPoints">array of points array that represents a set of figures ( 3 pts for triangles, 2 pts for lines )</param>
        /// <param name="color">color ( or null ) to associate this figure name</param>
        /// <returns>readonly list of figure indexes</returns>
        public IReadOnlyList<uint> AddFigures(string figureName, IEnumerable<Vector3D[]> figuresPoints, Func<Vector4> color = null)
        {
            List<uint> idxs = null;
            if (!figureIdxs.TryGetValue(figureName, out idxs))
            {
                idxs = new List<uint>();
                figureIdxs.Add(figureName, idxs);
                if (color != null) SetColor(figureName, color);
            }

            foreach (var figurePoints in figuresPoints)
            {
                foreach (var pt in figurePoints) idxs.Add(AddPoint(pt));
            }

            return idxs;
        }

        /// <summary>
        /// add given triangles array to the given figureName set
        /// </summary>
        /// <param name="figureName">set name for the triangles</param>
        /// <param name="triangles">array of triangles ( 3 pts each )</param>        
        /// <param name="color">color ( or null ) to associate this figure name</param>
        /// <returns>readonly list of triangles indexes</returns>
        public IReadOnlyList<uint> AddTriangles(string figureName, IEnumerable<Vector3D[]> triangles, Func<Vector4> color = null)
        {
            triangleFigures.Add(figureName);
            if (color != null) SetColor(figureName, color);

            List<uint> idxs = null;
            if (!figureIdxs.TryGetValue(figureName, out idxs))
            {
                idxs = new List<uint>();
                figureIdxs.Add(figureName, idxs);
            }

            foreach (var tri in triangles)
            {
                var aIdx = AddPoint(tri[0]);
                var bIdx = AddPoint(tri[1]);
                var cIdx = AddPoint(tri[2]);

                idxs.AddRange(new[] { aIdx, bIdx, cIdx });
            }

            return idxs;
        }

        /// <summary>
        /// creates triangles for a fat line (smoothed using circle polygonalized).
        /// autoallocate random figurename.
        /// </summary>        
        /// <param name="line">line from,to</param>
        /// <param name="color">color ( or null ) to associate this figure name</param>
        /// <param name="w">thickness of line</param>
        /// <param name="segmentCount">nr of segment inscribed to circle</param>
        /// <param name="closeCaps">if true generates triangles for the front and rear</param>
        /// <returns>figure name and indexes of triangles that represent a flat line described as a parallelepiped</returns>
        public (string figureName, IReadOnlyList<uint> idxs) AddLine(Line3D line,
            Func<Vector4> color = null,
            double w = 1.0, int segmentCount = 8, bool closeCaps = true)
        {
            var figureName = Guid.NewGuid().ToString();
            var idxs = AddLine(figureName, line, color, w, segmentCount, closeCaps);
            return (figureName, idxs);
        }

        /// <summary>
        /// creates triangles for a fat line (smoothed using circle polygonalized)
        /// </summary>
        /// <param name="figureName">name of vtx mgr figure for this fat line</param>
        /// <param name="line">line from,to</param>
        /// <param name="color">color ( or null ) to associate this figure name</param>
        /// <param name="w">thickness of line</param>
        /// <param name="segmentCount">nr of segment inscribed to circle</param>
        /// <param name="closeCaps">if true generates triangles for the front and rear</param>        
        /// <returns>indexes of triangles that represent a flat line described as a parallelepiped</returns>
        public IReadOnlyList<uint> AddLine(string figureName, Line3D line,
            Func<Vector4> color = null,
            double w = 1.0, int segmentCount = 8, bool closeCaps = true)
        {
            var triangles = new List<Vector3D[]>();

            var ll = line.Length;
            if (ll == 0) throw new System.Exception($"invalid line len 0");

            var cs = new CoordinateSystem3D(line.From, line.To - line.From);

            var c = new Circle3D(Tol, cs, w / 2);
            var sply = c.InscribedPolygon(Tol, segmentCount).ToList();
            var eply = sply.Select(w => w + line.To - line.From).ToList();

            for (int i = 0; i < sply.Count - 1; ++i)
            {
                var splyNext = sply[i + 1];
                var eplyNext = eply[i + 1];

                triangles.Add(new[] { sply[i], splyNext, eply[i] });
                triangles.Add(new[] { splyNext, eplyNext, eply[i] });
            }

            if (closeCaps)
            {
                // front
                for (int i = 0; i < sply.Count - 1; ++i)
                {
                    var splyNext = sply[i + 1];

                    triangles.Add(new[] { line.From, splyNext, sply[i] });
                }

                // rear
                for (int i = 0; i < eply.Count - 1; ++i)
                {
                    var eplyNext = eply[i + 1];

                    triangles.Add(new[] { line.To, eplyNext, eply[i] });
                }
            }

            return AddTriangles(figureName, triangles, color);
        }

        /// <summary>
        /// creates triangles for a fat line
        /// </summary>
        /// <param name="figureName">name of vtx mgr figure for this fat line</param>
        /// <param name="line">line from,to</param>
        /// <param name="color">color ( or null ) to associate this figure name</param>
        /// <param name="w">thickness of line</param>
        /// <returns>indexes of triangles that represent a flat line described as a parallelepiped</returns>
        public IReadOnlyList<uint> AddSquaredLine(string figureName, Line3D line, Func<Vector4> color, double w = 1.0)
        {
            var triangles = new List<Vector3D[]>();

            var ll = line.Length;

            var cs = new CoordinateSystem3D(line.From, line.To - line.From);

            var ps0 = new Vector3D(w / 2, w / 2, 0).ToWCS(cs);
            var ps1 = new Vector3D(-w / 2, w / 2, 0).ToWCS(cs);
            var ps2 = new Vector3D(-w / 2, -w / 2, 0).ToWCS(cs);
            var ps3 = new Vector3D(w / 2, -w / 2, 0).ToWCS(cs);

            var pe0 = new Vector3D(w / 2, w / 2, ll).ToWCS(cs);
            var pe1 = new Vector3D(-w / 2, w / 2, ll).ToWCS(cs);
            var pe2 = new Vector3D(-w / 2, -w / 2, ll).ToWCS(cs);
            var pe3 = new Vector3D(w / 2, -w / 2, ll).ToWCS(cs);

            /*
                                             .+z
                                            .
                                     0----1
                                     | To |   (pe0-3)
                                     3----2                                      
                      +y
                       .
                       .
                    0----1   (ps0-3)
            +x....  |From|
                    3----2

            */

            // top
            triangles.Add(new[] { ps0, ps1, pe0 });
            triangles.Add(new[] { ps1, pe1, pe0 });

            // right
            triangles.Add(new[] { ps1, ps2, pe1 });
            triangles.Add(new[] { ps2, pe2, pe1 });

            // bottom
            triangles.Add(new[] { ps3, ps2, pe3 });
            triangles.Add(new[] { ps2, pe2, pe3 });

            // left
            triangles.Add(new[] { ps0, ps3, pe0 });
            triangles.Add(new[] { ps3, pe3, pe0 });

            // front
            triangles.Add(new[] { ps0, ps3, ps1 });
            triangles.Add(new[] { ps3, ps2, ps1 });

            // rear
            triangles.Add(new[] { pe0, pe3, pe1 });
            triangles.Add(new[] { pe3, pe2, pe1 });

            return AddTriangles(figureName, triangles, color);
        }

        /// <summary>
        /// add a line using control screen pixel coordinates xy ( origin at left-bottom )
        /// </summary>
        /// <param name="targetControl">ctl to retrieve screen size</param>
        /// <param name="from">point from</param>
        /// <param name="to">point to</param>
        /// <param name="color">color fn</param>
        /// <param name="pixelWidth">width of line in pixels</param>
        /// <returns></returns>
        public (string figureName, IReadOnlyList<uint> idxs) AddLine(
            OpenGlControl targetControl,
            Vector2 from, Vector2 to,
            Func<Vector4> color = null,
            double pixelWidth = 1.0)
        {
            var figureName = Guid.NewGuid().ToString();
            var idxs = AddLine(targetControl, figureName, from, to, color, pixelWidth);
            return (figureName, idxs);
        }

        public IReadOnlyList<uint> AddLine(OpenGlControl targetControl, string figureName,
            Vector2 from, Vector2 to,
            Func<Vector4> color = null,
            double pixelWidth = 1.0)
        {
            Func<Vector2, Vector2> toNdc = (v) => targetControl.Model.ToNDC(targetControl, v);

            var pw = pixelWidth;

            var triangles = new List<Vector3D[]>();

            var ll = (from - to).Length();
            if (ll == 0) throw new System.Exception($"invalid line len 0");

            var f = new Vector3D(from.X, from.Y);
            var t = new Vector3D(to.X, to.Y);
            var l = f.LineTo(t);
            var cs = new CoordinateSystem3D(f, (t - f).RotateAboutZAxis(PI / 2), t - f);

            Vector3D faNdc = toNdc(new Vector3D(-pw / 2, 0).ToWCS(cs).ToVector2());
            Vector3D fbNdc = toNdc(new Vector3D(pw / 2, 0).ToWCS(cs).ToVector2());
            Vector3D taNdc = toNdc(new Vector3D(-pw / 2, ll).ToWCS(cs).ToVector2());
            Vector3D tbNdc = toNdc(new Vector3D(pw / 2, ll).ToWCS(cs).ToVector2());

            triangles.Add(new[] { taNdc, faNdc, fbNdc });
            triangles.Add(new[] { fbNdc, tbNdc, taNdc });

            return AddTriangles(figureName, triangles, color);
        }

        /// <summary>
        /// add an ellipse using control screen pixel coordinates xy ( origin at left-bottom )
        /// </summary>
        /// <param name="targetControl">ctl to retrieve screen size</param>
        /// <param name="center">ellipse center</param>
        /// <param name="rX">ellipse x radius</param>
        /// <param name="rY">ellipse y radius</param>
        /// <param name="color">color function</param>
        /// <param name="pixelWidth">width of line in pixels</param>
        /// <param name="flatness">maximum error of ellipse approximation</param>
        /// <returns></returns>
        public (string figureName, IReadOnlyList<uint> idxs) AddEllipse(
            OpenGlControl targetControl,
            Vector2 center, float rX, float rY,
            Func<Vector4> color = null,
            double pixelWidth = 1.0,
            double flatness = 0.1)
        {
            var figureName = Guid.NewGuid().ToString();
            var idxs = AddEllipse(targetControl, figureName, center, rX, rY, color, pixelWidth, flatness);
            return (figureName, idxs);
        }

        public IReadOnlyList<uint> AddEllipse(OpenGlControl targetControl, string figureName,
            Vector2 center, float rX, float rY,
            Func<Vector4> color = null,
            double pixelWidth = 1.0,
            double flatness = 0.1)
        {
            Vector3D c = center;
            // pts already include last=first
            var evtxs = Polygon.EllipseToPolygon2D(c, 2 * rX, 2 * rY, 0.5);

            var idxs = new List<uint>();
            foreach (var v in evtxs.WithPrev())
            {
                if (v.prev == null) continue;

                var q = AddLine(targetControl, figureName,
                    v.prev.ToVector2(), v.item.ToVector2(),
                    color, pixelWidth);
                idxs.AddRange(q);
            }

            return idxs;
        }

        /// <summary>
        /// add an circle using control screen pixel coordinates xy ( origin at left-bottom )
        /// </summary>
        /// <param name="targetControl">ctl to retrieve screen size</param>
        /// <param name="center">ellipse center</param>
        /// <param name="r">circle radius</param>        
        /// <param name="color">color function</param>
        /// <param name="pixelWidth">width of line in pixels</param>
        /// <param name="segmentCount">segment count of inscribed polygon (must >= 3)</param>
        /// <returns></returns>
        public (string figureName, IReadOnlyList<uint> idxs) AddCircle(
            OpenGlControl targetControl,
            Vector2 center, float r,
            Func<Vector4> color = null,
            double pixelWidth = 1.0,
            int segmentCount = 360)
        {
            var figureName = Guid.NewGuid().ToString();
            var idxs = AddCircle(targetControl, figureName, center, r, color, pixelWidth, segmentCount);
            return (figureName, idxs);
        }

        /// <summary>
        /// add an circle using control screen pixel coordinates xy ( origin at left-bottom )
        /// </summary>
        /// <param name="targetControl">ctl to retrieve screen size</param>
        /// <param name="circle">circle2 info</param>        
        /// <param name="color">color function</param>
        /// <param name="pixelWidth">width of line in pixels</param>
        /// <param name="segmentCount">segment count of inscribed polygon (must >= 3)</param>
        /// <returns></returns>
        public (string figureName, IReadOnlyList<uint> idxs) AddCircle(
            OpenGlControl targetControl,
            Circle2 circle,
            Func<Vector4> color = null,
            double pixelWidth = 1.0,
            int segmentCount = 360)
        {
            var figureName = Guid.NewGuid().ToString();
            var idxs = AddCircle(targetControl, figureName, circle.Center, circle.Radius, color, pixelWidth, segmentCount);
            return (figureName, idxs);
        }

        public IReadOnlyList<uint> AddCircle(OpenGlControl targetControl, string figureName,
            Vector2 center, float r,
            Func<Vector4> color = null,
            double pixelWidth = 1.0,
            int segmentCount = 360)
        {
            Vector3D c = center;

            var evtxs = new Circle3D(Tol, CoordinateSystem3D.WCS.Move(c), r).InscribedPolygon(Tol, segmentCount);

            var idxs = new List<uint>();

            foreach (var v in evtxs.WithNext())
            {
                if (v.next == null) break;

                var q = AddLine(targetControl, figureName,
                    v.item.ToVector2(), v.next.ToVector2(),
                    color, pixelWidth);
                idxs.AddRange(q);
            }

            return idxs;
        }

        /// <summary>
        /// add given STL facets to the given figureName set
        /// </summary>
        /// <param name="figureName">set name for the triangles</param>        
        /// <param name="facets">STL facets (see STLDocument.Read(stream).Facets)</param>
        /// <param name="color">color ( or null ) to associate this figure name</param>
        /// <returns>readonly list of triangles indexes</returns>
        public void AddFaces(string figureName, IEnumerable<Facet> facets, Func<Vector4> color = null) =>
            AddTriangles(figureName, facets.Select(w => w.Vertices.Select(w => (Vector3D)w).ToArray()), color);

        /// <summary>
        /// to be called after all figures inserted; this will rebuild vertex normals
        /// </summary>
        /// <returns>array of vertex with normal suitable to use with GL array</returns>
        void BuildGLVertexWithNormal()
        {
            //if (vtxWithNormals != null) throw new Exception($"internal error: vtxWithNormal must null before rebuild");

            var vtxWithNormalNfos = new GLVertexWithNormalNfo[vtxList.Count];
            for (int i = 0; i < vtxList.Count; ++i)
            {
                var v = vtxList[i];
                vtxWithNormalNfos[i] = new GLVertexWithNormalNfo
                {
                    Position = v.Position,
                    Normal = new Vector3()
                };
            }

            foreach (var triFigure in triangleFigures)
            {
                var idxs = figureIdxs[triFigure];

                for (int i = 0; i < idxs.Count; i += 3)
                {
                    var a = vtxWithNormalNfos[(int)idxs[i]];
                    var b = vtxWithNormalNfos[(int)idxs[i + 1]];
                    var c = vtxWithNormalNfos[(int)idxs[i + 2]];

                    //var norm = ((c.Position - b.Position).CrossProduct(a.Position - b.Position)).Normalized();
                    var norm = Vector3.Normalize(Vector3.Cross(c.Position - b.Position, a.Position - b.Position));

                    a.Normal += norm;
                    b.Normal += norm;
                    c.Normal += norm;
                }
            }

            for (int i = 0; i < vtxWithNormalNfos.Length; i++)
            {
                var v = vtxWithNormalNfos[i];
                v.Normal = Vector3.Normalize(vtxWithNormalNfos[i].Normal);
            }

            vtxWithNormals = vtxWithNormalNfos.Select(w => w.ToGLVertexWithNormal()).ToArray();
        }

        /// <summary>
        /// retrieve the set of indexes belonging to given figure name
        /// </summary>
        /// <param name="figureName">figure name for which to retrieve indexes</param>
        /// <returns>indexes belonging to given figure name</returns>
        public uint[] GetIdxs(string figureName) => figureIdxs[figureName].ToArray();

        Dictionary<string, Func<Vector4>> figureColorDict = new Dictionary<string, Func<Vector4>>();

        /// <summary>
        /// set figure associated color
        /// </summary>
        /// <param name="figureName">figure name which associate a color</param>
        /// <param name="color">color to associate given figure name</param>
        public void SetColor(string figureName, Func<Vector4> color)
        {
            if (figureColorDict.ContainsKey(figureName))
                figureColorDict[figureName] = color;
            else
                figureColorDict.Add(figureName, color);
        }

        /// <summary>
        /// retrieve figure associated color
        /// </summary>
        /// <param name="figureName">figure name to retrieve associated color</param>
        /// <returns>color associated to given figure name or null if not specified</returns>
        public Vector4? GetColor(string figureName)
        {
            Func<Vector4> fn;

            if (!figureColorDict.TryGetValue(figureName, out fn)) return null;

            return fn();
        }

    }

    public static partial class OpenGlExt
    {

        /// <summary>
        /// create GLVertexWithNormal from given vector ( normal = Vector.Zero )
        /// </summary>
        /// <param name="v">vector from which build GLVertexWithNormal</param>
        /// <returns>GLVertexWithNormal constructed from given vector ( normal = Vector.Zero )</returns>
        public static GLVertexWithNormal ToGLTriangleVertex(this Vector3D v)
        {
            return new GLVertexWithNormal
            {
                Position = new Vector3((float)v.X, (float)v.Y, (float)v.Z),
                Normal = Vector3D.Zero
            };
        }

        /// <summary>
        /// Add this BBox to GL vertex manager as wires of faces
        /// </summary>
        /// <param name="vtxMgr">vertex manager</param>
        /// <param name="color">color to associate to figure</param>
        /// <param name="wireMode">if true draw bbox as lines, faces elsewhere</param>
        /// <param name="_figureName">name of figure</param>
        /// <param name="w">thickness of lines</param>
        public static void AddToVertexManager(this BBox3D bbox,
            VertexManager vtxMgr,
            Func<System.Numerics.Vector4> color = null, bool wireMode = true, string _figureName = null, double w = 1)
        {
            if (!wireMode) throw new NotImplementedException("only wireMode actually implemented");

            var figureName = _figureName == null ? Guid.NewGuid().ToString() : _figureName;

            if (wireMode)
            {
                var pts = bbox.Points;

                for (int i = 0; i < 4; ++i)
                {
                    var f = pts[i];
                    var t = i == 3 ? pts[0] : pts[i + 1];

                    vtxMgr.AddLine(f.LineTo(t), color, w);

                    vtxMgr.AddLine(pts[i].LineTo(pts[i + 4]), color, w);
                }

                for (int i = 4; i < 8; ++i)
                {
                    var f = pts[i];
                    var t = i == 7 ? pts[4] : pts[i + 1];

                    vtxMgr.AddLine(f.LineTo(t), color, w);
                }
            }
        }

    }

}