using BriefFiniteElementNet;
using BriefFiniteElementNet.Elements;
using BriefFiniteElementNet.Solver;

namespace example;

// example-0024
// Show 3-D fem element displacement with countour and legend visible only in one of the split views
// using control and figure custom tag data.
//
// - try to close some view and open new ones using 'c' (close), 'h' (horizontal split), 'v' (vertical split)
// - note the legend bar is visible only in one of the views

class Program
{

    /// <summary>
    /// Mkl example<br/>
    /// https://github.com/BriefFiniteElementNet/BriefFiniteElement.Net/blob/b7173d2e3ab8e3896f365d5f1c5ee0943a3d307d/Samples/Examples.CSharp/MklExample.cs
    /// </summary>
    static Model BuildFEM_Model()
    {
        var model = BriefFiniteElementNet.StructureGenerator.Generate3DTetrahedralElementGrid(
            m: 2, n: 2, l: 2,
            dx: 1, dy: 1, dz: 1);

        var material = BriefFiniteElementNet.Materials.UniformIsotropicMaterial.CreateFromYoungPoisson(210e9, 0.2);

        foreach (var elm in model.Elements)
            if (elm is BriefFiniteElementNet.Elements.TetrahedronElement tet)
                tet.Material = material;


        model.Nodes.Last().Loads.Add(new NodalLoad(new Force(10, 0, 0, 0, 0, 0), LoadCase.DefaultLoadCase));

        var solverFactory = new LuSolverFactory();// new MklSolverFactory();

        var cfg = new BriefFiniteElementNet.SolverConfiguration(solverFactory, LoadCase.DefaultLoadCase);

        model.Solve_MPC(cfg);

        return model;
    }

    /// <summary>
    /// Class to tag controls with addictional custom informations.
    /// </summary>
    class ControlTagNfo
    {
        /// <summary>
        /// States if countour legend has to be shown in tag associated gl control.
        /// </summary>
        public bool ContourLegendVisible;
    }

    /// <summary>
    /// Class to tag figures with addictional custom informations.
    /// </summary>
    class FigureTagNfo
    {
        /// <summary>
        /// States if figure is related to contour data.
        /// </summary>
        public bool IsLegend;
    }

