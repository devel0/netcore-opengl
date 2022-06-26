using Avalonia;
using Avalonia.Media;
using Silk.NET.OpenGL;
//using Silk.NET.Windowing.Common;
using SearchAThing;
using System.Runtime.InteropServices;
using QuantumConcepts.Formats.StereoLithography;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Linq;
using static System.Math;
using Avalonia.Threading;

namespace SearchAThing.SciExamples
{

    public partial class SampleGlModel : SearchAThing.OpenGlModelBase
    {

        // vars ===========================================================================================

        SampleGlControl currentRenderingCtl = null;

        VertexBufferObject<GLVertexWithNormal> Vbo;
        VertexBufferObject<uint> EboMap, EboWcsX, EboWcsY, EboWcsZ;
        VertexArrayObject<GLVertexWithNormal> Vao;

        //Vertex shaders are run on each vertex.
        string VertexShaderSource =>
            UtilToolkit.GetEmbeddedResourcesList<SampleGlControl>().First(w => w.Contains("shader.vert")).GetEmbeddedFileContent<SampleGlControl>();

        //Fragment shaders are run on each fragment/pixel of the geometry.
        string FragmentShaderSource =>
            UtilToolkit.GetEmbeddedResourcesList<SampleGlControl>().First(w => w.Contains("shader.frag")).GetEmbeddedFileContent<SampleGlControl>();

        uint Shader;
        uint vertexShader;
        uint fragmentShader;

        // location of vertexShader attributes        
        uint positionLocation;
        uint normalLocation;

        // location of vertexShader uniforms
        int uModelLocation;
        int uProjectionLocation;
        int uViewLocation;

        // location of fragmentShader uniforms
        int uAmbLocation;
        int uAlphaLocation;
        int uObjColLocation;
        int uLightPosLocation;

        const double TOL = 1e-3;

        // vertex manager
        public VertexManager VtxMgr { get; private set; } = new VertexManager(TOL);
        readonly uint GLVertexWithNormal_SIZE = (uint)Marshal.SizeOf<GLVertexWithNormal>();
        readonly int IDX_SIZE = sizeof(uint);
        public BBox3D vtxMgrBBox => VtxMgr.BBox;
        GLVertexWithNormal[] vtxMgrPoints;

        const string FIGURE_WCSX = "wcsx";
        uint[] VtxMgr_Idxs_WCSX;

        const string FIGURE_WCSY = "wcsy";
        uint[] VtxMgr_Idxs_WCSY;

        const string FIGURE_WCSZ = "wcsz";
        uint[] VtxMgr_Idxs_WCSZ;

        // data ===========================================================================================

        void LoadStlData()
        {
            /* VtxMgr.AddLine(FIGURE_WCSX,
                 new Line3D(Vector3D.Zero, Vector3D.XAxis * 10, Line3DConstructMode.PointAndVector),
                 () => new Vector4(1f, 0, 0, currentRenderingCtl.Alpha));

             VtxMgr.AddLine(FIGURE_WCSY,
                 new Line3D(Vector3D.Zero, Vector3D.YAxis * 10, Line3DConstructMode.PointAndVector),
                 () => new Vector4(0, 1f, 0, currentRenderingCtl.Alpha));

             VtxMgr.AddLine(FIGURE_WCSZ,
                 new Line3D(Vector3D.Zero, Vector3D.ZAxis * 10, Line3DConstructMode.PointAndVector),
                 () => new Vector4(0, 0, 1f, currentRenderingCtl.Alpha));

             vtxMgrPoints = VtxMgr.VtxWithNormals;
             VtxMgr_Idxs_WCSX = VtxMgr.GetIdxs(FIGURE_WCSX);
             VtxMgr_Idxs_WCSY = VtxMgr.GetIdxs(FIGURE_WCSY);
             VtxMgr_Idxs_WCSZ = VtxMgr.GetIdxs(FIGURE_WCSZ);

             vtxMgrBBox = VtxMgr.BBox;*/
        }

        public SampleGlModel(OpenGlModelOptions options = null)
            : base(options)
        {
        }

        internal string exportDxfPending = null;

