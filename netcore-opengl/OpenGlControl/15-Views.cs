using System.Numerics;
using Silk.NET.OpenGL;
using static System.Math;
using System.Linq;

namespace SearchAThing
{

    public partial class OpenGlControl
    {

        public void Zoom(float scale)
        {
            if (Perspective)
            {
                CameraPos -= scale * (CameraTarget - CameraPos);
            }
            else
            {
                OrthoZoom += OrthoZoom * scale;
            }
        }

        public void ZoomIn(float scale = 1e-1f) => Zoom(scale);
        public void ZoomOut(float scale = -1e-1f) => Zoom(scale);

        public void ZoomFit()
        {
            var ar = Bounds.Size.AspectRatio;
            var ccs = CameraCS;

            CameraTarget = new Vector3();

            if (Perspective)
            {
                double? dMax = null;
                double dMin = 0d;

                var cotgFov_2 = 1d / Tan(FovDeg.ToRad() / 2);

                var mm = GetModelMatrix();

                foreach (var _p in Model.BBox.Points)
                {
                    var p = (Vector3D)Vector3.Transform(_p, mm);

                    var pcs = p.ToUCS(ccs);

                    // frustum (side)                                
                    var d = Abs(pcs.Y) * cotgFov_2;
                    if (!dMax.HasValue)
                    {
                        dMax = d;
                        dMin = d;
                    }
                    else
                    {
                        dMax = Max(dMax.Value, d);
                        dMin = Min(dMin, d);
                    }

                    // frustum (top)                            
                    d = Abs(pcs.X) * cotgFov_2;

                    dMax = Max(dMax.Value, d);
                    dMin = Min(dMin, d);
                }

                CameraPos = (Vector3D)CameraTarget + dMax.Value * ccs.BaseZ;
            }
            else
            {
                CameraPos = (Vector3D)CameraTarget + ccs.BaseZ;

                var mm = GetModelMatrix();

                var obbox = new BBox3D(Model.BBox.Points.Select(w => w.ToUCS(ccs)));
                var obboxSize = obbox.Size;
                var obboxWidth = obboxSize.X;
                var obboxHeight = obboxSize.Y;
                var oAr = obboxWidth / obboxHeight;

                var obboxR = new BBox3D(Model.BBox.Points.Select(w => ((Vector3D)Vector3.Transform(w, mm)).ToUCS(ccs)));
                var obboxRSize = obboxR.Size;
                var obboxRWidth = obboxRSize.X;
                var obboxRHeight = obboxRSize.Y;
                var oArR = obboxRWidth / obboxRHeight;

                var ogWidth = (obboxWidth * ar);
                var ogHeight = (obboxHeight * oAr);

                if (oArR > ar)
                    OrthoZoom = (float)(1d / ogWidth * obboxRWidth);
                else
                    OrthoZoom = (float)(1d / ogHeight * obboxRHeight);
            }
        }

        public void ResetRotation()
        {
            RotationMatrix = Matrix4x4.Identity;
            UpdateYawPitchRollFromRotationMatrix();
        }

        public virtual Matrix4x4 GetModelMatrix()
        {
            var tr = new Vector3(TranslationX, TranslationY, TranslationZ);

            return
                Matrix4x4.CreateTranslation(tr)
                *
                RotationMatrix;
        }

        public virtual Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.CreateLookAt(CameraPos, CameraTarget, CameraUp);
        }

        public virtual Matrix4x4 GetProjectionMatrix()
        {
            var ar = Bounds.Size.AspectRatio;

            if (Perspective)
                return Matrix4x4.CreatePerspectiveFieldOfView(FovDeg.ToRad(), (float)ar, Near, Far);
            else
            {
                var ccs = CameraCS;
                CameraPos = (Vector3D)CameraTarget + ccs.BaseZ;

                var mm = GetModelMatrix();

                var obbox = new BBox3D(Model.BBox.Points.Select(w => w.ToUCS(ccs)));
                var obboxSize = obbox.Size;
                var obboxWidth = obboxSize.X;
                var obboxHeight = obboxSize.Y;
                var oAr = obboxWidth / obboxHeight;

                var ogWidth = (obboxWidth * ar);
                var ogHeight = (obboxHeight * oAr);

                return Matrix4x4.CreateOrthographic(
                    (float)ogWidth * OrthoZoom,
                    (float)ogHeight * OrthoZoom,
                    Near, Far);
            }
        }

        // camera helpers

        void CameraTop(BBox3D bbox)
        {
            CameraTarget = new Vector3();
            CameraUp = new Vector3(0, 1, 0);
            CameraPos = new Vector3(0, 0, (float)(bbox.Max.Z + bbox.Size.Z));
        }

