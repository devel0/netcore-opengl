using System.Collections.Generic;
using System.Numerics;
using QuantumConcepts.Formats.StereoLithography;
using System.Linq;
using System;

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

        /// <summary>
        /// list of points
        /// entry i-th is coordinate of indexes i-th
        /// </summary>
        List<GLVertexWithNormalNfo> vtxs = new List<GLVertexWithNormalNfo>();

        /// <summary>
        /// readonly list of all inserted vertexes with normals ( normals updated only after BuildPoints() call )
        /// </summary>
        public IReadOnlyList<GLVertexWithNormalNfo> Vtxs => vtxs;

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
                res = (uint)vtxs.Count;
                vtxs.Add(new GLVertexWithNormalNfo { Position = v });
                idxs.Add(str, res);

                BBox.ApplyUnion(v);
            }

            ++Points;

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
            var sply = c.InscribedPolygon(segmentCount).ToList();
            var eply = sply.Select(w => w + line.To - line.From).ToList();

            for (int i = 0; i < sply.Count; ++i)
            {
                var splyNext = i == sply.Count - 1 ? sply[0] : sply[i + 1];
                var eplyNext = i == eply.Count - 1 ? eply[0] : eply[i + 1];

                triangles.Add(new[] { sply[i], splyNext, eply[i] });
                triangles.Add(new[] { splyNext, eplyNext, eply[i] });
            }

            if (closeCaps)
            {
                // front
                for (int i = 0; i < sply.Count; ++i)
                {
                    var splyNext = i == sply.Count - 1 ? sply[0] : sply[i + 1];

                    triangles.Add(new[] { line.From, splyNext, sply[i] });
                }

                // rear
                for (int i = 0; i < eply.Count; ++i)
                {
                    var eplyNext = i == eply.Count - 1 ? eply[0] : eply[i + 1];

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
        public GLVertexWithNormal[] BuildPoints()
        {
            for (int i = 0; i < vtxs.Count; ++i)
            {
                var v = vtxs[i];
                v.Normal = new Vector3();
            }

            foreach (var triFigure in triangleFigures)
            {
                var idxs = figureIdxs[triFigure];

                for (int i = 0; i < idxs.Count; i += 3)
                {
                    var a = vtxs[(int)idxs[i]];
                    var b = vtxs[(int)idxs[i + 1]];
                    var c = vtxs[(int)idxs[i + 2]];

                    var norm = ((c.Position - b.Position).CrossProduct(a.Position - b.Position)).Normalized();

                    a.Normal += norm;
                    b.Normal += norm;
                    c.Normal += norm;
                }
            }

            for (int i = 0; i < vtxs.Count; i++)
            {
                var v = vtxs[i];
                v.Normal = Vector3.Normalize(vtxs[i].Normal);
            }

            return vtxs.Select(w => new GLVertexWithNormal { Position = w.Position, Normal = w.Normal }).ToArray();
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