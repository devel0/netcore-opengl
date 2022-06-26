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
        public BBox3D vtxMgrBBox { get; private set; }
        GLVertexWithNormal[] vtxMgrPoints;

        DiscreteSpace<Facet> StlSpace;

        const string FIGURE_MAP = "mesh";
        uint[] VtxMgr_Idxs_MAP;

        const string FIGURE_WCSX = "wcsx";
        uint[] VtxMgr_Idxs_WCSX;

        const string FIGURE_WCSY = "wcsy";
        uint[] VtxMgr_Idxs_WCSY;

        const string FIGURE_WCSZ = "wcsz";
        uint[] VtxMgr_Idxs_WCSZ;

        // data ===========================================================================================

        void LoadStlData()
        {
            var swTotal = new Stopwatch();
            swTotal.Start();

            var sw0 = new Stopwatch();
            sw0.Start();

            var STLmapPathfilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "map.stl");

            if (!File.Exists(STLmapPathfilename))
            {
                System.Console.WriteLine($"can't find [{STLmapPathfilename}]");
                Environment.Exit(1);
            }

            System.Console.WriteLine($"loading from file [{STLmapPathfilename}]");

            double edgeLenSum = 0d;
            int edgeCnt = 0;

            using (var stream = File.Open(STLmapPathfilename, FileMode.Open, FileAccess.Read))
            {
                var stl = STLDocument.Read(stream);
                System.Console.WriteLine($"took {sw0.Elapsed}");

                System.Console.WriteLine($"computing indices");
                sw0.Restart();
                VtxMgr.AddFaces(FIGURE_MAP, stl.Facets, () => new Vector4(currentRenderingCtl.ObjColor, currentRenderingCtl.Alpha));
                System.Console.WriteLine($"took {sw0.Elapsed.ToString()}");

                System.Console.WriteLine($"estimate edge mean");
                sw0.Restart();
                foreach (var f in stl.Facets)
                {
                    foreach (var vn in f.Vertices.WithNext().Where(r => r.next != null))
                    {
                        edgeLenSum += (((Vector3D)vn.item) - ((Vector3D)vn.next)).Length;
                        ++edgeCnt;
                    }
                }
                System.Console.WriteLine($"edge mean: {edgeLenSum / edgeCnt}");
                System.Console.WriteLine($"took {sw0.Elapsed.ToString()}");

                System.Console.WriteLine($"points:{VtxMgr.VtxWithNormals.Length} indices:{VtxMgr.Idxs.Count}");

                // TODO:
                /*
                System.Console.WriteLine($"computing discrete space");
                sw0.Restart();
                StlSpace = new DiscreteSpace<Facet>(TOL, stl.Facets, facet => facet.Mean(), 3);
                System.Console.WriteLine($"took {sw0.Elapsed.ToString()}");
                */
            }

            VtxMgr.AddLine(FIGURE_WCSX,
                new Line3D(Vector3D.Zero, Vector3D.XAxis * 10, Line3DConstructMode.PointAndVector),
                () => new Vector4(1f, 0, 0, currentRenderingCtl.Alpha));

            VtxMgr.AddLine(FIGURE_WCSY,
                new Line3D(Vector3D.Zero, Vector3D.YAxis * 10, Line3DConstructMode.PointAndVector),
                () => new Vector4(0, 1f, 0, currentRenderingCtl.Alpha));

            VtxMgr.AddLine(FIGURE_WCSZ,
                new Line3D(Vector3D.Zero, Vector3D.ZAxis * 10, Line3DConstructMode.PointAndVector),
                () => new Vector4(0, 0, 1f, currentRenderingCtl.Alpha));

            System.Console.WriteLine($"computing normals");
            sw0.Restart();
            vtxMgrPoints = VtxMgr.VtxWithNormals;
            VtxMgr_Idxs_MAP = VtxMgr.GetIdxs(FIGURE_MAP);
            VtxMgr_Idxs_WCSX = VtxMgr.GetIdxs(FIGURE_WCSX);
            VtxMgr_Idxs_WCSY = VtxMgr.GetIdxs(FIGURE_WCSY);
            VtxMgr_Idxs_WCSZ = VtxMgr.GetIdxs(FIGURE_WCSZ);
            System.Console.WriteLine($"took {sw0.Elapsed.ToString()}");

            System.Console.WriteLine($"MODEL LOAD TOTAL TIME: {swTotal.Elapsed}");

            vtxMgrBBox = VtxMgr.BBox;
            System.Console.WriteLine($"Model Bounds: {vtxMgrBBox}");
        }

        public SampleGlModel(OpenGlModelOptions options = null)
            : base(options)
        {
        }

        internal string exportDxfPending = null;

        protected override void Render(OpenGlControl _ctl, DrawingContext context, PixelSize ps)
        {
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

                var drawBBoxDiags = false;
                if (drawBBoxDiags)
                {
                    vtxMgrTmp.AddLine(BBox.Min.LineTo(BBox.Max), () => Colors.Green.ToVector4());
                    vtxMgrTmp.AddLine(BBox.Min.Set(OrdIdx.Y, BBox.Max.Y).LineTo(BBox.Max.Set(OrdIdx.Y, BBox.Min.Y)), () => Colors.Green.ToVector4());
                }

                {
                    if (ctl.pointerMovedPosition != null && ctl.Perspective)
                    {
                        var mouse_x = (float)ctl.pointerMovedPosition.Position.X;
                        var mouse_y = (float)ctl.pointerMovedPosition.Position.Y;

                        var q = ctl.Model.MousePosToWorldPos(mouse_x, mouse_y);
                        var coord = q.pt;

                        System.Console.WriteLine($"coord:{coord}");
                        var l = new Line3D(Vector3D.Zero, coord);

                        Dispatcher.UIThread.Post(() =>
                        {
                            ctl.CurrentWorldCoord = coord;
                        });

                        vtxMgrTmp.AddLine(l, () => Colors.Yellow.ToVector4(), 0.2);

                        // display view cs used to compute current world coord
                        {
                            var ccs = ctl.CameraCS;
                            var csCameraAtTarget = ccs.Move((Vector3D)ctl.CameraTarget - ccs.Origin);
                            var cs1 = csCameraAtTarget.Transform(model.Inverse());

                            vtxMgrTmp.AddLine(
                                new Line3D(BBox.Middle, cs1.BaseX.Normalized() * 30, Line3DConstructMode.PointAndVector),
                                () => Colors.Red.ToVector4());
                            vtxMgrTmp.AddLine(
                                new Line3D(BBox.Middle, cs1.BaseY.Normalized() * 30, Line3DConstructMode.PointAndVector),
                                () => Colors.Green.ToVector4());
                        }
                    }
                }

                {
                    if (ctl.ShowModelBBox)
                    {
                        vtxMgrBBox.AddToVertexManager(vtxMgrTmp);
                    }

                    DrawVtxMgr(vtxMgrTmp);
                }
            }
            #endregion            

            // bind global vertex array object
            Vao.Bind();

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

            // copy from code to fragment shader uniform input            
            color = VtxMgr.GetColor(FIGURE_MAP);
            setGLColor(color);

            GL.Uniform1(uAmbLocation, ctl.Ambient);

            // draw map         
            if (ctl.ShowModel)
            {
                EboMap.Bind();
                unsafe
                {
                    GL.DrawElements(PrimitiveType.Triangles,
                        (uint)VtxMgr_Idxs_MAP.Length, DrawElementsType.UnsignedInt, null);
                }
                if (dxf != null) vtxMgrPoints.ExportDxf(VtxMgr_Idxs_MAP, dxf, color);
            }

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
        public override BBox3D BBox => this.vtxMgrBBox;

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
                EboMap = new VertexBufferObject<uint>(GL, BufferTargetARB.ElementArrayBuffer, VtxMgr_Idxs_MAP);

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