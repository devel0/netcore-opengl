using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System;

namespace SearchAThing
{

    /// <summary>
    /// Helper to manage gl vertexes for figures such as triangles, lines
    /// </summary>
    public partial class VertexManager
    {



        /// <summary>
        /// add an cuboid to the given figure name
        /// </summary>        
        /// <param name="cs">cs at base center of cuboid</param>
        /// <param name="length">length along cs z axis of the hollow rectangle</param>
        /// <param name="xLen">len of the rect along cs x axis</param>        
        /// <param name="yLen">len of rect along cs y axis</param>        
        /// <param name="color">color</param>
        /// <returns>array of indexes</returns>
        public (string figureName, IReadOnlyList<uint> idxs) AddCuboid(CoordinateSystem3D cs, double length,
            double xLen, double yLen,
            Func<Vector4> color = null)
        {
            var figureName = Guid.NewGuid().ToString();
            var idxs = AddCuboid(figureName, cs, length, xLen, yLen, color);
            return (figureName, idxs);
        }

        /// <summary>
        /// add an cuboid to the given figure name
        /// </summary>
        /// <param name="figureName">figure name</param>
        /// <param name="cs">cs at base center of cuboid</param>
        /// <param name="length">length along cs z axis of the hollow rectangle</param>
        /// <param name="xLen">len of the rect along cs x axis</param>        
        /// <param name="yLen">len of rect along cs y axis</param>        
        /// <param name="color">color</param>
        /// <returns>array of indexes</returns>
        public IReadOnlyList<uint> AddCuboid(string figureName, CoordinateSystem3D cs, double length,
            double xLen, double yLen,
            Func<Vector4> color = null)
        {
            var triangles = new List<Vector3D[]>();

            /*
                   .csy
            d      .      c
            +------.------+     --------
            |      .      |          ^ 
            |      .      |          |
            |      .      |          |
            |      .......|....csx   | yLen
            |             |          |        
            |             |          v
            +-------------+     --------
            a             b
            
            |<--- xLen -->|

            */

            var xaxis = Vector3D.XAxis;
            var yaxis = Vector3D.YAxis;

            var a = new Vector3D(-xLen / 2, -yLen / 2, 0);
            var b = a + xLen * xaxis;
            var c = b + yLen * yaxis;
            var d = a + yLen * yaxis;

            var zaxis = Vector3D.ZAxis;
            var zl = length * zaxis;

            var a2 = a + zl;
            var b2 = b + zl;
            var c2 = c + zl;
            var d2 = d + zl;

            triangles.Add(new[] { a, b, a2 }); triangles.Add(new[] { b, b2, a2 });
            triangles.Add(new[] { b, c, b2 }); triangles.Add(new[] { c, c2, b2 });
            triangles.Add(new[] { c, d, c2 }); triangles.Add(new[] { d, d2, c2 });
            triangles.Add(new[] { d, a, d2 }); triangles.Add(new[] { a, a2, d2 });

            // base,top
            triangles.Add(new[] { a, b, c }); triangles.Add(new[] { c, d, a });
            triangles.Add(new[] { a2, b2, c2 }); triangles.Add(new[] { c2, d2, a2 });

            // back to wcs
            triangles = triangles.Select(w => w.Select(r => r.ToWCS(cs)).ToArray()).ToList();

            return AddTriangles(figureName, triangles, color);
        }

        public (string figureName, IReadOnlyList<uint> idxs) AddCuboid(CoordinateSystem3D cs, double length,
            Vector3D p1, Vector3D p2,
            Func<Vector4> color = null)
        {
            var figureName = Guid.NewGuid().ToString();
            var idxs = AddCuboid(figureName, cs, length, p1, p2, color);
            return (figureName, idxs);
        }


        /// <summary>
        /// add a box
        /// </summary>
        /// <param name="figureName">figure name</param>
        /// <param name="cs">cs at base center of cuboid</param>
        /// <param name="length">length along cs z axis of the hollow rectangle</param>
        /// <param name="xLen">len of the rect along cs x axis</param>        
        /// <param name="yLen">len of rect along cs y axis</param>        
        /// <param name="color">color</param>
        /// <returns>array of indexes</returns>
        public IReadOnlyList<uint> AddCuboid(string figureName,
            CoordinateSystem3D cs, double length,
            Vector3D p1, Vector3D p2,
            Func<Vector4> color = null)
        {
            var _p1 = p1.ToUCS(cs);
            var _p2 = p2.ToUCS(cs);
            var _diff = _p2 - _p1;
            var xlen = _diff.X;
            var ylen = _diff.Y;

            return AddCuboid(figureName, cs.Move((p1 + p2) / 2), length, xlen, ylen, color);
        }

    }

}