using QuantumConcepts.Formats.StereoLithography;

namespace example;

// example-0003
// render stl terrain map varying vertex colors by height ; presence of a point light makes shadows
//
// - toggle shadow with 's' key gesture
// - view front with 'f' key gesture
// - switch perspective/ortho with 'p' key gesture

class Program
{

    static void Main(string[] args)
    {
        InitAvalonia();

        var w = GLWindow.Create();

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            glModel.Clear();

            var STLmapPathfilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "map.stl");

            if (!File.Exists(STLmapPathfilename))
                return;

            Vector3 ToVector3(Vertex v) => new Vector3(v.X, v.Y, v.Z);

            var waterColor = ColorTranslator.FromHtml("#073f5e").Fn(col => Color.FromArgb(230, col.R, col.G, col.B));
            var mountainsColor = ColorTranslator.FromHtml("#9a802c").Fn(col => Color.FromArgb(255, col.R, col.G, col.B));
            var highMountainsColor = Color.White;

            Color GetVertexColor(Vector3 stlVertex)
            {
                var z = stlVertex.Z;
                if (z < 1.2)
                    return waterColor;

                if (z < 11)
                    return mountainsColor;

                return highMountainsColor;
            }

            using (var stream = File.Open(STLmapPathfilename, FileMode.Open, FileAccess.Read))
            {
                var stl = STLDocument.Read(stream);

                var tris = stl.Facets.Select(f =>
                {
                    Vector3 v;
                    var p1 = new GLVertex(v = ToVector3(f.Vertices[0]), GetVertexColor(v));
                    var p2 = new GLVertex(v = ToVector3(f.Vertices[1]), GetVertexColor(v));
                    var p3 = new GLVertex(v = ToVector3(f.Vertices[2]), GetVertexColor(v));

                    var tri = new GLTriangle(p1, p2, p3);

                    return tri;
                }).ToArray();

                glModel.AddFigure(new GLTriangleFigure(tris));
            }

            glModel.PointLights.Clear();
            glModel.PointLights.Add(new GLPointLight(new Vector3(5, 5, 25), Color.White));
            glModel.SetupLightAttenuation(adjustLinear: 0, adjustQuadratic: 0);

            glCtl.CameraView(CameraViewType.FrontTop);
        };

        w.KeyDown += (sender, e) =>
        {
            if (e.Key == Avalonia.Input.Key.Space) w.FocusedControl?.Invalidate(InvalidateEnum.RebuildModelAndRedraw);
        };

        w.ShowSync();
    }

}
