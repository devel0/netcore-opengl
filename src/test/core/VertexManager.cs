namespace test;

public class VertexManagerTest
{
    [Fact]
    public void Test1()
    {
        var vtxMgr = new GLVertexManager();
        var vboArrayHash = vtxMgr.GetVboArray().GetHashCode();

        {
            var a = new GLVertex(new Vector3(14, 3, 0), Color.Red);
            var b = new GLVertex(new Vector3(51, 12, 0), Color.Green);
            var c = new GLVertex(new Vector3(29, 32, 0), Color.Blue);

            var tri = new GLTriangle(a, b, c);

            var fig = new GLTriangleFigure();

            fig.Add(tri);

            Assert.Empty(vtxMgr.GetVboArray());
            Assert.Equal(vboArrayHash, vtxMgr.GetVboArray().GetHashCode());
            vboArrayHash = vtxMgr.GetVboArray().GetHashCode();

            vtxMgr.AddFigure(fig);

            // Debug.WriteLine(vtxMgr.Dump());

            Assert.Equal(3, vtxMgr.GetVboArray().Length);
            AssertEqualsTol(1e-3f, a.Position, vtxMgr.GetVboArray()[0].Position);
            AssertEqualsTol(1e-3f, b.Position, vtxMgr.GetVboArray()[1].Position);
            AssertEqualsTol(1e-3f, c.Position, vtxMgr.GetVboArray()[2].Position);

            Assert.NotEqual(vboArrayHash, vtxMgr.GetVboArray().GetHashCode());
            vboArrayHash = vtxMgr.GetVboArray().GetHashCode();

            Assert.Equal(vboArrayHash, vtxMgr.GetVboArray().GetHashCode());
        }

        {
            var a = new GLVertex(new Vector3(14, 3, 0), Color.Red);

            var pt = new GLPoint(a);

            var fig = new GLPointFigure();

            fig.Add(pt);

            vtxMgr.AddFigure(fig);

            a.Position = new Vector3(10, 11, 12);
            Assert.Equal(4, vtxMgr.GetVboArray().Length);

            a.Position = new Vector3(14, 3, 0);
            Assert.Equal(4, vtxMgr.GetVboArray().Length);

            /*
            Vertexes Count : 4
            Vertexes Removed : 0
            Index   Members   Position      Normal      Signature                                                        
            -------------------------------------------------------------------------------------------------------------
            0       1         <14, 3, 0>    <0, 0, 1>   <14, 3, 0>_<1, 0, 0, 1>_<0.8, 0.6, 0.6>_<0, 0, 1>_<0, 0>         
            1       1         <51, 12, 0>   <0, 0, 1>   <51, 12, 0>_<0, 0.5019608, 0, 1>_<0.8, 0.6, 0.6>_<0, 0, 1>_<0, 0>
            2       1         <29, 32, 0>   <0, 0, 1>   <29, 32, 0>_<0, 0, 1, 1>_<0.8, 0.6, 0.6>_<0, 0, 1>_<0, 0>        
            3       1         <14, 3, 0>    <0, 0, 0>   <14, 3, 0>_<1, 0, 0, 1>_<0.8, 0.6, 0.6>_<0, 0, 0>_<0, 0>     
            */
            // Debug.WriteLine(vtxMgr.Dump());
        }

    }

    GLTriangleFigure GenTriFig(bool sphereType, float radius, int surfaceDivisions)
    {
        GLTriangleFigure res;

        if (sphereType)
            res = new UVSphere(Vector3.Zero, radius).Figure(surfaceDivisions);

        else
        {
            var tris = new List<GLTriangle>();

            var step = radius / surfaceDivisions;
            for (int sx = 0; sx < surfaceDivisions; ++sx)
            {
                for (int sy = 0; sy < surfaceDivisions; ++sy)
                {
                    var p1 = new Vector3(sx * step, sy * step, 0);
                    var p2 = p1 + new Vector3(step, 0, 0);
                    var p3 = p2 + new Vector3(0, step, 0);
                    tris.Add(new GLTriangle(p1, p2, p3));

                    var p4 = p1 + new Vector3(0, step, 0);
                    tris.Add(new GLTriangle(p1, p3, p4));
                }
            }

            res = new GLTriangleFigure(tris);
        }

        return res;
    }

    [Fact]
    public void Test2()
    {
        var vtxMgr = new GLVertexManager();

        GLTriangleFigure? triFig = null;

        var d = 10; // surface divisions

        for (int i = 0; i < 2; ++i)
        {
            if (triFig is not null) vtxMgr.RemoveFigure(triFig);
            triFig = GenTriFig(sphereType: false, radius: 10, surfaceDivisions: d);
            vtxMgr.AddFigure(triFig);
            // Debug.WriteLine($"d:{d} v:{vtxMgr.VertexCount} t:{vtxMgr.TriangleCount}");

            // v = (d+1)^2
            // t = 2*d^2

            Assert.Equal((d + 1) * (d + 1), vtxMgr.VertexCount);
            Assert.Equal(2 * d * d, vtxMgr.TriangleCount);
        }
    }

    /// <summary>
    /// Test automatic vertex distinct between figures.
    /// </summary>
    [Fact]
    public void Test3()
    {
        //! [PreserveFigureVertexTest]
        var vtxMgr = new GLVertexManager();

        var fig1 = new GLLineFigure(GLLine.FromTo(Vector3.Zero, Vector3.UnitX));
        var fig2 = new GLLineFigure(GLLine.FromTo(Vector3.Zero, Vector3.UnitY));

        vtxMgr.AddFigure(fig1);
        vtxMgr.AddFigure(fig2);
        Assert.Equal(vtxMgr.VertexCount, 3);

        fig1.SetColor(Color.Red);
        Assert.Equal(vtxMgr.VertexCount, 4);
        //! [PreserveFigureVertexTest]
    }

    [Fact]
    public void Test4()
    {
        var vtxMgr = new GLVertexManager();

        var a = new GLVertex(Vector3.Zero);
        var b = new GLVertex(Vector3.UnitX);
        var c = new GLVertex(Vector3.UnitY);

        var fig1 = new GLLineFigure(GLLine.FromTo(a, b));
        var fig2 = new GLLineFigure(GLLine.FromTo(a, c));

        vtxMgr.AddFigure(fig1);
        Assert.Throws<Exception>(() => vtxMgr.AddFigure(fig2));
    }

    [Fact]
    public void Test5()
    {
        var v1 = new GLVertex(new Vector3(1, 2, 1e-7f));
        var v2 = new GLVertex(new Vector3(1, 2, -1e-7f));

        Assert.Equal("1_2_0", v1.PositionSignature(1e-5f));
        Assert.Equal("1_2_0", v2.PositionSignature(1e-5f));
    }

}