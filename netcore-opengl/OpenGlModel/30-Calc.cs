using Avalonia;
using System;
using System.Numerics;
using SearchAThing;

namespace SearchAThing
{

    public abstract partial class OpenGlModelBase : AvaloniaObject, IDisposable
    {

        /// <summary>
        /// convert given x,y coord ( origin left-bottom ) to normalized device coord
        /// </summary>
        /// <param name="ctl">control to retrieve info about screen size</param>
        /// <param name="pt">2d point in pixel ( origin left-bottom )</param>
        /// <returns>ndc point</returns>
        public Vector2 ToNDC(OpenGlControl ctl, Vector2 pt)
        {
            var w = ctl.Bounds.Width;
            var h = ctl.Bounds.Height;

            var ndcx = (float)(2f * pt.X / w - 1f);
            var ndcy = (float)(2f * pt.Y / h - 1f);

            return new Vector2(ndcx, ndcy);
        }

        /// <summary>
        /// retrieve 3d world coord by given mouse xy and ray to make hover test
        /// </summary>
        public (Vector3D pt, Line3D? ray) MousePosToWorldPos(float mouse_x, float mouse_y)
        {
            if (BBox.IsEmpty) return (Vector3D.Zero, ray: null);

            var ctl = this.FocusedControl;

            var pm = ctl.ProjectionMatrix;
            var vm = ctl.ViewMatrix;
            var mm = ctl.ModelMatrix;

            var res = Vector3D.Zero;

            var mouse_ndc = ToNDC(ctl, new Vector2(mouse_x, mouse_y));

            Vector3 CalcRay(Vector2 ray_nds)
            {
                // https://antongerdelan.net/opengl/raycasting.html
                var ray_clip = new Vector4(ray_nds, -1, 1);
                var ray_eye = Vector4.Transform(ray_clip, pm.Inverse());
                ray_eye = new Vector4(ray_eye.X, ray_eye.Y, -1, 0);
                var ray_wor = Vector4.Transform(ray_eye, vm.Inverse());
                ray_wor = Vector4.Transform(ray_wor, mm.Inverse());

                return new Vector3(ray_wor.X, ray_wor.Y, ray_wor.Z);
            }

            var v = CalcRay(mouse_ndc);

            Vector3D from = Vector3D.Zero;

            var cam = ctl.CameraPos;
            var cami = Vector3.Transform(ctl.CameraPos, mm.Inverse());
            // distance between camera and bbox middle
            var camMiddleDst = (BBox.Middle - (Vector3D)cam).Length;
            // current ray                                                
            var ray = v;
            var rayi = Vector3.Transform(ray, mm.Inverse());

            var L = new Line3D(cami, ray.Normalized(), Line3DConstructMode.PointAndVector);
            var ccs = ctl.CameraCS;
            if (((Vector3D)ctl.CameraTarget).EqualsTol(GeomTol, ctl.CameraPos))
                return (Vector3D.Zero, ray: null);

            var csCameraAtTarget = ccs.Move((Vector3D)ctl.CameraTarget - ccs.Origin);
            var cs1 = csCameraAtTarget.Transform(mm.Inverse());
            System.Console.WriteLine($"ics:{csCameraAtTarget}");
            System.Console.WriteLine($"cs1:{cs1}");

            var camPlane = new Plane3D(cs1);
            var ip = L.Intersect(1e-6, camPlane);

            res = ip;

            return (res, ray: L);
        }

    }

}