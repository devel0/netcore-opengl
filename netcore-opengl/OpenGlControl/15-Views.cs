using System.Numerics;
using Silk.NET.OpenGL;
using static System.Math;

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
                OrthoWidthScale += OrthoWidthScale * scale;
                OrthoHeightScale += OrthoHeightScale * scale;
            }
        }

        public void ZoomIn(float scale = 1e-1f) => Zoom(scale);
        public void ZoomOut(float scale = -1e-1f) => Zoom(scale);

        public void ZoomFit()
        {
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

                var os = OrthoBBox.Size;
                var ar = Bounds.Size.AspectRatio;
                if (os.X >= os.Y)
                {
                    OrthoWidthScale = 1f;
                    OrthoHeightScale = (float)(1d / ar);
                }
                else
                {
                    OrthoWidthScale = (float)ar;
                    OrthoHeightScale = 1f;
                }
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
            var ar = (float)(Bounds.Width / Bounds.Height);

            if (Perspective)
                return Matrix4x4.CreatePerspectiveFieldOfView(FovDeg.ToRad(), ar, Near, Far);
            else
            {
                var obbox = OrthoBBox;
                var os = obbox.Size;
                
                var obboxAr = obbox.Size.X / obbox.Size.Y;                

                var minx = obbox.Min.X;
                var maxx = obbox.Max.X;
                var middlex = (minx + maxx) / 2;

                var miny = obbox.Min.Y;
                var maxy = obbox.Max.Y;
                var middley = (miny + maxy) / 2;                

                var w = maxx - minx;
                var h = maxy - miny;

                return Matrix4x4.CreateOrthographic((float)w * OrthoWidthScale, (float)h * OrthoHeightScale, Near, Far);
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
            Translation = -bbox.Middle;
            RotationMatrix = Matrix4x4.Identity;
            UpdateYawPitchRollFromRotationMatrix();
            CameraTop(bbox);
            ZoomFit();
        }

        public void ViewLeft()
        {
            var bbox = this.Model.BBox;
            Translation = -bbox.Middle;
            RotationMatrix = Matrix4x4.CreateRotationY((float)PI / 2);
            UpdateYawPitchRollFromRotationMatrix();
            CameraTop(bbox);
            ZoomFit();
        }

        public void ViewFront()
        {
            var bbox = Model.BBox;
            Translation = -bbox.Middle;
            RotationMatrix = Matrix4x4.CreateRotationX(-(float)PI / 2);
            UpdateYawPitchRollFromRotationMatrix();
            CameraTop(bbox);
            ZoomFit();
        }

        public void ViewRight()
        {
            var bbox = this.Model.BBox;
            Translation = -bbox.Middle;
            RotationMatrix = Matrix4x4.CreateRotationY(-(float)PI / 2);
            UpdateYawPitchRollFromRotationMatrix();
            CameraTop(bbox);
            ZoomFit();
        }

        public void ViewBack()
        {
            var bbox = Model.BBox;
            Translation = -bbox.Middle;
            RotationMatrix = Matrix4x4.CreateFromYawPitchRoll(0, -(float)PI / 2, -(float)PI);
            UpdateYawPitchRollFromRotationMatrix();
            CameraTop(bbox);
            ZoomFit();
        }

        public void ViewBottom()
        {
            var bbox = Model.BBox;
            Translation = -bbox.Middle;
            RotationMatrix = Matrix4x4.CreateFromYawPitchRoll((float)PI, 0, 0);
            UpdateYawPitchRollFromRotationMatrix();
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