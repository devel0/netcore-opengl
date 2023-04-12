using System.Runtime.InteropServices;

using Silk.NET.Windowing;

namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Gl context object with purpose to:<br/>
/// - create Silk.NET gl window <see cref="IWindow"/>.<br/>
/// - expose <see cref="Silk.NET.OpenGL.GL"/> interface.<br/>
/// - handle font char cache through instance of <see cref="GLFontCharManager"/>.<br/>
/// - load <see cref="GLPipeline"/> objects used by the framework.<br/>
/// - handle environment variable "OPENGL_LIBRARY_PATH" for <a href="https://fdossena.com/?p=mesa/index.frag">mesa win library</a> } loading. Useful for windows guest on virtualbox linux host.<br/>
/// - handle gl debug messages by using DebugMessageCallback.
/// </summary>
public class GLContext : IDisposable
{

    #region IDisposable & destructor

    bool disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Manual release of managed resources

            }
        }
        // Release of unmanaged resources
        disposed = true;
    }

    ~GLContext() => Dispose();

    #endregion  

    /// <summary>
    /// Windows load dll library invoke.
    /// </summary>
    /// <param name="path">DLL library path</param>
    /// <returns></returns>
    [DllImport("kernel32")]
    public static extern IntPtr LoadLibrary(string path);

    #region variables

    internal IWindow window;

    /// <summary>
    /// Silk.NET GL handle.
    /// </summary>    
    public GL GL { get; private set; }

    /// <summary>
    /// (depth) Shader used for shadow generation.
    /// </summary>    
    /// <seealso cref="GLControl.UseShadow"/>.
    public GLPipeline DepthShader { get; private set; }

    /// <summary>
    /// Shader used to render main scene.
    /// </summary>        
    public GLPipeline MainShader { get; private set; }

    /// <summary>
    /// (geometry) Shader used to render triangles with edges.
    /// </summary>
    /// <seealso cref="GLControl.ShadeWithEdge"/>.
    public GLPipeline ShadeWithEdgeShader { get; private set; }

    /// <summary>
    /// (geometry) Shader used to render triangle vertex normals.
    /// <seealso cref="GLControl.ShowNormals"/>.
    /// </summary>    
    public GLPipeline NormalShader { get; private set; }

    GLFontCharManager? _FontCharMgr = null;
    /// <summary>
    /// Gl font char manager used to cache font char bitmap and textures.
    /// </summary>    
    public GLFontCharManager FontCharMgr
    {
        get
        {
            if (_FontCharMgr is null) _FontCharMgr = new GLFontCharManager(this);
            return _FontCharMgr;
        }
    }

    object lckAutocleanup = new Object();
    HashSet<uint> DisposedTextureIds = new HashSet<uint>();

    public void AddDisposedTextureId(uint textureId) => DisposedTextureIds.Add(textureId);

    /// <summary>
    /// release disposed resources ( texture, ... ) ; this will be called
    /// during GL2DTexture() constructor or
    /// before GLModel.EnsureModel() or
    /// before GLControl.Invalidate()
    /// </summary>

    /// <summary>
    /// Automatic release of disposed resources such as <see cref="GLTexture2D"/>.<br/>
    /// This will be called in follow locations:<br/>
    /// - <see cref="GLTexture2D.GLTexture2D"/>.
    /// - <see cref="GLModel.EnsureModel"/>.
    /// - <see cref="GLControl.Invalidate"/>.
    /// </summary>
    public void AutoCleanup()
    {
        if (DisposedTextureIds.Count > 0)
        {
            lock (lckAutocleanup)
            {
                if (DisposedTextureIds.Count > 0)
                {
                    // Debug.WriteLine($"Dispose {(string.Join(",", DisposedTextureIds.Select(w => w.ToString())))} textures");
                    GL.DeleteTextures((uint)DisposedTextureIds.Count, DisposedTextureIds.ToArray());
                    DisposedTextureIds.Clear();
                }
            }
        }
    }

    #endregion

    /// <summary>
    /// Construct a new gl context.
    /// </summary>
    public GLContext()
    {
        // Init GL
        {
            var opts = WindowOptions.Default;
            opts.IsVisible = false;

            // opts.API = new GraphicsAPI(ContextAPI.OpenGL,
            //     ContextProfile.Core, ContextFlags.ForwardCompatible,
            //     new APIVersion(3, 3));

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                // if need to use mesa driver ( ie. windows guest on virtualbox )
                // download driver from https://fdossena.com/?p=mesa/index.frag)
                //
                // MESA environment required:
                //-----------------
                // MESA_GL_VERSION_OVERRIDE: 4.6
                // MESA_GLSL_VERSION_OVERRIDE: 460
                // OPENGL_LIBRARY_PATH: path to folder containing opengl32.dll                
                //                

                var mesa_lib_path = Environment.GetEnvironmentVariable(MESA_WIN_LIBRARY_ENV, EnvironmentVariableTarget.Process);
                if (mesa_lib_path is not null)
                {
                    var found_mesa = false;

                    foreach (var libfilename in new[] { "opengl32.dll" })
                    {
                        var lib_pathfilename = Path.Combine(mesa_lib_path, libfilename);
                        if (File.Exists(lib_pathfilename))
                        {
                            if (!found_mesa)
                            {
                                found_mesa = true;
                                Environment.SetEnvironmentVariable("MESA_GL_VERSION_OVERRIDE", "4.6", EnvironmentVariableTarget.Process);
                                Environment.SetEnvironmentVariable("MESA_GLSL_VERSION_OVERRIDE", "460", EnvironmentVariableTarget.Process);
                            }
                            LoadLibrary(lib_pathfilename);
                        }
                    }
                }
            }

            // var glfw = Silk.NET.GLFW.Glfw.GetApi();
            // glfw.WindowHint(Silk.NET.GLFW.WindowHintBool.Visible, false);

            opts.IsVisible = false;
            opts.SharedContext = null;

            window = Silk.NET.Windowing.Window.Create(opts);
            window.Initialize();

            // https://github.com/Ultz/Silk.NET/issues/66
            window.IsVisible = false;

            GL = Silk.NET.OpenGL.GL.GetApi(window);

            Debug.WriteLine(GetVersion());

            EnableDebug();
        }

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
#if DEBUG
        GL.Enable(EnableCap.DebugOutputSynchronous);
