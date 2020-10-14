using System.Collections.Generic;
using System.Numerics;
using QuantumConcepts.Formats.StereoLithography;
using System.Linq;
using System;
using Silk.NET.OpenGL;
using static System.Math;
using System.Runtime.CompilerServices;
using SearchAThing;

namespace SearchAThing
{

    /// <summary>
    /// Helper to manage gl vertexes for figures such as triangles, lines
    /// </summary>
    public partial class VertexManager
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
        /// add given STL facets to the given figureName set
        /// </summary>
        /// <param name="figureName">set name for the triangles</param>        
        /// <param name="facets">STL facets (see STLDocument.Read(stream).Facets)</param>
        /// <param name="color">color ( or null ) to associate this figure name</param>
        /// <returns>readonly list of triangles indexes</returns>
        public void AddFaces(string figureName, IEnumerable<Facet> facets, Func<Vector4> color = null) =>
            AddTriangles(figureName, facets.Select(w => w.Vertices.Select(w => (Vector3D)w).ToArray()), color);

        /// <summary>
        /// add given dxf facet3d to the given figureName set
        /// </summary>
        /// <param name="figureName">set name for the triangles</param>        
        /// <param name="facets">dxf face3d</param>
        /// <param name="color">color ( or null ) to associate this figure name</param>
        /// <returns>readonly list of triangles indexes</returns>
        public void AddFaces(string figureName, IEnumerable<netDxf.Entities.Face3d> facets, Func<Vector4> color = null) =>
            AddTriangles(figureName, facets.Select(w => w.Points().ToArray()), color);

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
        /// retrieve figure associated color (r,g,b,a)
        /// </summary>
        /// <param name="figureName">figure name to retrieve associated color</param>
        /// <returns>color (r,g,b,a) associated to given figure name or null if not specified</returns>
        public Vector4? GetColor(string figureName)
        {
            Func<Vector4> fn;

            if (!figureColorDict.TryGetValue(figureName, out fn)) return null;

            return fn();
        }

        /// <summary>
        /// given a,b,c,d vertex of quad retrieve two triangles that fit into;
        /// a,b,c,d must consecutive on the quad path
        /// </summary>
        /// <param name="a">quad 1th vertex</param>
        /// <param name="b">quad 2th vertex</param>
        /// <param name="c">quad 3th vertex</param>
        /// <param name="d">quad 4th vertex</param>
        /// <returns>array of triangles vertexes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Vector3D[][] QuadGetTriangles(Vector3D a, Vector3D b, Vector3D c, Vector3D d) =>
            new[]
            {
                new[] { a, b, d },
                new[] { b, c, d }
            };

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

    }

}