using Avalonia.Controls;
using Avalonia.Media;
using Silk.NET.OpenGL;
using Silk.NET.Windowing.Common;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Visuals.Media.Imaging;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using static System.Math;
using Avalonia.Input;

namespace SearchAThing
{

    public class OpenGlModelOptions
    {
        /// <summary>
        /// if enabled a debug process will called to log GL info;
        /// if CustomDebug not specified a default console logger will used.
        /// </summary>        
        public bool Debug { get; set; }

        /// <summary>
        /// if true a render info with ctl name will written to debug console when a render occurs.
        /// name of the ctl can be set on the CreateControl method
        /// </summary>
        public bool DebugRenderCtlName { get; set; }

        /// <summary>
        /// together with Debug=true allow to customize the logging of debug info.
        /// </summary>        
        public DebugProc CustomDebug { get; set; }

        /// <summary>
        /// double click (ms)
        /// </summary>
        public double DoubleClickMs { get; set; } = 250;

        public double GeomTol { get; set; } = 1e-10;
    }

    public abstract partial class OpenGlModelBase : AvaloniaObject, IDisposable
    {

        // vars ============================================================================================
        protected IWindow glContext;
        public GL GL;
        public OpenGlModelOptions Options { get; private set; }

        uint fb, texture, renderBuffer;
        object renderLck = new object();

        HashSet<OpenGlControl> glControls = new HashSet<OpenGlControl>();
        object glControlsLck = new object();

        #region FocusedControl
        private OpenGlControl _FocusedControl = null;

        public static readonly DirectProperty<OpenGlModelBase, OpenGlControl> FocusedControlProperty =
            AvaloniaProperty.RegisterDirect<OpenGlModelBase, OpenGlControl>("FocusedControl", o => o.FocusedControl, (o, v) => o.FocusedControl = v);

        public OpenGlControl FocusedControl
        {
            get => _FocusedControl;
            set => SetAndRaise(FocusedControlProperty, ref _FocusedControl, value);
        }
        #endregion

        #region FocusedControlSize
        private PixelSize _FocusedControlSize = new PixelSize();

        public static readonly DirectProperty<OpenGlModelBase, PixelSize> FocusedControlSizeProperty =
            AvaloniaProperty.RegisterDirect<OpenGlModelBase, PixelSize>("FocusedControlSize", o => o.FocusedControlSize, (o, v) => o.FocusedControlSize = v);

        public PixelSize FocusedControlSize
        {
            get => _FocusedControlSize;
            set => SetAndRaise(FocusedControlSizeProperty, ref _FocusedControlSize, value);
        }
        #endregion

        protected OpenGlModelBase(OpenGlModelOptions options = null)
        {
            if (options == null)
                this.Options = new OpenGlModelOptions();
            else
                this.Options = options;

            Init();
        }

        public double GeomTol => Options.GeomTol;

        internal void Register(OpenGlControl ctl)
        {
            if (!glControls.Contains(ctl))
            {
                lock (glControlsLck)
                {
                    if (!glControls.Contains(ctl))
                        glControls.Add(ctl);
                }
            }
        }

        internal void Deregister(OpenGlControl ctl)
        {
            if (glControls.Contains(ctl))
            {
                lock (glControlsLck)
                {
                    if (glControls.Contains(ctl))
                        glControls.Remove(ctl);
                }
            }
        }

        /// <summary>
        /// force invalidate visual of associated controls.
        /// this should not required from user side because its called automatically at render end.
        /// </summary>
        public void InvalidateAllControls()
        {
            OpenGlControl[] ary;
            lock (glControlsLck)
            {
                ary = glControls.ToArray();
            }
            foreach (var ctl in ary)
            {
                ctl.InvalidateVisual();
            }
        }

        public OpenGlControl[] GetAllControls()
        {
            OpenGlControl[] ary;
            lock (glControlsLck)
            {
                ary = glControls.ToArray();
            }
            return ary;
        }

        /// <summary>
        /// force invalidate visual of current focused control;
        /// </summary>
        public void InvalidateCurrentControl()
        {
            FocusedControl.InvalidateVisual();
        }

        /// <summary>
        /// retrieve attrib location generating exception if not found.
        /// example:
        /// layout(location = 0) in vec3 vPos;
        /// </summary>
        /// <param name="program">shader id</param>
        /// <param name="name">attrib name</param>
        /// <returns>attrib location</returns>
        protected uint SafeGetAttribLocation(uint program, string name)
        {
            var res = GL.GetAttribLocation(program, name);

            if (res < 0) throw new Exception($"invalid attrib location name [{name}]. it may simplified because not used by vertex shader or flowing through pipeline not used in the fragment shader.");

            return (uint)res;
        }

        /// <summary>
        /// retrieve uniform location generating exception if not found.
        /// example:
        /// uniform vec3 uObjCol;
        /// </summary>
        /// <param name="program">shader id</param>
        /// <param name="name">uniform name</param>
        /// <returns>uniform location</returns>
        protected int SafeGetUniformLocation(uint program, string name)
        {
            var res = GL.GetUniformLocation(program, name);

            if (res < 0) throw new Exception($"invalid uniform location name [{name}]. it may simplified because not used by vertex shader or flowing through pipeline not used in the fragment shader.");

            return res;
        }

        // events ==========================================================================================

