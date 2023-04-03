namespace example;

// example-0011
// texture, light and text transparency
//
// use application keys 1,2,3,4,5,6 to textured toggle box faces
// - 'x' to toggle out texture
// - 'w' to toggle at wireframe mode
// - 'n' to show normals

class Program
{

    static void Main(string[] args)
    {
        // this must called for console application to enable Avalonia framework
        // and must called before any other Avalonia control usage		
        InitAvalonia();

        var scale = 1.0f;

        Box? box = null;

        // create standalone Avalonia window for Silk.NET opengl rendering
        var w = GLWindow.Create();

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            glModel.Clear();

            // create a box to texture
            {
                box = new Box(
                    cs: MakeCS(scale * new Vector3(0, 0, -0.5f), Vector3.UnitZ),
                    csSize: scale * new Vector3(1, 1, 1),
                    setTextureST: true);

                box.Sides.ForEach(w => w.Visible = false);

                box.SideTop.Visible = true;
                box.SideBack.Visible = true;
                box.SideFront.Visible = true;

                // textures: marble1.jpg  marble2.jpg  tile1.jpg  verdello1.jpg

                string texturePathfilename(string name) =>
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "texture", name);

                var marble1 = glModel.LoadTexture(texturePathfilename("marble1.jpg"));
                var marble2 = glModel.LoadTexture(texturePathfilename("marble2.jpg"));
                var tile1 = glModel.LoadTexture(texturePathfilename("tile1.jpg"));
                var verdello1 = glModel.LoadTexture(texturePathfilename("verdello1.jpg"));

                box.SideTop.Texture2D = marble2;

                box.SideBottom.Texture2D = verdello1;

                box.SideLeft.Texture2D = tile1;

                box.SideRight.Texture2D =
                box.SideFront.Texture2D =
                box.SideBack.Texture2D = marble1;

                //box.Sides.ForEach(w => w.Tag = "side");

                glModel.AddFigure(box.Sides);

                // append text to box faces ( inner and outer )

                var bsides = box.Sides.ToArray();

                for (int si = 0; si < bsides.Length; ++si)
                {
                    var side = bsides[si];

                    // retrieve triangle CS so that the CS basez is concordant to the computed normal
                    var sidePlaneNZ = side.Triangles.First().PlaneNZ;

                    // set insCs to insert text at center of side with .01f distance perp to side
                    var insCs = sidePlaneNZ.SetOrigin(side.Center() + sidePlaneNZ.BaseZ() * .01f);

                    var sideText = new GLText(cs: insCs, text: $"{si + 1}")
                    {
                        Alignment = GLTextVHAlignment.MiddleCenter,
                        Color = side.Visible ? Color.White : Color.White.SetAlphaF(.4f)
                    };

                    var txtFigs = glModel.MakeTextFigure(sideText);
                    glModel.AddFigure(txtFigs);

                    // create a copy of txtFigs as txtFigsInner and tag them for further identification
                    // because we want only on text (outer) if side not visible
                    const string INNER_TAG = "inner";
                    var txtFigsInner = new List<GLTriangleFigure>();
                    foreach (var txtFig in txtFigs)
                    {
                        var copy = (GLTriangleFigure)txtFig.Copy();
                        copy.Move(-2 * sidePlaneNZ.BaseZ() * .01f);
                        copy.Tag = INNER_TAG;
                        txtFigsInner.Add(copy);
                    }
                    glModel.AddFigure(txtFigsInner);

                    // routine to setup text visibility and order
                    // called now and when visiblity of side changes
                    void setupTexts()
                    {
                        var color = side.Visible switch
                        {
                            true => Color.White,
                            false => Color.White.SetAlphaF(.4f)
                        };

                        txtFigs.Union(txtFigsInner).OfType<GLTextCharFigure>().ForEach(charFig =>
                        {
                            charFig.GLText.Color = color;
                            charFig.Order = side.Visible ? 0 : -1;
                            if (!side.Visible && charFig.Tag is string s && s == INNER_TAG)
                                charFig.Visible = false;
                            else
                                charFig.Visible = true;
                        });
                    }

                    setupTexts();

                    side.PropertyChanged += (a, b) =>
                    {
                        if (b.PropertyName == nameof(GLFigureBase.Visible))
                        {
                            setupTexts();
                        }
                    };
                }
            }

            // create a box to project the shadow onto
            {
                var sbox = new Box(
                    cs: MakeCS(scale * new Vector3(2, 0, -1.5f), Vector3.UnitZ),
                    csSize: scale * new Vector3(0.1f, 3, 3),
                    color: Color.Orange);
                glModel.AddFigure(sbox.Sides);
            }

            glModel.AddFigure(MakeWCSFigure(scale));

            glModel.PointLights.Add(new GLPointLight(-1, 0, .2f, Color.White));
            glModel.SetupLightAttenuation(adjustLinear: .1f);

            glCtl.LoadView();
        };

        w.KeyDown += (sender, e) =>
        {
            var glCtl = w.FocusedControl;
            if (glCtl is null) return;

            var glCtx = glCtl.GLContext;
            var glModel = glCtl.GLModel;
            var vtxMgr = glModel.GLVertexManager;

            if (box is not null &&
                ((e.Key >= Key.D1 && e.Key <= Key.D6) || (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad6)))
            {
                var faceIdx = (e.Key >= Key.D1 && e.Key <= Key.D6) ? e.Key - Key.D1 : e.Key - Key.NumPad1;

                var bsides = box.Sides.ToArray();

                bsides[faceIdx].Visible = !bsides[faceIdx].Visible;
            }

            glCtl.Invalidate();
        };

        w.ShowSync();
    }

}
