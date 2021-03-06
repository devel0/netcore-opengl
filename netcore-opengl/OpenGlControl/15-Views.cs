using System.Numerics;
using Silk.NET.OpenGL;
using static System.Math;
using System.Linq;
using System.Collections.Generic;

namespace SearchAThing
{

    public partial class OpenGlControl
    {

        public void Zoom(float scale)
        {
            if (Perspective)
            {
                var delta = scale * (CameraTarget - CameraPos);
                CameraPos -= delta;
            }
            else
            {
                OrthoZoom += OrthoZoom * scale;
            }
        }

        public void ZoomIn(float scale = 1e-1f) => Zoom(scale);
        public void ZoomOut(float scale = -1e-1f) => Zoom(scale);

        internal class FitNfo
        {
            internal Vector3D p;
            internal Vector3D pmv;
            internal double Md;
            internal double pdx;
            internal double pdy;
        };

        public void ZoomFit()
        {
            var boundsSize = Bounds.Size;
            if (boundsSize.Width == 0 || boundsSize.Height == 0)
                return;
            var ar = boundsSize.AspectRatio;

            var ccs = CameraCS;
            var cameraDir = Vector3.Normalize(CameraPos - CameraTarget);
            var mm = ModelMatrix;

            if (Perspective) // TODO : centering
            {
                var bbox = Model.BBox;
                var bboxMiddle = bbox.Middle;
                var mmDec = ModelMatrix.Decompose();

                ModelMatrix =
                    Matrix4x4.CreateTranslation(-bboxMiddle)
                    *
                    Matrix4x4.CreateFromQuaternion(mmDec.rotation)
                    *
                    Matrix4x4.CreateScale(mmDec.scale);

                var fov = FovDeg.ToRad();
                var alpha = fov / 2;
                var nfos = new List<FitNfo>();
                var M = bboxMiddle;
                foreach (var p in Model.BBox.Points)
                {
                    var nfo = new FitNfo();

                    nfos.Add(nfo);

                    nfo.p = p;
                    nfo.pmv = Vector3.Transform(p, ModelMatrix * ViewMatrix);
                    nfo.pdx = Abs(nfo.pmv.Y) / Tan(alpha);
                    nfo.pdy = Abs(nfo.pmv.X / ar) / Tan(alpha);
                    nfo.Md = ((Vector3)nfo.pmv - Vector3.Transform(M, ModelMatrix * ViewMatrix)).Z;
                }

                var matchPDx = nfos.OrderByDescending(w => w.pdx + w.Md).First();
                var matchPDy = nfos.OrderByDescending(w => w.pdy + w.pdy).First();

                var maxPDx = matchPDx.pdx + matchPDx.Md;
                var maxPDy = matchPDx.pdy + matchPDx.Md;

                CameraPos = CameraTarget + (float)Max(maxPDx, maxPDy) * cameraDir;
            }
            else
            {
                CameraPos = (Vector3D)CameraTarget + ccs.BaseZ;

                var obbox = new BBox3D(Model.BBox.Points.Select(w => (Vector3D)Vector3.Transform(w, ViewMatrix)));
                var obboxSize = obbox.Size;
                var obboxWidth = obboxSize.X;
                var obboxHeight = obboxSize.Y;
                var oAr = obboxWidth / obboxHeight;

                var obboxR = new BBox3D(Model.BBox.Points.Select(w => (Vector3D)Vector3.Transform(w, ModelMatrix * ViewMatrix)));
                var obboxRSize = obboxR.Size;
                var obboxRWidth = obboxRSize.X;
                var obboxRHeight = obboxRSize.Y;
                var oArR = obboxRWidth / obboxRHeight;

                var ogWidth = (obboxWidth * ar);
                var ogHeight = (obboxHeight * oAr);

                var mmDec = ModelMatrix.Decompose();

                ModelMatrix =
                    Matrix4x4.CreateTranslation(-Model.BBox.Middle)
                    *
                    Matrix4x4.CreateFromQuaternion(mmDec.rotation)
                    *
                    Matrix4x4.CreateScale(mmDec.scale);

                if (oArR > ar)
                    OrthoZoom = (float)(1d / ogWidth * obboxRWidth);
                else
                    OrthoZoom = (float)(1d / ogHeight * obboxRHeight);
            }
        }

        public void ResetRotation()
        {
            ModelMatrix = Matrix4x4.Identity;
        }

        // camera helpers

        /// <summary>
        /// set CameraTarget=(0,0,0) ; CameraUp=(0,1,0) ; CameraPos=(0,0,dst)
        /// </summary>
        /// <param name="bbox">used to compute dst</param>
        public void CameraTop(BBox3D bbox)
        {
            CameraTarget = new Vector3();
            CameraUp = new Vector3(0, 1, 0);
            CameraPos = new Vector3(0, 0, (float)(bbox.Max.Z + bbox.Size.Z));
        }

        /// <summary>
        /// set CameraTarget=(0,0,0) ; CameraUp=(0,0,1) ; CameraPos=(dst,0,0)
        /// </summary>
        /// <param name="bbox">used to compute dst</param>
        public void CameraLeft(BBox3D bbox)
        {
            CameraTarget = new Vector3();
            CameraUp = new Vector3(0, 0, 1);
            CameraPos = new Vector3((float)(bbox.Min.X - bbox.Size.X), 0, 0);
        }