#endif
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        {
            var VertexShaderSource = GetResourceContentAsString(SHADER_RESOURCE_DEPTH_VERT);
            var GeometryShaderSource = GetResourceContentAsString(SHADER_RESOURCE_DEPTH_GEOM);
            var FragmentShaderSource = GetResourceContentAsString(SHADER_RESOURCE_DEPTH_FRAG);

            DepthShader = new GLPipeline(this, VertexShaderSource, GeometryShaderSource, FragmentShaderSource,
                friendlyName: "SHADOW");
        }

        {
            var VertexShaderSource = GetResourceContentAsString(SHADER_RESOURCE_MAIN_VERT);
            var GeometryShaderSource = (string?)null;
            var FragmentShaderSource = GetResourceContentAsString(SHADER_RESOURCE_MAIN_FRAG);

            MainShader = new GLPipeline(this, VertexShaderSource, GeometryShaderSource, FragmentShaderSource,
                friendlyName: "MAIN");
        }

        {
            var VertexShaderSource = GetResourceContentAsString(SHADER_RESOURCE_MAIN_VERT);
            var GeometryShaderSource = GetResourceContentAsString(SHADER_RESOURCE_MAIN_GEOM_SHADE_WITH_EDGE);
            var FragmentShaderSource = GetResourceContentAsString(SHADER_RESOURCE_MAIN_FRAG);

            ShadeWithEdgeShader = new GLPipeline(this, VertexShaderSource, GeometryShaderSource, FragmentShaderSource,
                friendlyName: "MAIN_SHADE_WITH_EDGE");
        }

        {
            var VertexShaderSource = GetResourceContentAsString(SHADER_RESOURCE_MAIN_VERT);
            var GeometryShaderSource = GetResourceContentAsString(SHADER_RESOURCE_MAIN_GEOM_NORMAL);
            var FragmentShaderSource = GetResourceContentAsString(SHADER_RESOURCE_MAIN_FRAG);

            NormalShader = new GLPipeline(this, VertexShaderSource, GeometryShaderSource, FragmentShaderSource,
                friendlyName: "MAIN_NORMAL");
        }
    }

    static Assembly? _netcore_opengl_core_assembly = null;

    internal static Assembly netcore_opengl_core_assembly
    {
        get
        {
            if (_netcore_opengl_core_assembly is null)
            {
                var q = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == NETCORE_OPENGL_CORE_ASSEMBLY_NAME);

                // foreach (var x in AppDomain.CurrentDomain.GetAssemblies())
                // {
                //     var name = x.GetName().Name;
                //     if (name?.Contains("netcore-opengl") == true)
                //         System.Console.WriteLine($"assembly: {name}");
                // }

                if (q is null)
                {
                    throw new Exception($"unable to find [netcore-opengl-core] assembly");
                }

                _netcore_opengl_core_assembly = q;
            }

            return _netcore_opengl_core_assembly;
        }
    }

    string GetResourceContentAsString(string regex)
    {
        var names = GetEmbeddedResourceNames(netcore_opengl_core_assembly);

        // foreach (var name in names)
        // {
        //     System.Console.WriteLine($"resource name [{name}]");
        // }

        return names
            .First(w => w.RegexMatch(regex) > 0)
            .Fn(resourcename => GetEmbeddedFileContentString(netcore_opengl_core_assembly, resourcename));
    }

    void EnableDebug()
    {
        GL.Enable(EnableCap.DebugOutput);

        DebugProc dProc = (_source, _type, id, _severity, length, message, userParam) =>
        {
            var str = Marshal.PtrToStringAnsi(message);
            var source = (DebugSource)_source;
            var type = (DebugType)_type;
            var severity = (DebugSeverity)_severity;

            if (severity > DebugSeverity.DebugSeverityNotification)
            {
                if (
                    //                 
                    // id != 131185 &&

                    // Program/shader state performance warning: Vertex shader in program 7 is being recompiled based on GL state.
                    id != 131218
                    )
                {
                    System.Console.WriteLine();
                    System.Console.WriteLine($"Severity:{severity.ToString().StripBegin("DebugSeverity")} - Type:{type.ToString().StripBegin("DebugType")} - Source:{source.ToString().StripBegin("DebugSource")} - Id:{id} ===> {str}");
                }
            }

            if (severity == DebugSeverity.DebugSeverityHigh)
                Debugger.Break();
        };

        unsafe
        {
            GL.DebugMessageCallback(dProc, null);
        }
    }

    /// <summary>
    /// Retrieve GL library version.
    /// </summary>
    public string GetVersion()
    {
        var res = "";

        unsafe
        {
            var str = GL.GetString(GLEnum.Version);
            var q = Marshal.PtrToStringAnsi((IntPtr)str);
            if (q is not null) res = q;
        }

        return res;
    }

}
