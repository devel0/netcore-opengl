using System;
using System.Numerics;
using Avalonia.Input;
using static System.Math;

namespace SearchAThing
{

    public enum OrbitFunction
    {
        None,

        /// <summary>
        /// rotate bbox model within its center around camera X axis
        /// </summary>
        Vertical,

        /// <summary>
        /// rotate bbox model within its center around camera Y axis
        /// </summary>
        Horizontal,

        /// <summary>
        /// rotate bbox model within its center around camera Z axis
        /// </summary>
        Roll,

        /// <summary>
        /// rotate bbox model within its center around camera X,Y axes
        /// </summary>
        Combined
    }

    public partial class OpenGlControl
    {

        // ==[ ZOOM ]==============================================================================

        void EvalZoomChanged(double deltaY)
        {
            if (deltaY > 0)
                ZoomOut();
            else
                ZoomIn();
        }

        // ==[ PAN ]===============================================================================

        GlControlMatrixSource panPressMatrixSource = null;

        public PointerPoint? panPressPosition { get; private set; }

        void EvalPanPointerMoved(PointerPoint p)
        {
            if (panPressPosition != null)
            {
                var ptcur = p.Position;
                var delta = ptcur - panPressPosition.Position;

                Pan(delta.X, delta.Y, panPressMatrixSource.ModelMatrix);
            }
        }

        void Pan(double screenDeltaX, double screenDeltaY, Matrix4x4? mm = null)
        {
            var dxs = screenDeltaX / Bounds.Width;
            var dys = screenDeltaY / Bounds.Height;
            
            var ccs = CameraCS;

            var pm = ProjectionMatrix;            

            // from ndc -1 to 1 = 2 to modelview
            var vsp = Vector3.Transform(new Vector3(2, 2, 0), pm.Inverse());

            var cd = (CameraTarget - CameraPos).Length();

            var deltaX = vsp.X * dxs * cd;
            var deltaY = vsp.Y * dys * cd;

            var deltaCamera = (Vector3)(ccs.BaseX * deltaX + ccs.BaseY * deltaY);

            ModelMatrix =
                (mm.HasValue ? mm.Value : ModelMatrix)
                *
                Matrix4x4.CreateTranslation(deltaCamera);
        }

        void EvalPanPointerReleased(PointerPoint p)
        {
            panPressPosition = null;
        }

        // ==[ ORBIT ]=============================================================================        

        GlControlMatrixSource orbitPressMatrixSource = null;

        public OrbitGeometry orbitGeometry { get; private set; } = new OrbitGeometry();
        OrbitFunction orbitFunction = OrbitFunction.None;
        public PointerPoint? orbitPressPosition { get; private set; }
        public PointerPoint? orbitMovedPosition { get; private set; }

        void EvalOrbitPointerPressed(PointerPoint pp)
        {
            if (orbitGeometry == null) return;

            if (orbitFunction != OrbitFunction.None) return;

            var p = pp.Position;

            if (pp.Properties.IsLeftButtonPressed && ShowOrbit)
            {
                if (orbitGeometry.Top.Contains(p) || orbitGeometry.Bottom.Contains(p))
                    orbitFunction = OrbitFunction.Vertical;
                else if (orbitGeometry.Left.Contains(p) || orbitGeometry.Right.Contains(p))
                    orbitFunction = OrbitFunction.Horizontal;
                else if (orbitGeometry.Main.Contains(p))
                    orbitFunction = OrbitFunction.Combined;
                else
                    orbitFunction = OrbitFunction.Roll;
            }
            else if (ShiftKeyPressed && pp.Properties.IsMiddleButtonPressed)
            {
                orbitFunction = OrbitFunction.Combined;
            }
            orbitPressPosition = pp;

            orbitPressMatrixSource = new GlControlMatrixSource(this);
        }

        void EvalOrbitPointerReleased(PointerPoint p)
        {
            orbitFunction = OrbitFunction.None;
        }

