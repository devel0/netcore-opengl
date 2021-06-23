using Avalonia;
using Avalonia.Media;
using Silk.NET.OpenGL;
using System;
using System.Linq;
using System.Numerics;

namespace SearchAThing.SciExamples
{
    public partial class SampleGlModel : SearchAThing.OpenGlModelBase
    {

        public Vector3 ObjColorMin => new Vector3(0f, 0, 0);
        public Vector3 ObjColorMax => new Vector3(1, 1, 1);

        VertexBufferObject<float> Vbo;
        VertexBufferObject<uint> Ebo;
        VertexArrayObject<float> Vao;

        private uint Shader;

        //Vertex shaders are run on each vertex.
        string VertexShaderSource =>
            UtilToolkit.GetEmbeddedResourcesList<SampleGlModel>().First(w => w.Contains("vertexShader.glsl")).GetEmbeddedFileContent<SampleGlModel>();

        //Fragment shaders are run on each fragment/pixel of the geometry.
        string FragmentShaderSource =>
            UtilToolkit.GetEmbeddedResourcesList<SampleGlModel>().First(w => w.Contains("fragmentShader.glsl")).GetEmbeddedFileContent<SampleGlModel>();

        //Vertex data, uploaded to the VBO.
        private readonly float[] Vertices =
        {
            // xyz
            -1f, -1f, 0f,
            1f, -1f, 0f,
            0f, 1f, 0f
        };

        //Index data, uploaded to the EBO.
        private readonly uint[] Indices =
        {
            0, 1, 2,
        };

        public SampleGlModel(OpenGlModelOptions opts = null) : base(opts)
        {
        }

        protected override void OnInitialized()
        {            
            //Creating a vertex array.
            Vao = new VertexArrayObject<float>(GL);

            //Initializing a vertex buffer that holds the vertex data.
            Vbo = new VertexBufferObject<float>(GL, BufferTargetARB.ArrayBuffer, new Span<float>(Vertices));

            //Initializing a element buffer that holds the index data.            
            Ebo = new VertexBufferObject<uint>(GL, BufferTargetARB.ElementArrayBuffer, new Span<uint>(Indices));

            //Creating a vertex shader.
            uint vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, VertexShaderSource);
            GL.CompileShader(vertexShader);

            //Checking the shader for compilation errors.
            string infoLog = GL.GetShaderInfoLog(vertexShader);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                Console.WriteLine($"Error compiling vertex shader {infoLog}");
            }

            //Creating a fragment shader.
            uint fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
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

            //Checking the linking for errors.
            string shader = GL.GetProgramInfoLog(Shader);
            if (!string.IsNullOrWhiteSpace(shader))
            {
                Console.WriteLine($"Error linking shader {infoLog}");
            }

            //Delete the no longer useful individual shaders;
            GL.DetachShader(Shader, vertexShader);
            GL.DetachShader(Shader, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            //GL.UseProgram(Shader);            

            var vPosLocation = SafeGetAttribLocation(Shader, "vPos");

            //Tell opengl how to give the data to the shaders.

            /*
                                Vertex0 | Vertex1 |
                components      x y z   | x y z   | ...
                positionLoc     o o o   | o o o   |     components=3
                offset          ^       | ^       |     offset=0
                sizeof(float)=4
                stride=4*3      o . .   | o . .   |

                */

            Vao.AttribPointer(vPosLocation, 0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3);
        }

        // protected override void RenderClear(OpenGlControl _ctl, DrawingContext context, PixelSize ps)
        // {
        //     //Clear the color channel.
        //     GL.Clear((uint)ClearBufferMask.ColorBufferBit);
        // }

        protected override void Render(OpenGlControl _ctl, DrawingContext context, PixelSize ps)
        {
            GL.UseProgram(Shader);

            var objColLoc = GL.GetUniformLocation(Shader, "ObjCol");
            var glCtl = _ctl as SampleGlControl;
            GL.Uniform3(objColLoc, glCtl.ObjColorRed, glCtl.ObjColorGreen, glCtl.ObjColorBlue);

            //Bind the geometry and shader.
            Vao.Bind();

            Ebo.Bind();

            //Draw the geometry.
            unsafe
            {
                GL.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
            }
        }

        /// <summary>
        /// override bbox in order to allow framework F
        /// </summary>
        public override BBox3D BBox => new BBox3D(new[] { new Vector3D(-1, -1, 0), new Vector3D(1, 1, 0) });

    }

}