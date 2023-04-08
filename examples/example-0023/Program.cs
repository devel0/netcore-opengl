using BriefFiniteElementNet;
using BriefFiniteElementNet.Elements;
using BriefFiniteElementNet.Materials;

namespace example;

// example-0023
// TODO: doc

class Program
{

    /// <summary>
    /// Simple Cantilever FEM model<br/>
    /// https://github.com/BriefFiniteElementNet/BriefFiniteElement.Net/blob/b7173d2e3ab8e3896f365d5f1c5ee0943a3d307d/Samples/Examples.CSharp/SimpleCantilever.cs
    /// </summary>
    static Model BuildFEM_Model()
    {
        var model = new Model();

        var l = 5;

        var n1 = new Node(0, 0, 0);
        var n2 = new Node(0, 0, l);

        var axialLoad = 1000;
        var horizontalLoad = 100000;

        var f = new Force(horizontalLoad, 0, axialLoad, 0, 0, 0);

        /**/
        var h = 0.1;
        var w = 0.05;

        var a = h * w;
        var iy = h * h * h * w / 12;
        var iz = w * w * w * h / 12;
        var j = iy + iz;
        var e = 210e9;
        var nu = 0.3;

        var g = e / (2 * 1 + nu);
        /**/

        var sec = new BriefFiniteElementNet.Sections.UniformParametric1DSection(a, iy, iz, j);
        var mat = UniformIsotropicMaterial.CreateFromYoungShear(e, g);

        var belm = new BarElement(n1, n2)
        {
            Material = mat,
            Section = sec,
            Behavior = BarElementBehaviours.FullFrame
        };
        model.Elements.Add(belm);
        model.Nodes.Add(n1, n2);

        n1.Constraints = Constraints.Fixed;

        n2.Loads.Add(new NodalLoad(f));

        model.Solve_MPC();

        var d = model.Nodes[1].GetNodalDisplacement();

        var expectedDx = (horizontalLoad * l * l * l) / (3 * e * iy);
        var expectedRy = (horizontalLoad * l * l) / (2 * e * iy);
        var expectedDz = axialLoad * l / (e * a);

        // var epsilon = 0.0;

        // if (Math.Abs(d.DX - expectedDx) > epsilon) throw new NotImplementedException();
        // if (Math.Abs(d.RY - expectedRy) > epsilon) throw new NotImplementedException();
        // if (Math.Abs(d.DZ - expectedDz) > epsilon) throw new NotImplementedException();

        return model;
    }

    static void Main(string[] args)
    {
        InitAvalonia();

        var w = GLWindow.Create();

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            glModel.Clear();

            var fmodel = BuildFEM_Model();

            glModel.AddFigure(MakeWCSFigure());

            foreach (var node in fmodel.Nodes)
            {
                var nodeFig = new GLPointFigure(node.ToVector3());
                glModel.AddFigure(nodeFig);

                var nodeDelta = node.GetNodalDisplacement().ToVector3();

                var dnodeFig = new GLPointFigure(node.ToVector3() + nodeDelta).SetColor(Color.Yellow);
                glModel.AddFigure(dnodeFig);

                foreach (var nodeLoad in node.Loads)
                {
                    var fx = nodeLoad.Force.Fx.ToFloat();
                    var fy = nodeLoad.Force.Fy.ToFloat();
                    var fz = nodeLoad.Force.Fz.ToFloat();

                    var normalizedForce = Vector3.Normalize(new Vector3(fx, fy, fz)) * .3f;

                    var arrow = new Arrow(node.ToVector3(), node.ToVector3() + normalizedForce, diameterFactor: .3f);
                    glModel.AddFigure(arrow.Figure(lineMode: true).SetColor(Color.Red));
                }
            }

            foreach (var fel in fmodel.Elements)
            {
                if (fel is BarElement bar)
                {
                    var glLine = GLLine.FromTo(
                        fel.Nodes[0].ToVector3(),
                        fel.Nodes[1].ToVector3(),
                        Color.Blue);
                    glModel.AddFigure(new GLLineFigure(glLine));

                    var N = 10;
                    {
                        var section_off = 0d;
                        var section_off_step = 1d / N;
                        Vector3? prevDisp = null;
                        Vector3? barPrevPos = null;
                        var lines = new List<GLLine>();
                        var dpts = new List<Vector3>();
                        var barLine = Line.FromTo(bar.StartNode.ToVector3(), bar.EndNode.ToVector3());

                        // Iso Parametric Coordination system for BarElement with two nodes
                        // ref: https://bfenet.readthedocs.io/en/latest/elements/finiteElements/Bar/coords.html#iso-parametric-coordination-system-for-barelement-with-two-nodes

                        for (int i = 0; i < N + 1; ++i)
                        {
                            var xi = (2 * section_off) - 1;
                            var disp = bar.GetGlobalDisplacementAt(xi).ToVector3();

                            Debug.WriteLine(Invariant($"section off:[{section_off}] iso ξ:[{xi}] g.disp:[{disp}]"));
                            var barPos = barLine.From + barLine.V * (float)section_off;

                            var dpt = barPos + disp;
                            dpts.Add(dpt);

                            if (prevDisp is not null && barPrevPos is not null)
                                lines.Add(GLLine.FromTo(barPrevPos.Value + prevDisp.Value, dpt));

                            glModel.AddFigure(glModel.MakeTextFigure(
                                new GLText(XZCS.Move(dpt + .2f * Vector3.UnitX),
                                Invariant($"{disp}"),
                                height: .2f,
                                alignment: GLTextVHAlignment.MiddleLeft)));

                            prevDisp = disp;
                            barPrevPos = barPos;

                            section_off += section_off_step;
                        }
                        glModel.AddFigure(new GLLineFigure(lines).SetColor(Color.Yellow));
                        glModel.AddFigure(new GLPointFigure(dpts).SetPointSize(5).SetColor(Color.Orange));
                    }
                }
            }

            glCtl.Perspective = false;
            glCtl.CameraView(CameraViewType.Front);
        };

        w.ShowSync();
    }

}

public static partial class Ext
{

    /// <summary>
    /// retrieve location x,y,z as Vector3
    /// </summary>    
    public static Vector3 ToVector3(this BriefFiniteElementNet.Node node) =>
        new Vector3(node.Location.X.ToFloat(), node.Location.Y.ToFloat(), node.Location.Z.ToFloat());

    /// <summary>
    /// retrieve dx,dy,dz as Vector3
    /// </summary>
    public static Vector3 ToVector3(this Displacement disp) =>
        new Vector3(disp.DX.ToFloat(), disp.DY.ToFloat(), disp.DZ.ToFloat());
}