        void EvalOrbitPointerMoved(PointerPoint p)
        {
            if (orbitGeometry == null) return;

            if (orbitFunction == OrbitFunction.None)
            {
                return;
            }

            orbitMovedPosition = p;

            var bboxMiddle = Model.BBox.Middle;

            var orbitPressMMInverseDecomposed = orbitPressMatrixSource.ModelMatrix.Inverse().Decompose();
            var orbitPressVMInverseDecomposed = orbitPressMatrixSource.ViewMatrix.Inverse().Decompose();

            var ccs = CameraCS;

            switch (orbitFunction)
            {

                case OrbitFunction.Horizontal:
                    {
                        var moveDelta = orbitMovedPosition.Position - orbitPressPosition.Position;
                        var dx = moveDelta.X;
                        var angleYRad = (-PI * dx / (2 * orbitGeometry.Main.Radius));

                        var oYaxis = Vector3.Transform(Vector3D.YAxis, orbitPressVMInverseDecomposed.rotation);

                        var wYaxis = Vector3.Transform(oYaxis, orbitPressMMInverseDecomposed.rotation);

                        ModelMatrix =
                            Matrix4x4.CreateTranslation(-bboxMiddle)
                            *
                            Matrix4x4.CreateFromAxisAngle(wYaxis, (float)-angleYRad)
                            *
                            Matrix4x4.CreateTranslation(bboxMiddle)
                            *
                            orbitPressMatrixSource.ModelMatrix;
                    }
                    break;

                case OrbitFunction.Vertical:
                    {
                        var moveDelta = orbitMovedPosition.Position - orbitPressPosition.Position;
                        var dy = moveDelta.Y;
                        var angleXRad = PI * dy / (2 * orbitGeometry.Main.Radius);

                        var oXaxis = Vector3.Transform(Vector3D.XAxis, orbitPressVMInverseDecomposed.rotation);

                        var wXaxis = Vector3.Transform(oXaxis, orbitPressMMInverseDecomposed.rotation);

                        ModelMatrix =
                            Matrix4x4.CreateTranslation(-bboxMiddle)
                            *
                            Matrix4x4.CreateFromAxisAngle(wXaxis, (float)-angleXRad)
                            *
                            Matrix4x4.CreateTranslation(bboxMiddle)
                            *
                            orbitPressMatrixSource.ModelMatrix;
                    }
                    break;

                case OrbitFunction.Combined:
                    {
                        var r = orbitGeometry.Main.Radius;

                        // https://www.khronos.org/opengl/wiki/Object_Mouse_Trackball

                        Vector3 getArcballVector(float x, float y)
                        {
                            var P = new Vector3((x - (float)Bounds.Width / 2) / r, (y - (float)Bounds.Height / 2) / r, 0);

                            var OP_squared = P.X * P.X + P.Y * P.Y;

                            if (OP_squared <= 1)
                                P = new Vector3(P.X, P.Y, (float)Sqrt(1 - OP_squared));
                            else
                                P = P.Normalized();

                            return P;
                        }

                        var va = getArcballVector((float)orbitPressPosition.Position.X, (float)orbitPressPosition.Position.Y);
                        var vb = getArcballVector((float)orbitMovedPosition.Position.X, (float)orbitMovedPosition.Position.Y);

                        var angle = Acos(Min(1, Vector3.Dot(va, vb)));
                        var axisInCC = Vector3.Cross(va, vb).Normalized();

                        var oAxis = Vector3.Transform(axisInCC, orbitPressVMInverseDecomposed.rotation);

                        var wAxis = Vector3.Transform(oAxis, orbitPressMMInverseDecomposed.rotation);

                        ModelMatrix =
                            Matrix4x4.CreateTranslation(-bboxMiddle)
                            *
                            Matrix4x4.CreateFromAxisAngle(wAxis, (float)angle)
                            *
                            Matrix4x4.CreateTranslation(bboxMiddle)
                            *
                            orbitPressMatrixSource.ModelMatrix;
                    }
                    break;

                case OrbitFunction.Roll:
                    {
                        var sc = new Vector3D(Bounds.Width / 2, Bounds.Height / 2);
                        var a = sc.LineTo(orbitPressPosition.Position.Eval(w => new Vector3D(w.X, w.Y)));
                        var b = sc.LineTo(orbitMovedPosition.Position.Eval(w => new Vector3D(w.X, w.Y)));
                        var angleZRad = a.V.AngleToward(1e-6, b.V, Vector3D.ZAxis);

                        var oZaxis = Vector3.Transform(Vector3D.ZAxis, orbitPressVMInverseDecomposed.rotation);

                        var wZaxis = Vector3.Transform(oZaxis, orbitPressMMInverseDecomposed.rotation);

                        ModelMatrix =
                            Matrix4x4.CreateTranslation(-bboxMiddle)
                            *
                            Matrix4x4.CreateFromAxisAngle(wZaxis, (float)angleZRad)
                            *
                            Matrix4x4.CreateTranslation(bboxMiddle)
                            *
                            orbitPressMatrixSource.ModelMatrix;

                    }
                    break;
            }
        }

    }

}