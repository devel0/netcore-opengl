using System.Collections.Generic;
using System.Numerics;
using QuantumConcepts.Formats.StereoLithography;
using System.Linq;
using System;
using Silk.NET.OpenGL;
using static System.Math;

namespace SearchAThing
{

    public static partial class OpenGlExt
    {       

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