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
        /// together with Debug=true allow to customize the logging of debug info.
        /// </summary>        
        public DebugProc CustomDebug { get; set; }
    }

    public abstract partial class OpenGlModelBase : AvaloniaObject, IDisposable
    {

        #region vars ============================================================================================
        protected IWindow glContext;
        protected GL GL;
        OpenGlModelOptions options;

        uint fb, texture, renderBuffer;
        object renderLck = new object();
        PixelSize oldSize = new PixelSize();

        HashSet<OpenGlControl> glControls = new HashSet<OpenGlControl>();
        object glControlsLck = new object();       

        #region FocusedCtl
        private OpenGlControl _FocusedCtl = null;

        public static readonly DirectProperty<OpenGlModelBase, OpenGlControl> FocusedCtlProperty =
            AvaloniaProperty.RegisterDirect<OpenGlModelBase, OpenGlControl>("FocusedCtl", o => o.FocusedCtl, (o, v) => o.FocusedCtl = v);

        public OpenGlControl FocusedCtl
        {
            get => _FocusedCtl;
            set
            {
                System.Console.WriteLine($"OpenGlModelBase: FocusedCtl");
                SetAndRaise(FocusedCtlProperty, ref _FocusedCtl, value);
            }
        }
        #endregion    

        #endregion

        protected OpenGlModelBase(OpenGlModelOptions options = null)
        {
            if (options == null)
                this.options = new OpenGlModelOptions();
            else
                this.options = options;

            Init();
        }

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
        public void Invalidate()
        {
            OpenGlControl[] ary;
            lock (glControlsLck)
            {
                ary = glControls.ToArray();
            }
            foreach (var ctl in ary)
            {
                //System.Console.WriteLine($"invalidating ctl:{ctl.Name}");
                ctl.InvalidateVisual();
            }
        }

        #region render ==========================================================================================
        internal void RenderToGlControl(OpenGlControl ctl, DrawingContext context, PixelSize ps)
        {
            lock (renderLck)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fb);
                if (!oldSize.Equals(ps))
                {
                    Resize((uint)ps.Width, (uint)ps.Height);
                }
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

        protected abstract void Render(OpenGlControl ctl, DrawingContext context, PixelSize ps);
        #endregion

        #region init ============================================================================================
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

                if (options.Debug)
                {
                    GL.Enable(EnableCap.DebugOutput);
                    var dProc = options.CustomDebug;
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
                                id != 131218)
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

        #endregion

        #region deinit ==========================================================================================
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
        #endregion

    }

}