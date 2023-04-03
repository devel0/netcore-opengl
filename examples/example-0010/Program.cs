namespace example;

// example-0010
// draw 3d shapes

class Program
{

    static string texturePathfilename(string name) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "texture", name);

    static void Main(string[] args)
    {
        InitAvalonia();

        var w = GLWindow.Create();

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;
            var glCtx = glModel.GLContext;

            glModel.Clear();

            // place a point light at xyz=(6,-4,6)
            glModel.PointLights.Add(new GLPointLight(6, -4, 6));

            // create and add a sphere centered at (0,0,0) with radius=1 and meshed as uvsphere with N=20 divisions
            var sphere = new UVSphere(center: Vector3.Zero, radius: 1);
            glModel.AddFigure(sphere.Figure(divisions: 20).SetColor(Color.Cyan));

            // place a base box which receive sphere shadow centered like the sphere but 2*z lower ( out of sphere )
            // with size xyz=(5, 5, .1f) larger than sphere but with small thickness
            var basebox = new Box(
                cs: WCS.Move(sphere.Center - Vector3.UnitZ * 2),
                csSize: new Vector3(10, 10, .1f),
                setTextureST: true); // turn on set TextureST on vertex coord

            // load texture from file
            var marble = new GLTexture2D(glCtx, SKBitmap.Decode(texturePathfilename("marble2.jpg")));

            // apply texture to the top side of the base box
            basebox.SideTop.Texture2D = marble;
            glModel.AddFigure(basebox.Sides);

            // detect max vertex z of the basebox
            var zBase = basebox.Sides.SelectMany(w => w.Vertexes().Select(v => v.Position.Z)).Max();

            // create a cs with z at the top surface of basebox
            var csBase = MakeWCS(0, 0, zBase);

            // create a truncated cone ( baseRadius < topRadius )
            var truncCone = new Cone(csBase.Move(2, 0, 0), baseRadius: .5f, topRadius: .25f, height: 3);
            glModel.AddFigure(truncCone.Figure().SetColor(Color.Orange));

            // create a pyramid ( topRadius = 0 )
            var pyramid = new Cone(csBase.Move(-2, -2, .2f), baseRadius: .5f, topRadius: 0, height: .5f);
            glModel.AddFigure(pyramid.Figure(divisions: 4).SetColor(Color.RosyBrown));

            // create a cone ( baseRadius == topRadius )
            var cone = new Cone(csBase.Move(3, -3, 0), baseRadius: .2f, topRadius: .2f, height: 1);
            glModel.AddFigure(cone.Figure().SetColor(Color.LightGreen));

            {
                // create an arrow by from, to ( note that z lies on the top of basebox )
                var arrow = new Arrow(from: new Vector3(-1, -3, zBase), to: new Vector3(1, -2.5f, zBase));

                // retrieve actual arrow tip diameter and use it to move the arrow half of that size in +z
                var td = arrow.TipDiameter;
                glModel.AddFigure(arrow.Figure().SetColor(Color.Red).Move(0, 0, td / 2));
            }

            glModel.PointLights.ForEach(w => w.SetupAttenuation(glModel));

            glCtl.LoadView();
        };

        // show the gl window
        w.ShowSync();
    }
}