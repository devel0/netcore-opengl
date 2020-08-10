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
 
        public (string figureName, IReadOnlyList<uint> idxs) AddCylinder(Line3D line,
            double radius, Func<Vector4> color = null, int segmentCount = 8, bool closeCaps = true) =>
                AddLine(line, color, radius, segmentCount, closeCaps);

        public IReadOnlyList<uint> AddCylinder(string figureName, Line3D line,
            double radius, Func<Vector4> color = null, int segmentCount = 8, bool closeCaps = true)
                => AddLine(figureName, line, color, radius, segmentCount, closeCaps);

    }

}