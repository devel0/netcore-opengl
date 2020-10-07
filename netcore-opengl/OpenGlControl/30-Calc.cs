namespace SearchAThing
{

    public partial class OpenGlControl
    {

        /// <summary>
        /// camera coordinate systems ( Z+ point to observer ):
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