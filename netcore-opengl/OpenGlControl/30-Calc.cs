using static System.Math;
using System.Collections.Generic;
using System.Numerics;

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
        
    }

}