        protected override void Render(OpenGlControl _ctl, DrawingContext context, PixelSize ps)
        {
            if (vtxMgrBBox.IsEmpty) ((SampleGlControl)_ctl).Reset();

            netDxf.DxfDocument dxf = null;

            if (exportDxfPending != null)
            {
                dxf = new netDxf.DxfDocument();
            }

            GL.UseProgram(Shader);

            var ctl = _ctl as SampleGlControl;

            currentRenderingCtl = ctl;

            // transformation matrixes               
            var model = ctl.ModelMatrix;
            var view = ctl.ViewMatrix;
            var projection = ctl.ProjectionMatrix;

            // load vertex shader variables
            unsafe
            {
                GL.UniformMatrix4(uProjectionLocation, 1, false, (float*)&projection);
                GL.UniformMatrix4(uViewLocation, 1, false, (float*)&view);
                GL.UniformMatrix4(uModelLocation, 1, false, (float*)&model);
            }

            GL.PolygonMode(GLEnum.FrontAndBack, ctl.Wireframe ? PolygonMode.Line : PolygonMode.Fill);
            GL.Uniform3(uLightPosLocation, ctl.LightPos);

            // ambient (60%)
            GL.Uniform1(uAmbLocation, 0.6f);

            void setGLColor(Vector4? color)
            {
                GL.Uniform1(uAlphaLocation, color.HasValue ? color.Value.W : ctl.Alpha);
                GL.Uniform3(uObjColLocation, color.HasValue ? color.Value.ToVector3() : new Vector3(1f, 1f, 1f));
            }

            Vector4? color = null;

            /// <summary>
            /// draw given vertex manager figures using tmp vao
            /// </summary>
            void DrawVtxMgr(VertexManager vtxmgr)
            {
                var pts = vtxmgr.VtxWithNormals;

                using (var VboTmp = new VertexBufferObject<GLVertexWithNormal>(GL, BufferTargetARB.ArrayBuffer, pts))
                {
                    using (var VaoTmp = new VertexArrayObject<GLVertexWithNormal>(GL))
                    {
                        VaoTmp.AttribPointer(positionLocation, 0);
                        VaoTmp.AttribPointer(normalLocation, 3 * sizeof(float));

                        foreach (var figure in vtxmgr.FigureNames)
                        {
                            var idxs = vtxmgr.GetIdxs(figure);
                            color = vtxmgr.GetColor(figure);

                            if (dxf != null) pts.ExportDxf(idxs, dxf, color);

                            using (var EboTmp = new VertexBufferObject<uint>(GL, BufferTargetARB.ElementArrayBuffer, idxs))
                            {
                                setGLColor(color);
                                unsafe
                                {
                                    GL.DrawElements(PrimitiveType.Triangles,
                                        (uint)idxs.Length, DrawElementsType.UnsignedInt, null);
                                }
                            }
                        }
                    }
                }
            }

            #region draw tmp            
            {
                var vtxMgrTmp = new VertexManager(TOL);

                // draw ccs axes
                {
                    // x
                    vtxMgrTmp.AddLine(Vector3D.Zero.LineTo(10 * Vector3D.XAxis), () => Colors.Red.ToVector4(), 0.2);
                    // y
                    vtxMgrTmp.AddLine(Vector3D.Zero.LineTo(10 * Vector3D.YAxis), () => Colors.Green.ToVector4(), 0.2);
                    // z
                    vtxMgrTmp.AddLine(Vector3D.Zero.LineTo(10 * Vector3D.ZAxis), () => Colors.Blue.ToVector4(), 0.2);
                }

                // draw bbox
                {
                    if (ctl.ShowModelBBox)
                    {
                        vtxMgrTmp.BBox.AddToVertexManager(vtxMgrTmp, null, true, null, 0.2);
                    }
                }

                this.VtxMgr = vtxMgrTmp;

                {
                    if (ctl.pointerMovedPosition != null)// && ctl.Perspective)
                    {
                        var mouse_x = (float)ctl.pointerMovedPosition.Position.X;
                        var mouse_y = (float)ctl.pointerMovedPosition.Position.Y;

                        var coord = ctl.Model.MousePosToWorldPos(mouse_x, mouse_y);

                        System.Console.WriteLine($"coord:{coord}");
                        var l = new Line3D(Vector3D.Zero, coord);
                        if (!coord.EqualsTol(TOL, Vector3D.Zero))
                        {

                            Dispatcher.UIThread.Post(() =>
                            {
                                ctl.CurrentWorldCoord = coord;
                            });

                            var bboxFaces = BBox.Faces(1e-6).ToList();
                            //vtxMgrTmp.AddFaces("bboxFace0", bboxFaces[5].Points.Tessellate());

                            foreach (var k in new[]
                            {
                                new
                                {
                                    line=Line3D.XAxisLine,
                                    color=Colors.Red
                                },

                                new
                                {
                                    line=Line3D.YAxisLine,
                                    color=Colors.Green
                                },

                                new
                                {
                                    line=Line3D.ZAxisLine,
                                    color=Colors.Blue
                                }
                            })
                            {
                                var xps = bboxFaces
                                    .Select(bboxFace => new
                                    {
                                        bboxFace = bboxFace,
                                        ip = bboxFace.Intersect(TOL, k.line.Move(coord))
                                    })
                                    .Where(r => r.ip != null && r.bboxFace.Contains(TOL, r.ip))
                                    .ToList();

                                if (xps.Count == 2)
                                {
                                    var L = xps.First().ip.LineTo(xps.Skip(1).First().ip);
                                    vtxMgrTmp.AddLine(L, () => k.color.ToVector4(), 0.2);


                                }
                            }

                            vtxMgrTmp.AddLine(l, () => Colors.Yellow.ToVector4(), 0.2);
                        }

                        // draw widgets
                        {
                            double x = 0;
                            double y = 0;
                            double z = 0;

                            var step = 5;
                            var cubsize = new Vector3D(1, 1, 1);

                            while (x <= 10)
                            {
                                y = 0;
                                while (y <= 10)
                                {
                                    z = 0;
                                    while (z <= 10)
                                    {
                                        var matches =
                                            l.To.X >= x - cubsize.X / 2 &&
                                            l.To.X <= x + cubsize.X / 2 &&

                                            l.To.Y >= y - cubsize.Y / 2 &&
                                            l.To.Y <= y + cubsize.Y / 2 &&

                                            l.To.Z >= z - cubsize.Z / 2 &&
                                            l.To.Z <= z + cubsize.Z / 2;

                                        // if (matches)
                                        //     System.Console.WriteLine("maches");

                                        vtxMgrTmp.AddCuboid(CoordinateSystem3D.WCS.Move(new Vector3D(x, y, z)),
                                            cubsize.X, cubsize.Y, cubsize.Z,
                                            () => matches ? Colors.Yellow.ToVector4() : Colors.Cyan.ToVector4(0.1f));

                                        z += step;
                                    }

                                    y += step;
                                }
                                x += step;
                            }
                        }
                    }
                }

                DrawVtxMgr(vtxMgrTmp);
            }
            #endregion            

            // bind global vertex array object
            Vao.Bind();
            /*
                        #region draw wcs                                                            
                        {
                            {
                                color = VtxMgr.GetColor(FIGURE_WCSX);
                                setGLColor(color);
                                EboWcsX.Bind();
                                unsafe
                                {
                                    GL.DrawElements(PrimitiveType.Triangles,
                                        (uint)VtxMgr_Idxs_WCSX.Length, DrawElementsType.UnsignedInt, null);
                                }
                                if (dxf != null) vtxMgrPoints.ExportDxf(VtxMgr_Idxs_WCSX, dxf, color);
                            }
                            {
                                color = VtxMgr.GetColor(FIGURE_WCSY);
                                setGLColor(color);
                                EboWcsY.Bind();
                                unsafe
                                {
                                    GL.DrawElements(PrimitiveType.Triangles,
                                        (uint)VtxMgr_Idxs_WCSY.Length, DrawElementsType.UnsignedInt, null);
                                }
                                if (dxf != null) vtxMgrPoints.ExportDxf(VtxMgr_Idxs_WCSY, dxf, color);
                            }
                            {
                                color = VtxMgr.GetColor(FIGURE_WCSZ);
                                setGLColor(color);
                                EboWcsZ.Bind();
                                unsafe
                                {
                                    GL.DrawElements(PrimitiveType.Triangles,
                                        (uint)VtxMgr_Idxs_WCSZ.Length, DrawElementsType.UnsignedInt, null);
                                }
                                if (dxf != null) vtxMgrPoints.ExportDxf(VtxMgr_Idxs_WCSZ, dxf, color);
                            }
                        }
                        #endregion                     
            */
            // copy from code to fragment shader uniform input            
            // color = VtxMgr.GetColor(FIGURE_MAP);
            // setGLColor(color);

            GL.Uniform1(uAmbLocation, ctl.Ambient);

            ctl.UpdateInfo();

            currentRenderingCtl = null;

            if (exportDxfPending != null)
            {
                dxf.Viewport.ShowGrid = false;
                dxf.Save(exportDxfPending, true);
                exportDxfPending = null;

                var psi = new ProcessStartInfo(Environment.CurrentDirectory);
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
        }

        /// <summary>
        /// override bbox in order to allow framework F
        /// </summary>
        public override BBox3D BBox =>
            this.VtxMgr == null ? new BBox3D() : this.VtxMgr.BBox;

        bool dataLoaded = false;

        protected override void OnInitialized()
        {
            if (!dataLoaded)
            {
                LoadStlData();
                dataLoaded = true;
            }
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Init Shader
            {
                //Creating a vertex shader.
                vertexShader = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(vertexShader, VertexShaderSource);
                GL.CompileShader(vertexShader);

                //Checking the shader for compilation errors.
                var infoLog = GL.GetShaderInfoLog(vertexShader);
                if (!string.IsNullOrWhiteSpace(infoLog))
                {
                    Console.WriteLine($"Error compiling vertex shader {infoLog}");
                }

                //Creating a fragment shader.
                fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(fragmentShader, FragmentShaderSource);
                GL.CompileShader(fragmentShader);

                //Checking the shader for compilation errors.
                infoLog = GL.GetShaderInfoLog(fragmentShader);
                if (!string.IsNullOrWhiteSpace(infoLog))
                {
                    Console.WriteLine($"Error compiling fragment shader {infoLog}");
                }

                //Combining the shaders under one shader program.
                Shader = GL.CreateProgram();
                GL.AttachShader(Shader, vertexShader);
                GL.AttachShader(Shader, fragmentShader);
                GL.LinkProgram(Shader);

                //Delete the no longer useful individual shaders
                GL.DetachShader(Shader, vertexShader);
                GL.DetachShader(Shader, fragmentShader);
                GL.DeleteShader(vertexShader);
                GL.DeleteShader(fragmentShader);

                positionLocation = SafeGetAttribLocation(Shader, "vPos");
                normalLocation = SafeGetAttribLocation(Shader, "vNormal");

                //Checking the linking for errors.
                var shader = GL.GetProgramInfoLog(Shader);
                if (!string.IsNullOrWhiteSpace(shader))
                {
                    Console.WriteLine($"Error linking shader {infoLog}");
                }

                GL.UseProgram(Shader);

                //Retrieve uniform locations
                uModelLocation = SafeGetUniformLocation(Shader, "uModel");
                uProjectionLocation = SafeGetUniformLocation(Shader, "uProjection");
                uViewLocation = SafeGetUniformLocation(Shader, "uView");
                //uApplyTransform = SafeGetUniformLocation("uApplyTransform");

                uAmbLocation = SafeGetUniformLocation(Shader, "uAmb");
                uAlphaLocation = SafeGetUniformLocation(Shader, "uAlpha");
                uObjColLocation = SafeGetUniformLocation(Shader, "uObjCol");
                uLightPosLocation = SafeGetUniformLocation(Shader, "uLightPos");
            }

            // Init Buffers
            {
                //Initializing a vertex buffer that holds the vertex data.
                Vbo = new VertexBufferObject<GLVertexWithNormal>(GL, BufferTargetARB.ArrayBuffer, vtxMgrPoints);

                //Initializing a element buffer that holds the index data.                
                EboWcsX = new VertexBufferObject<uint>(GL, BufferTargetARB.ElementArrayBuffer, VtxMgr_Idxs_WCSX);

                EboWcsY = new VertexBufferObject<uint>(GL, BufferTargetARB.ElementArrayBuffer, VtxMgr_Idxs_WCSY);

                EboWcsZ = new VertexBufferObject<uint>(GL, BufferTargetARB.ElementArrayBuffer, VtxMgr_Idxs_WCSZ);

                //Creating a vertex array.                
                Vao = new VertexArrayObject<GLVertexWithNormal>(GL);

                /*
                                Vertex0        | Vertex1        |
                components      x y z nx ny nz | x y z nx ny nz | ...
                positionLoc     o o o .  .  .  | o o o .  .  .  |     components=3
                        offset  ^              | ^              |     offset=0
                normalLoc       . . . o  o  o  | . . . o  o  o  |     components=3
                        offset        ^        |       ^        |     offset=3

                sizeof(float)=4
                stride=4*6      o . . .  .  .  | o . . .  .  .  |

                */

                Vao.AttribPointer(positionLocation, 0);
                Vao.AttribPointer(normalLocation, 3 * sizeof(float));
            }
        }
    }

}