        void CameraLeft(BBox3D bbox)
        {
            CameraTarget = new Vector3();
            CameraUp = new Vector3(0, 0, 1);
            CameraPos = new Vector3((float)(bbox.Min.X - bbox.Size.X), 0, 0);
        }

        void CameraFront(BBox3D bbox)
        {
            CameraTarget = new Vector3();
            CameraUp = new Vector3(0, 0, 1);
            CameraPos = new Vector3(0, (float)(bbox.Min.Y - bbox.Size.Y), 0);
        }

        void CameraRight(BBox3D bbox)
        {
            CameraTarget = new Vector3();
            CameraUp = new Vector3(0, 0, 1);
            CameraPos = new Vector3((float)(bbox.Max.X + bbox.Size.X), 0, 0);
        }

        void CameraBack(BBox3D bbox)
        {
            CameraTarget = new Vector3();
            CameraUp = new Vector3(0, 0, 1);
            CameraPos = new Vector3(0, (float)(bbox.Max.Y + bbox.Size.Y), 0);
        }

        void CameraBottom(BBox3D bbox)
        {
            CameraTarget = new Vector3();
            CameraUp = new Vector3(0, 1, 0);
            CameraPos = new Vector3(0, 0, (float)(bbox.Min.Z - bbox.Size.Z));
        }

        // views

        public void ViewTop()
        {
            var bbox = this.Model.BBox;
            if (bbox.IsEmpty) return;

            Translation = -bbox.Middle;
            RotationMatrix = Matrix4x4.Identity;
            UpdateYawPitchRollFromRotationMatrix();
            CameraTop(bbox);
            ZoomFit();
        }

        public void ViewLeft()
        {
            var bbox = this.Model.BBox;
            if (bbox.IsEmpty) return;

            Translation = -bbox.Middle;
            RotationMatrix =
                Matrix4x4.CreateRotationZ(-(float)PI / 2) *
                Matrix4x4.CreateRotationX(-(float)PI / 2);
            UpdateYawPitchRollFromRotationMatrix();
            CameraTop(bbox);
            ZoomFit();
        }

        public void ViewFront()
        {
            var bbox = Model.BBox;
            if (bbox.IsEmpty) return;

            Translation = -bbox.Middle;
            RotationMatrix = Matrix4x4.CreateRotationX(-(float)PI / 2);
            UpdateYawPitchRollFromRotationMatrix();
            CameraTop(bbox);
            ZoomFit();
        }

        public void ViewRight()
        {
            var bbox = this.Model.BBox;
            if (bbox.IsEmpty) return;

            Translation = -bbox.Middle;
            RotationMatrix =
                Matrix4x4.CreateRotationZ((float)PI / 2) *
                Matrix4x4.CreateRotationX(-(float)PI / 2);
            UpdateYawPitchRollFromRotationMatrix();
            CameraTop(bbox);
            ZoomFit();
        }

        public void ViewBack()
        {
            var bbox = Model.BBox;
            if (bbox.IsEmpty) return;

            Translation = -bbox.Middle;
            RotationMatrix = Matrix4x4.CreateFromYawPitchRoll(0, -(float)PI / 2, -(float)PI);
            UpdateYawPitchRollFromRotationMatrix();
            CameraTop(bbox);
            ZoomFit();
        }

        public void ViewBottom()
        {
            var bbox = Model.BBox;
            if (bbox.IsEmpty) return;

            Translation = -bbox.Middle;
            RotationMatrix = Matrix4x4.CreateFromYawPitchRoll((float)PI, 0, 0);
            UpdateYawPitchRollFromRotationMatrix();
            CameraTop(bbox);
            ZoomFit();
        }

        /// <summary>
        /// set view using rotation from yaw,pitch,roll
        /// </summary>
        /// <param name="yawDeg">rotation around Yaxis</param>
        /// <param name="pitchDeg">rotation around Xaxis</param>
        /// <param name="rollDeg">rotation around Zaxis</param>
        public void ViewFromYawPitchRoll(float yawDeg, float pitchDeg, float rollDeg)
        {
            var bbox = Model.BBox;
            if (bbox.IsEmpty) return;

            Translation = -bbox.Middle;
            RotationMatrix = Matrix4x4.CreateFromYawPitchRoll(yawDeg.ToRad(), pitchDeg.ToRad(), rollDeg.ToRad());
            CameraTop(bbox);
            ZoomFit();
        }

        public void SetViewProjection(bool perspective)
        {
            Perspective = perspective;
            ZoomFit();
        }

    }

}