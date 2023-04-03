namespace example;

public static partial class Toolkit
{

    public class TestConfig
    {

        public bool Wireframe;

        public bool ShadeWithEdge;

        public bool Perspective;

        public bool UseShadow;

        public bool ShowNormals;

        public bool ShowModelBBox;

        public CameraViewType ViewType;

        public override string ToString() =>
            $"w{(Wireframe ? 1 : 0)}_" +
            $"e{(ShadeWithEdge ? 1 : 0)}_" +
            $"p{(Perspective ? 1 : 0)}_" +
            $"s{(UseShadow ? 1 : 0)}_" +
            $"n{(ShowNormals ? 1 : 0)}_" +
            $"b{(ShowModelBBox ? 1 : 0)}_" +
            $"{ViewType.ShortString().ToUpper()}";
    }

    /// <summary>
    /// set BuildModel of given glModel to the scene used for unit test rendering
    /// </summary>
    public static void RenderUnitTest_SetupBuildModel(GLModel glModel, Action<GLControl>? postAction = null)
    {
        glModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            glModel.Clear();

            var box = new Box(WCS.Move(new Vector3(50, 100, 200)), new Vector3(10, 20, 5), csBaseIsMiddle: true);
            box.SideTop.SetColor(Color.Red);
            box.SideBottom.SetColor(Color.Green);
            box.SideLeft.SetColor(Color.Blue);
            box.SideRight.SetColor(Color.Magenta);
            box.SideFront.SetColor(Color.White);
            box.SideBack.SetColor(Color.Cyan);
            glModel.AddFigure(box.Sides);

            {
                var boxBase = new Box(WCS.Move(box.Center + Vector3.UnitY * 15 - box.CS.BaseZ() * 10f),
                    new Vector3(30, 70, -1), color: Color.White/*.SetAlphaF(.75f)*/, csBaseIsMiddle: false);
                glModel.AddFigure(boxBase.Sides);
            }

            {
                var box1 = new Box(WCS.Move(box.Center - Vector3.UnitX * 4 - Vector3.UnitY * 8.5f),
                    new Vector3(1, 1, 20), color: Color.LightGray, csBaseIsMiddle: true);
                glModel.AddFigure(box1.Sides);
            }

            {
                var box1 = new Box(WCS.Move(box.Center + Vector3.UnitX * 4 - Vector3.UnitY * 8.5f),
                    new Vector3(1, 1, 20), color: Color.LightGray, csBaseIsMiddle: true);
                glModel.AddFigure(box1.Sides);
            }

            {
                var box1 = new Box(WCS.Move(box.Center - Vector3.UnitX * 4 + Vector3.UnitY * 8.5f),
                    new Vector3(1, 1, 20), color: Color.LightGray, csBaseIsMiddle: true);
                glModel.AddFigure(box1.Sides);
            }

            {
                var box1 = new Box(WCS.Move(box.Center + Vector3.UnitX * 4 + Vector3.UnitY * 8.5f),
                    new Vector3(1, 1, 20), color: Color.LightGray, csBaseIsMiddle: true);
                glModel.AddFigure(box1.Sides);
            }

            var sphere = new UVSphere(box.Center, 7);
            glModel.AddFigure(sphere.Figure(50).SetColor(Color.White.SetAlphaF(.75f)));

            var txt = glModel.GLContext.MakeTextFigure(new GLText(
                cs: MakeCS(box.SideFront.Center() - Vector3.UnitY * 0.1f, Vector3.UnitX, Vector3.UnitZ),
                "π", 5, Color.Black, GLTextVHAlignment.MiddleCenter
            ));
            glModel.AddFigure(txt);

            //-----------

            var light1 = new GLPointLight(box.Center - Vector3.UnitX * 0 - Vector3.UnitY * 30 + Vector3.UnitZ * 30);
            light1.SetupAttenuation(glModel);
            glModel.PointLights.Add(light1);

            foreach (var vtx in glModel.Vertexes)
            {
                var matProp = vtx.MaterialProperties;
                vtx.MaterialProperties = matProp with { X = .5f };
            }

            glModel.NormalFactor = DEFAULT_NORMAL_FACTOR / 2f;

            postAction?.Invoke(glCtl);

        };


    }

}
