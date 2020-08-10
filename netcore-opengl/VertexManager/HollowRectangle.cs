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
    public partial class VertexManager
    {

        public (string figureName, IReadOnlyList<uint> idxs) AddHollowRectangle(CoordinateSystem3D cs, double length,
            double xLen, double xThickness,
            double yLen, double yThickness,
            Func<Vector4> color = null)
        {
            var figureName = Guid.NewGuid().ToString();
            var idxs = AddHollowRectangle(figureName, cs, length, xLen, xThickness, yLen, yThickness, color);
            return (figureName, idxs);
        }

        /// <summary>
        /// add an hollow rectangle to the given figure name
        /// </summary>
        /// <param name="figureName">figure name</param>
        /// <param name="cs">cs at base of hollow rectangle</param>
        /// <param name="length">length along cs z axis of the hollow rectangle</param>
        /// <param name="xLen">len of the rect along cs x axis</param>
        /// <param name="xThickness">thickness of rect (2 walls) along cs x axis</param>
        /// <param name="yLen">len of rect along cs y axis</param>
        /// <param name="yThickness">thickness of rect (2 walls) along cs y axis</param>
        /// <param name="color">color</param>
        /// <returns>array of indexes</returns>
        public IReadOnlyList<uint> AddHollowRectangle(string figureName, CoordinateSystem3D cs, double length,
            double xLen, double xThickness,
            double yLen, double yThickness,
            Func<Vector4> color = null)
        {
            var triangles = new List<Vector3D[]>();

            /*
                   .csy
            d      .      c
            +------.------+     ---------------------
            |  di  .  ci  |                       ^ 
            |  +---.---+  |                       |
            |  |   .   |  |                       |
            |  |   ....|..|....csx                | yLen
            |  |       |  |                       |
            |  +-------+  |   --                  |
            |  ai     bi  |   |   yThickness      v
            +-------------+   --   -----------------
            a             b

            |--| xThickness
            |<--- xLen -->|

            */

            var xaxis = Vector3D.XAxis;
            var yaxis = Vector3D.YAxis;

            var a = new Vector3D(-xLen / 2, -yLen / 2, 0);
            var b = a + xLen * xaxis;
            var c = b + yLen * yaxis;
            var d = a + yLen * yaxis;

            var xiLen = xLen - 2 * xThickness;
            var yiLen = yLen - 2 * yThickness;

            var ai = a + xThickness * xaxis + yThickness * yaxis;
            var bi = ai + xiLen * xaxis;
            var ci = bi + yiLen * yaxis;
            var di = ai + yiLen * yaxis;

            var zaxis = Vector3D.ZAxis;
            var zl = length * zaxis;             

            var v = new[] { a, b, c, d, a };
            var vi = new[] { ai, bi, ci, di, ai };
            var v2 = v.Select(w => w + zl).ToArray();
            var v2i = vi.Select(w => w + zl).ToArray();

            /*
                   .csy
            v[3]   .   v[2] 
            +------.------+     
            | vi[3]. vi[2]|       
            |  +---.---+  |       
            |  |   .   |  |       
            |  |   ....|..|....csx
            |  |       |  | 
            |  +-------+  | 
            | vi[0]  vi[1]| 
            +-------------+ 
            v[0]        v[1]

            */

            for (int i = 0; i < 4; ++i)
            {
                // external face
                triangles.AddRange(QuadGetTriangles(v[i], v[i + 1], v2[i + 1], v2[i]));

                // internal face
                triangles.AddRange(QuadGetTriangles(vi[i], vi[i + 1], v2i[i + 1], v2i[i]));

                // closing taps
                triangles.AddRange(QuadGetTriangles(v[i], v[i + 1], vi[i + 1], vi[i]));
                triangles.AddRange(QuadGetTriangles(v2[i], v2[i + 1], v2i[i + 1], v2i[i]));
            }

            // back to wcs
            triangles = triangles.Select(w => w.Select(r => r.ToWCS(cs)).ToArray()).ToList();

            return AddTriangles(figureName, triangles, color);
        }

    }

}