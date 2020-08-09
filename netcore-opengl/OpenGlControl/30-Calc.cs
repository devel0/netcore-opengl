using static System.Math;
using System.Collections.Generic;

namespace SearchAThing
{

    public partial class OpenGlControl
    {

        /// <summary>
        /// camera coordinate systems such as:
        /// - origin in CameraPosition
        /// - baseX : Normalized(cameraUp x BaseZ)
        /// - baseY : CameraUp
        /// - baseZ : Normalized(cameraPos - cameraTarget) 
        /// </summary>        
        public CoordinateSystem3D CameraCS =>
            new CoordinateSystem3D(CameraTarget, ((Vector3D)CameraUp).CrossProduct(CameraPos - CameraTarget), CameraUp);

        /// <summary>
        /// bbox of orthographic view
        /// </summary>        
        public BBox3D OrthoBBox
        {
            get
            {
                var ccs = CameraCS;
                var csPts = new List<Vector3D>();

                foreach (var p in Model.BBox.Points)
                {
                    csPts.Add(p.ToUCS(ccs));
                }

                return new BBox3D(csPts);
            }
        }
        
    }

}