        // render ==========================================================================================
        internal void RenderToGlControl(OpenGlControl ctl, DrawingContext context, PixelSize ps)
        {
            lock (renderLck)
            {
                if (Options.DebugRenderCtlName)
                {
                    //System.Diagnostics.Debug.WriteLine($"render [name:{ctl.Name}] {DateTime.Now}");
                }
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fb);
                if (!FocusedControlSize.Equals(ps))
                {
                    Resize((uint)ps.Width, (uint)ps.Height);
                    ctl.RebuildProjectionMatrix();
                    FocusedControlSize = ps;
                }

                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                RenderClear(ctl, context, ps);

                GL.UseProgram(overlayShader);
                GL.PolygonMode(GLEnum.FrontAndBack, PolygonMode.Fill);
                ctl.RenderOverlay(GL, context, ps);

                Render(ctl, context, ps);

                using (var bitmap = new WriteableBitmap(
                    ps,
                    new Avalonia.Vector(96, 96),
                    Avalonia.Platform.PixelFormat.Rgba8888))
                {
                    using (var l = bitmap.Lock())
                    {
                        GL.PixelStore(
                            PixelStoreParameter.PackRowLength,
                            l.RowBytes / 4);

                        unsafe
                        {
                            GL.ReadPixels(0, 0, (uint)ps.Width, (uint)ps.Height,
                                PixelFormat.Bgra, PixelType.UnsignedByte, l.Address.ToPointer());
                        }
                    }

                    context.DrawImage(bitmap,
                        new Rect(bitmap.Size),
                        new Rect(bitmap.Size),
                        BitmapInterpolationMode.LowQuality);
                }
            }
        }

        protected virtual void RenderClear(OpenGlControl ctl, DrawingContext context, PixelSize ps)
        {
            GL.ClearColor(Colors.Black.ToSystemDrawingColor());
            GL.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        }

        protected abstract void Render(OpenGlControl ctl, DrawingContext context, PixelSize ps);

        public abstract BBox3D BBox { get; }

        // init ============================================================================================
        void Init()
        {
            #region create gl context
            {
                var opts = WindowOptions.Default;
                opts.IsVisible = false;
                glContext = Silk.NET.Windowing.Window.Create(opts);
                glContext.Initialize();
                // https://github.com/Ultz/Silk.NET/issues/66
                glContext.IsVisible = false;
                GL = GL.GetApi(glContext);

                if (Options.Debug)
                {
                    GL.Enable(EnableCap.DebugOutput);
                    var dProc = Options.CustomDebug;
                    if (dProc == null)
                    {
                        dProc = (_source, _type, id, _severity, length, message, userParam) =>
                        {
                            var str = Marshal.PtrToStringAnsi(message);
                            var source = (DebugSource)_source;
                            var type = (DebugType)_type;
                            var severity = (DebugSeverity)_severity;
                            if (id != 131185)
                            {
                                System.Console.WriteLine();
                                System.Console.WriteLine($"Severity:{severity.ToString().StripBegin("DebugSeverity")} - Type:{type.ToString().StripBegin("DebugType")} - Source:{source.ToString().StripBegin("DebugSource")} - Id:{id} ===> {str}");
                            }
                            if (severity > DebugSeverity.DebugSeverityNotification &&
                                //
                                // Severity:Medium - Type:Performance - Source:Api - Id:131218 ===> Program/shader state performance warning: Vertex shader in program 3 is being recompiled based on GL state.
                                id != 131218 &&
                                // Severity:Medium - Type:Performance - Source:Api - Id:131154 ===> Pixel-path performance warning: Pixel transfer is synchronized with 3D rendering.
                                id != 131154
                                )
                                Debugger.Break();
                        };
                    }
                    unsafe
                    {
                        GL.DebugMessageCallback(dProc, null);
                    }
                }
            }
            #endregion

            Resize(100, 100);

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != GLEnum.FramebufferComplete)
            {
                throw new Exception($"can't alloc framebuffer");
            }

            InitBuiltinOverlayShader();

            OnInitialized();
        }

        protected virtual void OnInitialized()
        {

        }

        bool fbInitialized = false;

        void Resize(uint width, uint height)
        {
            var firstInit = !fbInitialized;

            if (!fbInitialized) fbInitialized = true;

            if (firstInit)
            {
                fb = GL.GenFramebuffer();

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fb);

                texture = GL.GenTexture();
            }
            else
            {
                GL.DeleteRenderbuffer(renderBuffer);
            }

            GL.Viewport(0, 0, width, height);

            GL.BindTexture(TextureTarget.Texture2D, texture);
            unsafe
            {
                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0, // level
                    (int)GLEnum.Rgba8,
                    width, height,
                    0, // border
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    (void*)0);
            }
            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (int)GLEnum.Nearest);
            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (int)GLEnum.Nearest);

            // create new render buffer
            renderBuffer = GL.GenRenderbuffer();

            GL.BindRenderbuffer(
                RenderbufferTarget.Renderbuffer,
                (uint)renderBuffer);
            GL.RenderbufferStorage(
                RenderbufferTarget.Renderbuffer,
                InternalFormat.DepthComponent,
                width, height);
            GL.FramebufferRenderbuffer(
                GLEnum.Framebuffer,
                GLEnum.DepthAttachment,
                GLEnum.Renderbuffer,
                renderBuffer);

            if (firstInit)
            {
                GL.FramebufferTexture2D(
                   FramebufferTarget.Framebuffer,
                   FramebufferAttachment.ColorAttachment0,
                   TextureTarget.Texture2D,
                   texture,
                   0);
            }
        }

        // deinit ==========================================================================================
        public void Dispose()
        {
            OnDeinitialize();

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DeleteFramebuffer(fb);
            GL.DeleteTexture(texture);
            GL.DeleteRenderbuffer(renderBuffer);
        }

        protected virtual void OnDeinitialize()
        {

        }

    }

}