using Silk.NET.OpenGL;
using Avalonia;
using System;
using System.Linq;

namespace SearchAThing
{

    public abstract partial class OpenGlModelBase : AvaloniaObject, IDisposable
    {

        // location of builtin overlay vertexShader attributes        
        internal uint overlayPositionLocation;

        // location of builtin overlay fragmentShader uniforms
        internal int overlayUObjColLocation;

        // builtin overlay vertex shader        
        string OverlayVertexShaderSource =>
            UtilToolkit.GetEmbeddedResourceNames().First(w => w.Contains("overlay.vert")).GetEmbeddedFileContent<OpenGlModelBase>();        

        // builtin overlay fragment shader        
        string OverlayFragmentShaderSource =>
            UtilToolkit.GetEmbeddedResourceNames().First(w => w.Contains("overlay.frag")).GetEmbeddedFileContent<OpenGlModelBase>();            

        uint overlayShader;
        uint overlayVertexShader;
        uint overlayFragmentShader;

        void InitBuiltinOverlayShader()
        {
            // Init Shader
            {
                //Creating a vertex shader.
                overlayVertexShader = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(overlayVertexShader, OverlayVertexShaderSource);
                GL.CompileShader(overlayVertexShader);

                //Checking the shader for compilation errors.
                var infoLog = GL.GetShaderInfoLog(overlayVertexShader);
                if (!string.IsNullOrWhiteSpace(infoLog))
                {
                    Console.WriteLine($"Error compiling vertex shader {infoLog}");
                }

                //Creating a fragment shader.
                overlayFragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(overlayFragmentShader, OverlayFragmentShaderSource);
                GL.CompileShader(overlayFragmentShader);

                //Checking the shader for compilation errors.
                infoLog = GL.GetShaderInfoLog(overlayFragmentShader);
                if (!string.IsNullOrWhiteSpace(infoLog))
                {
                    Console.WriteLine($"Error compiling fragment shader {infoLog}");
                }

                //Combining the shaders under one shader program.
                overlayShader = GL.CreateProgram();
                GL.AttachShader(overlayShader, overlayVertexShader);
                GL.AttachShader(overlayShader, overlayFragmentShader);
                GL.LinkProgram(overlayShader);

                //Delete the no longer useful individual shaders
                GL.DetachShader(overlayShader, overlayVertexShader);
                GL.DetachShader(overlayShader, overlayFragmentShader);
                GL.DeleteShader(overlayVertexShader);
                GL.DeleteShader(overlayFragmentShader);

                overlayPositionLocation = SafeGetAttribLocation(overlayShader, "vPos");
                //                ovelayNormalLocation = SafeGetAttribLocation(overlayShader, "vNormal");

                //Checking the linking for errors.
                var shader = GL.GetProgramInfoLog(overlayShader);
                if (!string.IsNullOrWhiteSpace(shader))
                {
                    Console.WriteLine($"Error linking shader {infoLog}");
                }

                GL.UseProgram(overlayShader);

                //Retrieve uniform locations                
                overlayUObjColLocation = SafeGetUniformLocation(overlayShader, "uObjCol");
            }
        }



    }

}