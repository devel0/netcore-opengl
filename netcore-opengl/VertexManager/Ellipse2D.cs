using System.Collections.Generic;
using System.Numerics;
using System;

namespace SearchAThing
{

    /// <summary>
    /// Helper to manage gl vertexes for figures such as triangles, lines
    /// </summary>
    public partial class VertexManager
    {

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

                var q = AddLine(figureName,
                    new Line3D(v.prev, v.item),
                    color, pixelWidth, 8, false, targetControl);
                idxs.AddRange(q);
            }

            return idxs;
        }

        /// <summary>
        /// add a circle
        /// </summary>        
        /// <param name="circle">circle2 info</param>        
        /// <param name="color">color function</param>
        /// <param name="pixelWidth">width of line in pixels</param>
        /// <param name="segmentCount">segment count of inscribed polygon (must >= 3)</param>
        /// <param name="screenControl">if specified drawing will be done in screen coordinates</param>
        /// <returns>figurename and geometry indexes</returns>
        public (string figureName, IReadOnlyList<uint> idxs) AddCircle(
            Circle3D circle,
            Func<Vector4> color = null,
            double pixelWidth = 1.0,
            int segmentCount = 360,
            OpenGlControl screenControl = null)
        {
            var figureName = Guid.NewGuid().ToString();
            var idxs = AddCircle(figureName, circle, color, pixelWidth, segmentCount, screenControl);
            return (figureName, idxs);
        }

        /// <summary>
        /// add a circle
        /// </summary>        
        /// <param name="figureName">name of figure where to store points</param>        
        /// <param name="circle">circle2 info</param>        
        /// <param name="color">color function</param>
        /// <param name="pixelWidth">width of line in pixels</param>
        /// <param name="segmentCount">segment count of inscribed polygon (must >= 3)</param>
        /// <param name="screenControl">if specified drawing will be done in screen coordinates</param>
        /// <returns>geometry indexes</returns>
        public IReadOnlyList<uint> AddCircle(string figureName,
            Circle3D circle,
            Func<Vector4> color = null,
            double pixelWidth = 1.0,
            int segmentCount = 360,
            OpenGlControl screenControl = null)
        {
            var c = circle.Center;

            var evtxs = circle.InscribedPolygon(Tol, segmentCount);

            var idxs = new List<uint>();

            foreach (var v in evtxs.WithNext())
            {
                if (v.next == null) break;

                var q = AddLine(figureName,
                    new Line3D(v.item, v.next),
                    color, pixelWidth, 8, false, screenControl);
                idxs.AddRange(q);
            }

            return idxs;
        }

    }

}