        /// <summary>
        /// set CameraTarget=(0,0,0) ; CameraUp=(0,0,1) ; CameraPos=(0,dst,0)
        /// </summary>
        /// <param name="bbox">used to compute dst</param>
        public void CameraFront(BBox3D bbox)
        {
            CameraTarget = new Vector3();
            CameraUp = new Vector3(0, 0, 1);
            CameraPos = new Vector3(0, (float)(bbox.Min.Y - bbox.Size.Y), 0);
        }

        /// <summary>
        /// set CameraTarget=(0,0,0) ; CameraUp=(0,0,1) ; CameraPos=(dst,0,0)
        /// </summary>
        /// <param name="bbox">used to compute dst</param>
        public void CameraRight(BBox3D bbox)
        {
            CameraTarget = new Vector3();
            CameraUp = new Vector3(0, 0, 1);
            CameraPos = new Vector3((float)(bbox.Max.X + bbox.Size.X), 0, 0);
        }

        /// <summary>
        /// set CameraTarget=(0,0,0) ; CameraUp=(0,0,1) ; CameraPos=(dst,0,0)
        /// </summary>
        /// <param name="bbox">used to compute dst</param>
        public void CameraBack(BBox3D bbox)
        {
            CameraTarget = new Vector3();
            CameraUp = new Vector3(0, 0, 1);
            CameraPos = new Vector3(0, (float)(bbox.Max.Y + bbox.Size.Y), 0);
        }

        /// <summary>
        /// set CameraTarget=(0,0,0) ; CameraUp=(0,1,0) ; CameraPos=(0,0,dst)
        /// </summary>
        /// <param name="bbox">used to compute dst</param>
        public void CameraBottom(BBox3D bbox)
        {
            CameraTarget = new Vector3();
            CameraUp = new Vector3(0, 1, 0);
            CameraPos = new Vector3(0, 0, (float)(bbox.Min.Z - bbox.Size.Z));
        }

        // views

        public Matrix4x4 BaseModelMatrix => ModelMatrix = Matrix4x4.CreateTranslation(-this.Model.BBox.Middle);

        public void ViewTop()
        {
            var bbox = this.Model.BBox;
            if (bbox.IsEmpty) return;

            ModelMatrix = BaseModelMatrix;
            CameraTop(bbox);
            ZoomFit();
        }

        public void ViewLeft(bool resetModelMatrix = true)
        {
            var bbox = this.Model.BBox;
            if (bbox.IsEmpty) return;

            if (resetModelMatrix) ModelMatrix = BaseModelMatrix;
            CameraLeft(bbox);
            ZoomFit();
        }

        public void ViewFront(bool resetModelMatrix = true)
        {
            var bbox = Model.BBox;
            if (bbox.IsEmpty) return;

            if (resetModelMatrix) ModelMatrix = BaseModelMatrix;
            CameraFront(bbox);
            ZoomFit();
        }

        public void ViewRight(bool resetModelMatrix = true)
        {
            var bbox = this.Model.BBox;
            if (bbox.IsEmpty) return;

            if (resetModelMatrix) ModelMatrix = BaseModelMatrix;
            CameraRight(bbox);
            ZoomFit();
        }

        public void ViewBack(bool resetModelMatrix = true)
        {
            var bbox = Model.BBox;
            if (bbox.IsEmpty) return;

            if (resetModelMatrix) ModelMatrix = BaseModelMatrix;
            CameraBack(bbox);
            ZoomFit();
        }

        public void ViewBottom(bool resetModelMatrix = true)
        {
            var bbox = Model.BBox;
            if (bbox.IsEmpty) return;

            if (resetModelMatrix) ModelMatrix = BaseModelMatrix;
            CameraBottom(bbox);
            ZoomFit();
        }

        public void ViewSouthWest(bool resetModelMatrix = true)
        {
            var bbox = Model.BBox;
            if (bbox.IsEmpty) return;

            CameraTop(bbox);
            if (resetModelMatrix)
            {
                ModelMatrix = BaseModelMatrix;
                ViewModelRoll((float)(PI / 4));
                ViewModelVRot((float)(PI / 2 - Atan(1d / Sqrt(2))));
            }
            ZoomFit();
        }

        public void ViewSouthEast(bool resetModelMatrix = true)
        {
            var bbox = Model.BBox;
            if (bbox.IsEmpty) return;

            CameraTop(bbox);
            if (resetModelMatrix)
            {
                ModelMatrix = BaseModelMatrix;
                ViewModelRoll((float)(-PI / 4));
                ViewModelVRot((float)(PI / 2 - Atan(1d / Sqrt(2))));
            }
            ZoomFit();
        }

        public void ViewNorthEast(bool resetModelMatrix = true)
        {
            var bbox = Model.BBox;
            if (bbox.IsEmpty) return;

            CameraTop(bbox);
            if (resetModelMatrix)
            {
                ModelMatrix = BaseModelMatrix;
                ViewModelRoll((float)(-PI / 4 - PI / 2));
                ViewModelVRot((float)(PI / 2 - Atan(1d / Sqrt(2))));
            }
            ZoomFit();
        }

        public void ViewNorthWest(bool resetModelMatrix = true)
        {
            var bbox = Model.BBox;
            if (bbox.IsEmpty) return;

            CameraTop(bbox);
            if (resetModelMatrix)
            {
                ModelMatrix = BaseModelMatrix;
                ViewModelRoll((float)(-PI / 4 - PI));
                ViewModelVRot((float)(PI / 2 - Atan(1d / Sqrt(2))));
            }
            ZoomFit();
        }

        public void SetViewProjection(bool perspective)
        {
            Perspective = perspective;
            ZoomFit();
        }

    }

}