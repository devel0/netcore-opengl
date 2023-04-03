/// <summary>
/// GUI module use Avalonia window in stand-alone console app and in mvvm applications 
/// to transfer gl pixels from the GL framebuffer to the control drawing context.<br/>
/// More in general <see cref="GLControl"/> require a <see cref="IRenderDevice"/>; there are implemented:<br/>
/// - <see cref="AvaloniaGLControl"/>
/// - <see cref="OffscreenRenderDevice"/>
/// - <see cref="DummyRenderDevice"/>
/// </summary>
namespace SearchAThing.OpenGL.GUI;