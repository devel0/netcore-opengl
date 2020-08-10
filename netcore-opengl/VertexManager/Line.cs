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

        public (string figureName, IReadOnlyList<uint> idxs) AddPolyLine(IEnumerable<Vector3D> pts,
            Func<Vector4> color = null,
            double w = 1.0, int segmentCount = 8, bool closeCaps = true)
        {
            var figureName = Guid.NewGuid().ToString();
            var idxs = AddPolyLine(figureName, pts, color, w, segmentCount, closeCaps);
            return (figureName, idxs);
        }

        public IReadOnlyList<uint> AddPolyLine(string figureName, IEnumerable<Vector3D> pts,
            Func<Vector4> color = null,
            double w = 1.0, int segmentCount = 8, bool closeCaps = true)
        {
            var triangles = new List<Vector3D[]>();

            foreach (var p in pts.WithNext())
            {
                if (p.next == null) continue;

                var cs = new CoordinateSystem3D(p.item, p.next - p.item);

                var c = new Circle3D(Tol, cs, w / 2);
                var sply = c.InscribedPolygon(Tol, segmentCount).ToList();
                var eply = sply.Select(w => w + p.next - p.item).ToList();

                for (int i = 0; i < sply.Count - 1; ++i)
                {
                    var splyNext = sply[i + 1];
                    var eplyNext = eply[i + 1];

                    triangles.Add(new[] { sply[i], splyNext, eply[i] });
                    triangles.Add(new[] { splyNext, eplyNext, eply[i] });
                }
            }

            // if (closeCaps)
            // {
            //     // front
            //     for (int i = 0; i < sply.Count - 1; ++i)
            //     {
            //         var splyNext = sply[i + 1];

            //         triangles.Add(new[] { line.From, splyNext, sply[i] });
            //     }

            //     // rear
            //     for (int i = 0; i < eply.Count - 1; ++i)
            //     {
            //         var eplyNext = eply[i + 1];

            //         triangles.Add(new[] { line.To, eplyNext, eply[i] });
            //     }
            // }

            return AddTriangles(figureName, triangles, color);
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

    }

}