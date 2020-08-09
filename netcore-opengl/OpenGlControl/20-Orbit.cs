using System.Numerics;
using Avalonia.Input;
using static System.Math;

namespace SearchAThing
{

    public enum OrbitFunction
    {
        None,
        Vertical,
        Horizontal,
        Combined,
        Roll
    }

    public partial class OpenGlControl
    {
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

            OrbitPressCameraPos = CameraPos;
            OrbitPressTargetCameraPos = CameraTarget;
            OrbitPressCameraUp = CameraUp;
            OrbitPressRotationMatrix = RotationMatrix;
        }

        void EvalOrbitPointerReleased(PointerPoint p)
        {
            orbitFunction = OrbitFunction.None;
        }

        void UpdateYawPitchRollFromRotationMatrix()
        {
            var q = RotationMatrix.ToEulerAngles();

            RotationMatrixX = q.X;
            RotationMatrixY = q.Y;
            RotationMatrixZ = q.Z;

            _RotationX = RotationMatrixX.ToDeg();
            _RotationY = RotationMatrixY.ToDeg();
            _RotationZ = RotationMatrixZ.ToDeg();

            RaisePropertyChanged(RotationXProperty, _RotationX, _RotationX);
            RaisePropertyChanged(RotationYProperty, _RotationY, _RotationY);
            RaisePropertyChanged(RotationZProperty, _RotationZ, _RotationZ);
        }

        void EvalOrbitPointerMoved(PointerPoint p)
        {
            if (orbitGeometry == null) return;

            if (orbitFunction == OrbitFunction.None)
            {
                return;
            }

            orbitMovedPosition = p;

            switch (orbitFunction)
            {

                case OrbitFunction.Horizontal:
                    {
                        BBox3D obboxBefore = null;
                        if (!Perspective) obboxBefore = OrthoBBox;

                        var moveDelta = orbitMovedPosition.Position - orbitPressPosition.Position;

                        var dx = moveDelta.X;

                        var angleY = (-PI * dx / (2 * orbitGeometry.Main.Radius));

                        RotationMatrix = OrbitPressRotationMatrix * Matrix4x4.CreateRotationY((float)-angleY);

                        UpdateYawPitchRollFromRotationMatrix();
                    }
                    break;

                case OrbitFunction.Vertical:
                    {
                        BBox3D obboxBefore = null;
                        if (!Perspective) obboxBefore = OrthoBBox;

                        var moveDelta = orbitMovedPosition.Position - orbitPressPosition.Position;

                        var dy = moveDelta.Y;

                        var angleX = PI * dy / (2 * orbitGeometry.Main.Radius);

                        RotationMatrix = OrbitPressRotationMatrix *
                            Matrix4x4.CreateRotationX((float)-angleX);

                        UpdateYawPitchRollFromRotationMatrix();
                    }
                    break;

                case OrbitFunction.Combined:
                    {
                        var moveDelta = orbitMovedPosition.Position - orbitPressPosition.Position;

                        var dx = moveDelta.X;
                        var dy = moveDelta.Y;

                        var angleX = PI * dy / (2 * orbitGeometry.Main.Radius);
                        var angleY = PI * dx / (2 * orbitGeometry.Main.Radius);

                        RotationMatrix = OrbitPressRotationMatrix *
                            Matrix4x4.CreateRotationY((float)angleY) *
                            Matrix4x4.CreateRotationX((float)-angleX);

                        UpdateYawPitchRollFromRotationMatrix();
                    }
                    break;

                case OrbitFunction.Roll:
                    {
                        BBox3D obboxBefore = null;
                        if (!Perspective) obboxBefore = OrthoBBox;

                        var sc = new Vector3D(Bounds.Width / 2, Bounds.Height / 2);
                        var a = sc.LineTo(orbitPressPosition.Position.Eval(w => new Vector3D(w.X, w.Y)));
                        var b = sc.LineTo(orbitMovedPosition.Position.Eval(w => new Vector3D(w.X, w.Y)));
                        var alphaRad = a.V.AngleToward(1e-6, b.V, Vector3D.ZAxis);

                        RotationMatrix = OrbitPressRotationMatrix *
                            Matrix4x4.CreateRotationZ((float)alphaRad);

                        UpdateYawPitchRollFromRotationMatrix();
                    }
                    break;
            }
        }

    }

}