    static void Main(string[] args)
    {
        InitAvalonia();

        /// Length comparision tolerance used to gather vertexes grouped by their position.    
        const float LEN_TOL = 1e-10f;

        var w = GLWindow.Create(
            onFocusedControlChanged: (split, AvaloniaGLControl, isInitial) =>
            {
                if (isInitial)
                    split.LoadViewLayout();
            },

            onGLControlCreated: (avaloniaGLControl) =>
            {
                var glCtl = avaloniaGLControl.GLControl;

                // tag created control with custom ControlTagNfo data.                
                glCtl.Tag = new ControlTagNfo
                {
                    ContourLegendVisible = glCtl.IsInitial
                }; ;

                // define a custom gl control related figure visibility function
                // with pourpose of showing legend figures only in at most 1 gl control between those gl splitted.
                glCtl.ControlFigureVisible = (glCtl, fig) =>
                {
                    if (glCtl.Tag is ControlTagNfo ctlTagNfo &&
                        fig.Tag is FigureTagNfo figTagNfo)
                    {
                        // figure not visible if its a legend figure in a gl control not tagged for legend showing
                        if (figTagNfo.IsLegend && !ctlTagNfo.ContourLegendVisible)
                            return false;
                    }
                    return true;
                };
            }
        );

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            glModel.Clear();

            var fmodel = BuildFEM_Model();

            glModel.AddFigure(MakeWCSFigure(size: .2f));

            // sweep fem node loads to draw load arrow
            foreach (var node in fmodel.Nodes)
            {
                foreach (var nodeLoad in node.Loads)
                {
                    var fx = nodeLoad.Force.Fx.ToFloat();
                    var fy = nodeLoad.Force.Fy.ToFloat();
                    var fz = nodeLoad.Force.Fz.ToFloat();

                    var normalizedForce = Vector3.Normalize(new Vector3(fx, fy, fz)) * .3f;

                    var arrow = new Arrow(node.ToVector3(), node.ToVector3() + normalizedForce, diameterFactor: .05f);
                    glModel.AddFigure(arrow.Figure(lineMode: false)
                        .SetColor(Color.Cyan)
                        .Act(fig => fig.ExcludeFromShadeWithEdge = true));
                }
            }

            var tets = new List<TetrahedronElement>();
            var tetFigs = new List<GLTriangleFigure>();

            // sweep fem tetrahedron elements to draw figures
            foreach (var tet in fmodel.Elements.OfType<TetrahedronElement>())
            {
                var tris = new List<GLTriangle>();

                var nodes = tet.Nodes.Select(w => w.ToVector3()).ToList();

                tris.Add(new GLTriangle(nodes[0], nodes[1], nodes[3]));
                tris.Add(new GLTriangle(nodes[1], nodes[2], nodes[3]));
                tris.Add(new GLTriangle(nodes[2], nodes[0], nodes[3]));
                tris.Add(new GLTriangle(nodes[0], nodes[2], nodes[1]));

                var tetFig = new GLTriangleFigure(tris);

                tets.Add(tet);

                tetFigs.Add(tetFig);

                glModel.AddFigure(tetFig);
            }

            // retrieve the gl model position distinct vertexes related to those of tetFigs
            var vtxDict = tetFigs.BuildVertexPosDict(tol: LEN_TOL);

            var nodeNfos = fmodel
                .Nodes
                .Select(node => new
                {
                    node,
                    dsp = node.GetNodalDisplacement()
                })
                .ToList();

            var dx_min_max = nodeNfos.Select(nfo => (float)nfo.dsp.DX).MinMax()!.Value;
            var dx_size = dx_min_max.max - dx_min_max.min;

            foreach (var nodeNfo in nodeNfos)
            {
                var node_dx = nodeNfo.dsp.DX;
                var f = (float)((node_dx - dx_min_max.min) / dx_size);
                if (vtxDict.TryGetValue(nodeNfo.node.Location.ToVector3().PositionSignature(LEN_TOL), out var vtxs))
                {
                    foreach (var vtx in vtxs) vtx.SetColor(GetContourColor(f));
                }
            }

            var legendFigures = new ContourLegend(
                glModel.GLContext,
                title: "Brick Disp Dx",
                divisions: 10,
                minVal: dx_min_max.min,
                maxVal: dx_min_max.max,
                textHeight: 20,
                contourBlockSize: 30)
                .Figures()
                .Move(new Vector3(20, 15, 0)) // left, top margin
                .ToHashSet();
            // tag legend figures in order to recognize them in custom gl control visibity function
            {
                var tag = new FigureTagNfo { IsLegend = true };
                foreach (var fig in legendFigures) fig.Tag = tag;
            }
            glModel.AddFigure(legendFigures);

            glModel.PointLights.Add(new GLPointLight(.5f, -2f, 2f));

            glCtl.Perspective = true;

            // listen to gl split changes to update gl controls tag nfo about legend visibility
            if (w.GLControlSplit is not null)
                w.GLControlSplit.Children.CollectionChanged += (sender, e) =>
                {
                    var glviews = w.GLControlSplit.Children.ToList();
                    
                    var atLeastOneControlWithLegend = glviews.Any(glview => 
                        glview.AvaloniaGLControl.GLControl.Tag is ControlTagNfo tagNfo && tagNfo.ContourLegendVisible);
                    
                    // if all gl control have no legend, set the first one to show that
                    if (!atLeastOneControlWithLegend)
                    {
                        var glview = w.GLControlSplit.Children.First();

                        if (glview.AvaloniaGLControl.GLControl.Tag is ControlTagNfo tagNfo)                        
                            tagNfo.ContourLegendVisible = true;                        
                    }

                    w.GLControlSplit.Invalidate();
                };
        };

        w.ShowSync();
    }

}

public static partial class Ext
{

    /// <summary>
    /// retrieve point x,y,z as Vector3
    /// </summary>    
    public static Vector3 ToVector3(this in BriefFiniteElementNet.Point point) =>
        new Vector3(point.X.ToFloat(), point.Y.ToFloat(), point.Z.ToFloat());

    /// <summary>
    /// retrieve location x,y,z as Vector3
    /// </summary>    
    public static Vector3 ToVector3(this BriefFiniteElementNet.Node node) =>
        new Vector3(node.Location.X.ToFloat(), node.Location.Y.ToFloat(), node.Location.Z.ToFloat());

}
