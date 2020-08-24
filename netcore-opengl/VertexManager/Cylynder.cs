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
        /// add an cylynder
        /// </summary>        
        /// <param name="line">line describing cylinder from, to</param>
        /// <param name="radius">radius of cylinder</param>        
        /// <param name="color">color</param>
        /// <param name="segmentCount">nr. of segment discretization of cylinder circle shape</param>
        /// <param name="closeCaps">close ending caps</param>
        /// <returns>figure name and geometry indexes</returns>
        public (string figureName, IReadOnlyList<uint> idxs) AddCylinder(Line3D line,
            double radius, Func<Vector4> color = null, int segmentCount = 8, bool closeCaps = true) =>
                AddLine(line, color, 2 * radius, segmentCount, closeCaps);

        /// <summary>
        /// add an cylinder to the given figurename
        /// </summary>        
        /// <param name="figureName">figure name which adds cylinder</param>
        /// <param name="line">line describing cylinder from, to</param>
        /// <param name="radius">radius of cylinder</param>        
        /// <param name="color">color</param>
        /// <param name="segmentCount">nr. of segment discretization of cylinder circle shape</param>
        /// <param name="closeCaps">close ending caps</param>
        /// <returns>figure name and geometry indexes</returns>
        public IReadOnlyList<uint> AddCylinder(string figureName, Line3D line,
            double radius, Func<Vector4> color = null, int segmentCount = 8, bool closeCaps = true)
                => AddLine(figureName, line, color, 2 * radius, segmentCount, closeCaps);

    }

}