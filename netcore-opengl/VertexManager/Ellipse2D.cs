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

    }

}