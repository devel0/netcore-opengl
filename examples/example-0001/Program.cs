namespace example;

// example-0001
// random lines ( console program )
//
// - press and hold added keybinding 'space' to animate the scene
// - and left click to rotate the model
// - or wheel to zoom or middle button to pan

class Program
{

    static void Main(string[] args)
    {
        InitAvalonia();

        var w = GLWindow.Create();

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            var glModel = glCtl.GLModel;

            glModel.Clear();

            var L = 100;
            var rnd = new Random();
            var N = 100;

            // create line with different colors on from, to vertexes
            {
                var xLine = GLLine.PointV(Vector3.Zero, new Vector3(L, 0, 0), Color.White, Color.Red);
                var yLine = GLLine.PointV(Vector3.Zero, new Vector3(0, L, 0), Color.White, Color.Green);
                var zLine = GLLine.PointV(Vector3.Zero, new Vector3(0, 0, L), Color.White, Color.Blue);

                glModel.AddFigure(new GLLineFigure(xLine, yLine, zLine));
            }

            var lines = new List<GLLine>();

            for (int i = 0; i < N; ++i)
            {
                var rnds = Enumerable.Range(0, 6).Select(r => rnd.NextSingle()).ToList();

                var color = Color.FromArgb((byte)(rnds[0] * 255), (byte)(rnds[1] * 255), (byte)(rnds[2] * 255));

                // create random line with same color on from, to vertexes
                var line = GLLine.FromTo(
                    new Vector3(rnds[0] * L, rnds[1] * L, rnds[2] * L),
                    new Vector3(rnds[3] * L, rnds[4] * L, rnds[5] * L),
                    color);

                lines.Add(line);
            }

            glModel.AddFigure(new GLLineFigure(lines));

            if (isInitial)
                glCtl.CameraView(CameraViewType.Top);
        };

        // console key handler
        w.KeyDown += (sender, e) =>
        {
            if (e.Key == Avalonia.Input.Key.Space)
            {
                // the window could contains more GLControl by the splitting behavior
                // w.FocusedControl is the actual focused
                // call the Invalidate with model rebuild ( note that normally isn't required to rebuild the model
                // but in this example we want to regenerate the entire model so RebuildModelAndRedraw will imply
                // a call to the defined GLModel.BuildModel )
                w.FocusedControl?.Invalidate(InvalidateEnum.RebuildModelAndRedraw);
            }
        };

        w.ShowSync();
    }

}
