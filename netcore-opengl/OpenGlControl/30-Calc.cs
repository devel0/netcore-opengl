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
        /// - csZaxis : Normalized(cameraPos - cameraTarget) 
        /// - csXaxis : Normalized(cameraUp x (cameraPos - cameraTarget))        
        /// </summary>        
        public CoordinateSystem3D CameraCS =>
            new CoordinateSystem3D(CameraPos,
                CameraPos - CameraTarget,
                ((Vector3D)CameraUp).CrossProduct(CameraPos - CameraTarget),                
                SmartCsMode.Z_X);

    }

}