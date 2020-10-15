using System.Numerics;
using Silk.NET.OpenGL;
using static System.Math;
using System.Linq;
using System.Collections.Generic;

namespace SearchAThing
{

    public partial class OpenGlControl
    {

        /// <summary>
        /// rotate model like as roll applied (depends on view)
        /// </summary>
        /// <param name="angle">roll angle</param>
        public void ViewModelRoll(float angle)
        {
            var bboxMiddle = Model.BBox.Middle;
            var MMInverseDecomposed = ModelMatrix.Inverse().Decompose();
            var VMInverseDecomposed = ViewMatrix.Inverse().Decompose();

            var oZaxis = Vector3.Transform(Vector3D.ZAxis, VMInverseDecomposed.rotation);
            var wZaxis = Vector3.Transform(oZaxis, MMInverseDecomposed.rotation);

            ModelMatrix =
                Matrix4x4.CreateTranslation(-bboxMiddle)
                *
                Matrix4x4.CreateFromAxisAngle(wZaxis, (float)angle)
                *
                Matrix4x4.CreateTranslation(bboxMiddle)
                *
                ModelMatrix;
        }

        /// <summary>
        /// rotate model like a vertical rotation applied (depends on view)
        /// </summary>
        /// <param name="angle">vertical rotation angle</param>
        public void ViewModelVRot(float angle)
        {
            var bboxMiddle = Model.BBox.Middle;
            var MMInverseDecomposed = ModelMatrix.Inverse().Decompose();
            var VMInverseDecomposed = ViewMatrix.Inverse().Decompose();

            var oXaxis = Vector3.Transform(Vector3D.XAxis, VMInverseDecomposed.rotation);

            var wXaxis = Vector3.Transform(oXaxis, MMInverseDecomposed.rotation);

            ModelMatrix =
                Matrix4x4.CreateTranslation(-bboxMiddle)
                *
                Matrix4x4.CreateFromAxisAngle(wXaxis, (float)-angle)
                *
                Matrix4x4.CreateTranslation(bboxMiddle)
                *
                ModelMatrix;
        }

        /// <summary>
        /// rotate model like an horizontal rotation applied (depends on view)
        /// </summary>
        /// <param name="angle">horizontal rotation angle</param>
        public void ViewModelHRot(float angle)
        {
            var bboxMiddle = Model.BBox.Middle;
            var MMInverseDecomposed = ModelMatrix.Inverse().Decompose();
            var VMInverseDecomposed = ViewMatrix.Inverse().Decompose();

            var oYaxis = Vector3.Transform(Vector3D.YAxis, VMInverseDecomposed.rotation);
            var wYaxis = Vector3.Transform(oYaxis, MMInverseDecomposed.rotation);

            ModelMatrix =
                Matrix4x4.CreateTranslation(-bboxMiddle)
                *
                Matrix4x4.CreateFromAxisAngle(wYaxis, (float)-angle)
                *
                Matrix4x4.CreateTranslation(bboxMiddle)
                *
                orbitPressMatrixSource.ModelMatrix;
        }

    }

}