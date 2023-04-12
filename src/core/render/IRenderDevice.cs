namespace SearchAThing.OpenGL.Core;

/// <summary>
/// A render device will read pixels from given gl context through ReadPixels
/// and write to an appropriate device implementation ( avalonia, offscreen, dummy devices available ).
/// </summary>
public interface IRenderDevice : INotifyPropertyChanged
{

    /// <summary>
    /// Retrieve actual size of the device screen (pixels).
    /// </summary>    
    Size Size { get; }

    /// <summary>
    /// Use given GL object to issue a ReadPixel and transfer them to destination device.<br/>
    /// Following some details how this function is involved in the rendering process:<br/>
    /// <a href="ReactiveUI.svg">image</a>
    /// \image html ReactiveUI.svg width=100%    
    /// <br/>
    /// Here an example implementation given from the <see cref="OffscreenRenderDevice.TransferGLPixels"/>:<br/>
    /// \snippet{'trimleft'} src/render/OffscreenRenderDevice.cs OffscreenTrasferGLPixels
    /// </summary>
    /// <param name="GL"></param>
    void TransferGLPixels(GL GL);

    /// <summary>
    /// Used by <see cref="GLControl"/> to emit PropertyChanged event.<br/>
    /// This is needed in most frameworks that allow ui interactions only from the ui thread itself.<br/>
    /// Its necessary expecially if want to access ui data from a separate Task running in parallel.<br/>    
    /// </summary>
    /// <param name="action">Action to be executed in ui thread.</param>
    void Post(Action action);

    /// <summary>
    /// Stats counter of <see cref="IRenderDevice.TransferGLPixels"/> calls.
    /// </summary>    
    int TransferGLPixelsCount { get; }

}