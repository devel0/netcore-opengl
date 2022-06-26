using System;
using System.Numerics;
using Silk.NET.OpenGL;
using static System.Math;

namespace SearchAThing
{

    public struct Circle2
    {
        public float Radius;
        public Vector2 Center;
        public bool Contains(Avalonia.Point pt) =>
            (new Vector2((float)pt.X, (float)pt.Y) - Center).Length() < Radius;

        public Circle3D ToCircle3D(double tolLen) => 
            new Circle3D(CoordinateSystem3D.WCS.Move(Center), Radius);
    }

    public class OrbitGeometry
    {

        public OrbitGeometry()
        {
        }

        public double sw { get; private set; }
        public double sh { get; private set; }

        public Circle2 Main { get; private set; }
        public Circle2 Top { get; private set; }
        public Circle2 Bottom { get; private set; }
        public Circle2 Left { get; private set; }
        public Circle2 Right { get; private set; }

        public void Recompute(double sw, double sh)
        {
            this.sw = sw;
            this.sh = sh;

            var s = Min(sw, sh);
            Main = new Circle2 { Center = new Vector2((float)(sw / 2), (float)(sh / 2)), Radius = (float)(s / 3) };

            var miniCircleRadius = Main.Radius / 12;

            Top = new Circle2 { Center = Main.Center + new Vector2(0, Main.Radius), Radius = miniCircleRadius };
            Bottom = new Circle2 { Center = Main.Center - new Vector2(0, Main.Radius), Radius = miniCircleRadius };
            Left = new Circle2 { Center = Main.Center - new Vector2(Main.Radius, 0), Radius = miniCircleRadius };
            Right = new Circle2 { Center = Main.Center + new Vector2(Main.Radius, 0), Radius = miniCircleRadius };
        }

    }

}