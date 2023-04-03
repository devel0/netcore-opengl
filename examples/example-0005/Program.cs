namespace example;

// example-0005
// draw box with keyboard face toggler
//
// - use application shortcut Control + (L:left R:Right F:Front B:Back T:Top O:bOttom) to toggle cube faces visibility

class Program
{

    static void Main(string[] args)
    {
        InitAvalonia();

        // custom initial window size
        var w = GLWindow.Create(width: 1100, height: 600);

        var visibleFaces = new[] { true, true, true, true, true, true };

        Box? box = null;

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            glModel.Clear();

            var sizef = glCtl.Size();
            var sw = sizef.X;
            var sh = sizef.Y;

            // unless csBaseIsMiddle given the box csBase is the plane centered on the base of the box
            // while csSize specify the width(x), height(y), elevation(z)            
            box = new Box(
                cs: MakeCS(origin: Vector3.Zero, N: Vector3.UnitZ), // alternatively MakeWCS()
                csSize: new Vector3(10, 20, 5));

            // each face of the cube is a figure composed by 2 triangles

            box.SideTop.SetColor(Color.Red); // 1
            box.SideBottom.SetColor(Color.Green); // 2
            box.SideLeft.SetColor(Color.Blue); // 3
            box.SideRight.SetColor(Color.Magenta); // 4
            box.SideFront.SetColor(Color.White); // 5
            box.SideBack.SetColor(Color.Cyan); // 6

            glModel.AddFigure(MakeWCSFigure(origin: box.Center));

            Debug.WriteLine($"SW:{sw} SH:{sh}");

            // box.Sides enumerate over all box sides representing entire figure
            glModel.AddFigure(box.Sides);

            box.SideTop.Visible = false;

            glCtl.CameraView(CameraViewType.FrontTopLeft);
        };

        w.KeyDown += (sender, e) =>
        {
            if (box is not null)
            {
                if (e.KeyModifiers == KeyModifiers.Control)
                    switch (e.Key)
                    {
                        case Avalonia.Input.Key.T: box.SideTop.Visible = !box.SideTop.Visible; break;
                        case Avalonia.Input.Key.O: box.SideBottom.Visible = !box.SideBottom.Visible; break;
                        case Avalonia.Input.Key.L: box.SideLeft.Visible = !box.SideLeft.Visible; break;
                        case Avalonia.Input.Key.R: box.SideRight.Visible = !box.SideRight.Visible; break;
                        case Avalonia.Input.Key.F: box.SideFront.Visible = !box.SideFront.Visible; break;
                        case Avalonia.Input.Key.B: box.SideBack.Visible = !box.SideBack.Visible; break;
                    }

                w.GLControlSplit?.Invalidate();
            }
        };

        w.ShowSync();
    }

}
