namespace SearchAThing.OpenGL.Shapes;

/// <summary>
/// Delegate for <see cref="ContourLegend.CustomLegendLabel"/>.
/// </summary>
/// <param name="index">Index of the i-th contour legend entry.</param>
/// <param name="val">Value of the i-th contour legend ventry.</param>
/// <returns>String representation of the i-th contour legend.</returns>
public delegate string CustomLegendLabelDelegate(int index, double val);

/// <summary>
/// Contour Legend
/// </summary>
public class ContourLegend
{

    /// <summary>
    /// GL context ( used to make text )
    /// </summary>
    public GLContext GLContext { get; private set; }

    /// <summary>
    /// Title of contour legend.
    /// </summary>    
    public string Title { get; private set; }

    /// <summary>
    /// Number of legend blocks to represent.<br/>
    /// Lower bound (MinValue) extends through given division toward upper bound (MaxValue)
    /// displaying intermediate values aside colored block legend rectangle.
    /// </summary>    
    public int Divisions { get; private set; }

    /// <summary>
    /// Minimum value of contour.
    /// </summary>    
    public double MinValue { get; private set; }

    /// <summary>
    /// Maximum value of contour.
    /// </summary>    
    public double MaxValue { get; private set; }

    /// <summary>
    /// Text height.
    /// </summary>
    public float TextHeight { get; private set; }

    /// <summary>
    /// Size of contour legend squared block.
    /// </summary>
    public float ContourBlockSize { get; private set; }

    /// <summary>
    /// If not null a function that will invoked to generate legend labels.<br/>
    /// args:(int index, double val) (Default:null).
    /// </summary>    
    public CustomLegendLabelDelegate? CustomLegendLabel { get; private set; }

    /// <summary>
    /// If true a black (alpha:100f/255) coloured background is placed under legend bbox.
    /// </summary>    
    public bool UseFigureBackground { get; private set; }

    /// <summary>
    /// Create a set of figures that represents a contour legend.<br/>    
    /// </summary>    
    /// <param name="glContext">GL context ( used to make text )</param>
    /// <param name="title">Title of contour legend.</param>
    /// <param name="divisions">Number of legend blocks to represent.<br/>
    /// Lower bound (MinValue) extends through given division toward upper bound (MaxValue)
    /// displaying intermediate values aside colored block legend rectangle.</param>
    /// <param name="minVal">Minimum value of contour.</param>
    /// <param name="maxVal">Maximum value of contour.</param>
    /// <param name="textHeight">Text height.</param>
    /// <param name="contourBlockSize">Size of contour legend squared block.</param>
    /// <param name="customLegendLabel">If not null a function that will invoked to generate legend labels.<br/>
    /// args:(int offset, double val) (Default:null).</param>
    /// <param name="useFigureBackground">If true a black (alpha:100f/255) coloured background is placed under legend bbox.</param>
    public ContourLegend(GLContext glContext,
        string title, int divisions, double minVal, double maxVal,
        float textHeight = 20, float contourBlockSize = 10,
        CustomLegendLabelDelegate? customLegendLabel = null,
        bool useFigureBackground = true)
    {
        GLContext = glContext;
        Title = title;
        Divisions = divisions;
        MinValue = minVal;
        MaxValue = maxVal;
        TextHeight = textHeight;
        ContourBlockSize = contourBlockSize;
        CustomLegendLabel = customLegendLabel;
        UseFigureBackground = useFigureBackground;
    }

    /// <summary>
    /// Retrieve list of figures that represent the contour legend [screen].
    /// </summary>    
    public List<GLFigureBase> Figures()
    {
        var res = new List<GLFigureBase>();

        var scs = MakeScreenCS();
        var titleFig = GLContext
            .MakeTextFigure(new GLText(scs, Title, TextHeight, alignment: GLTextVHAlignment.TopLeft))
            .SetScreenMode(true);
        {
            var bbox = titleFig.OBBox();
            res.Add(new GLLineFigure(GLLine.FromTo(
                new Vector3(bbox.Min.X, bbox.Max.Y, 0),
                new Vector3(bbox.Max.X, bbox.Max.Y, 0)))
            { ScreenCoordMode = true });
        }

        var y = TextHeight * 1.5f;

        var glpts = new List<GLPoint>();
        var gltxt = new List<GLTriangleFigure>();
        var valDelta = MaxValue - MinValue;
        for (int i = Divisions; i >= 0; --i)
        {
            double val;
            if (i == Divisions) val = MaxValue;
            else if (i == 0) val = MinValue;
            else val = MinValue + (valDelta / Divisions) * i;

            var f = ((float)i) / Divisions;
            var color = GetContourColor(f);

            glpts.Add(new GLPoint(TextHeight / 2, y + TextHeight / 2, 0, color));
            gltxt.AddRange(GLContext.MakeTextFigure(new GLText(
                cs: scs.Move(1.5f * TextHeight, y + TextHeight / 2, 0),
                text: CustomLegendLabel is null ? Invariant($"{val}") : CustomLegendLabel.Invoke(i, val),
                height: TextHeight,
                alignment: GLTextVHAlignment.MiddleLeft)));

            y += ContourBlockSize;
        }

        res.Add(new GLPointFigure(glpts) { PointSize = ContourBlockSize, ScreenCoordMode = true });
        res.AddRange(gltxt.SetScreenMode(true));
        res.AddRange(titleFig);

        if (UseFigureBackground)
        {
            var bbox = res.OBBox();

            res.Add(new Plate(
                new Vector3(bbox.Min.X, bbox.Max.Y, 0),
                new Vector3(bbox.Max.X, bbox.Max.Y, 0),
                new Vector3(bbox.Max.X, bbox.Min.Y, 0),
                new Vector3(bbox.Min.X, bbox.Min.Y, 0))
                .GetTriangles(Color.FromArgb(100, 0, 0, 0))
                .ToFigure()
                .SetScreenMode(true));
        }

        return res;
    }

}