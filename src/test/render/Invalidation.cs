namespace test;

public class InvalidationTest
{

    [Fact]
    public void Test1()
    {
        var renderDevice = new DummyRenderDevice(new Size(1024, 768));
        var glCtx = new GLContext();
        var glModel = new GLModel(glCtx);
        var glCtl = new GLControl(glModel, renderDevice);        

        var modelViewInvalidatedCount = 0;
        glModel.ViewInvalidated += (model) =>
        {
            ++modelViewInvalidatedCount;
        };

        glCtl.Render();
        Assert.True(glModel.LBBox.IsEmpty);
        Assert.Equal(0, renderDevice.TransferGLPixelsCount); // model is empty so no render happens
        Assert.Equal(0, modelViewInvalidatedCount);

        var line = GLLine.FromTo(Vector3.Zero, Vector3.UnitX);
        glModel.AddFigure(new GLLineFigure(line));
        Assert.False(glModel.LBBox.IsEmpty);
        Assert.Equal(2, modelViewInvalidatedCount); // model detected view changed ( 2 vtx added )

        glCtl.Render();
        Assert.Equal(1, renderDevice.TransferGLPixelsCount); // render happens because model not empty and is the first render
        Assert.Equal(2, modelViewInvalidatedCount);

        glCtl.Render();
        Assert.Equal(1, renderDevice.TransferGLPixelsCount); // don't render because already done
        Assert.Equal(2, modelViewInvalidatedCount);

        line.SetPrimitiveColor(Color.Yellow); // change a model object ( 2 vtx(from,to) removed 2 vtx added(from,to) )
        Assert.Equal(6, modelViewInvalidatedCount); // model detected view changed

        // don't render because even modelview invalidated the control invalidation is responsible by the user choice
        glCtl.Render();
        Assert.Equal(1, renderDevice.TransferGLPixelsCount);
        Assert.Equal(6, modelViewInvalidatedCount); // model detected view changed        

        glCtl.Invalidate(); // for changes to the model the user has to set invalidation on the control
        glCtl.Render();
        Assert.Equal(2, renderDevice.TransferGLPixelsCount);
        Assert.Equal(6, modelViewInvalidatedCount);        

        glCtl.ZoomFit(invalidate: false);
        glCtl.Render();
        Assert.Equal(2, renderDevice.TransferGLPixelsCount);
        Assert.Equal(6, modelViewInvalidatedCount);        

        glCtl.ZoomFit(invalidate: true);
        glCtl.Render();
        Assert.Equal(3, renderDevice.TransferGLPixelsCount);
        Assert.Equal(6, modelViewInvalidatedCount);        

    }

}