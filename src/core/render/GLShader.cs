namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Gl pipeline object.<br/>
/// Contains vertex, geometry and fragment shaders where defined and helper method to set uniforms, attributes ans storage blocks.
/// </summary>
public class GLPipeline : IGLContextObject
{

    /// <summary>
    /// Gl context.
    /// </summary>    
    public GLContext GLContext { get; private set; }

    /// <summary>
    /// Gl api.
    /// </summary>
    public GL GL => GLContext.GL;

    /// <summary>
    /// GLSL source code of the vertex shader.
    /// </summary>    
    public string? VertexShaderSource { get; private set; } = null;

    /// <summary>
    /// GLSL source code of the geometry shader.
    /// </summary>    
    public string? GeometryShaderSource { get; private set; } = null;

    /// <summary>
    /// GLSL source code of the fragment shader.
    /// </summary>    
    public string? FragmentShaderSource { get; private set; } = null;

    HashSet<string> missingUniformLocations = new HashSet<string>();

    HashSet<string> missingAttributeLocations = new HashSet<string>();

    /// <summary>
    /// If true shader warning are logged to Debug.WriteLine.
    /// </summary>
    public bool TraceWarnings = DEFAULT_TRACE_LOCATION_WARNINGS;

    uint _VertexShader = 0;

    /// <summary>
    /// Gl vertex shader id.
    /// </summary>    
    public uint VertexShader
    {
        get
        {
            if (VertexShaderSource is null) throw new Exception($"vertex shader source not defined");
            return _VertexShader;
        }
    }

    uint _GeometryShader = 0;

    /// <summary>
    /// Gl geometry shader id.
    /// </summary>    
    public uint GeometryShader
    {
        get
        {
            if (GeometryShaderSource is null) throw new Exception($"geometry shader source not defined");
            return _GeometryShader;
        }
    }

    uint _FragmentShader = 0;

    /// <summary>
    /// Gl fragment shader id.
    /// </summary>    
    public uint FragmentShader
    {
        get
        {
            if (FragmentShaderSource is null) throw new Exception($"fragment shader source not defined");
            return _FragmentShader;
        }
    }

    /// <summary>
    /// Gl Program id.
    /// </summary>
    public uint Program = 0;

    /// <summary>
    /// If set true an exception will generated if setting a non existant uniform. (Default: false).
    /// </summary>    
    public bool ExceptionOnMissingLocation { get; set; }

    /// <summary>
    /// Custom friendly name of the shader.
    /// </summary>    
    public string FriendlyName { get; private set; } = "";

    string prefix => FriendlyName.Length > 0 ? $"{FriendlyName}: " : "";

    /// <summary>
    /// Create a gl shader.
    /// </summary>
    /// <param name="glContext">Gl context.</param>
    /// <param name="vertexShaderSource">(Optional) GLSL source code of the vertex shader.</param>
    /// <param name="geometryShaderSource">(Optional) GLSL source code of the geometry shader.</param>
    /// <param name="fragmentShaderSource">(Optional) GLSL source code of the fragment shader.</param>
    /// <param name="exceptionOnMissingLocation">(Optional) If true an exception will generated if setting a non existant uniform. (Default: false).</param>
    /// <param name="friendlyName">(Optional) Friendly shader name.</param>
    public GLPipeline(GLContext glContext,
        string? vertexShaderSource = null,
        string? geometryShaderSource = null,
        string? fragmentShaderSource = null,
        bool exceptionOnMissingLocation = false,
        string friendlyName = "")
    {
        if (vertexShaderSource is null && fragmentShaderSource is null && geometryShaderSource is null)
            throw new ArgumentException($"{prefix}no shader source given");

        this.GLContext = glContext;
        this.VertexShaderSource = vertexShaderSource;
        this.GeometryShaderSource = geometryShaderSource;
        this.FragmentShaderSource = fragmentShaderSource;
        this.ExceptionOnMissingLocation = exceptionOnMissingLocation;
        this.FriendlyName = friendlyName;

        var GL = glContext.GL;

        if (VertexShaderSource is not null)
        {
            _VertexShader = GL.CreateShader(GLEnum.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);
            GL.CompileShader(VertexShader);

            var infoLog = GL.GetShaderInfoLog(VertexShader);
            if (!string.IsNullOrWhiteSpace(infoLog))
                throw new Exception($"{prefix}Error compiling vertex shader {infoLog}");
        }

        if (GeometryShaderSource is not null)
        {
            _GeometryShader = GL.CreateShader(GLEnum.GeometryShader);
            GL.ShaderSource(GeometryShader, GeometryShaderSource);
            GL.CompileShader(GeometryShader);

            var infoLog = GL.GetShaderInfoLog(GeometryShader);
            if (!string.IsNullOrWhiteSpace(infoLog))
                throw new Exception($"{prefix}Error compiling geometry shader {infoLog}");
        }

        if (FragmentShaderSource is not null)
        {
            _FragmentShader = GL.CreateShader(GLEnum.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);
            GL.CompileShader(FragmentShader);

            var infoLog = GL.GetShaderInfoLog(FragmentShader);
            if (!string.IsNullOrWhiteSpace(infoLog))
                throw new Exception($"{prefix}Error compiling fragment shader {infoLog}");
        }

        Program = GL.CreateProgram();
        if (VertexShaderSource is not null) GL.AttachShader(Program, VertexShader);
        if (GeometryShaderSource is not null) GL.AttachShader(Program, GeometryShader);
        if (FragmentShaderSource is not null) GL.AttachShader(Program, FragmentShader);
        GL.LinkProgram(Program);

        {
            var infoLog = GL.GetProgramInfoLog(Program);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                if (infoLog.Lines().Select(w => w.StripBegin(".")).Where(w => w.Length > 0)
                    .All(w => w.StartsWith("warning:") || w.StartsWith(".warning:")))
                {
                    if (TraceWarnings)
                        Console.WriteLine($"{prefix}Warnings compiling program\n{infoLog}");
                }
                else
                    throw new Exception($"{prefix}Error compiling program\n{infoLog}");
            }
        }

        if (FragmentShaderSource is not null) GL.DeleteShader(FragmentShader);
        if (GeometryShaderSource is not null) GL.DeleteShader(GeometryShader);
        if (VertexShaderSource is not null) GL.DeleteShader(VertexShader);
    }

    /// <summary>
    /// Use the program containing pipeline of shaders configured here.
    /// </summary>
    public void Use() => GL.UseProgram(Program);

    /// <summary>
    /// Retrieve the uniform id by the given uniform name.
    /// </summary>
    /// <param name="name">Name of the location for which retrieve the id.</param>
    /// <returns>Id of the uniform.</returns>
    public uint? GetUniformLocation(string name)
    {
        if (missingUniformLocations.Contains(name)) return null;

        var res = GL.GetUniformLocation(Program, name);
        if (res < 0)
        {
            missingUniformLocations.Add(name);

            var msg = $"{prefix}Given uniform location [{name}] not found.";

            if (ExceptionOnMissingLocation)
                throw new Exception(msg);

            else if (TraceWarnings)
                Debug.WriteLine(msg);

            return null;
        }

        return (uint)res;
    }

    /// <summary>
    /// Retrieve the attribute location by the given attribute name.
    /// </summary>
    /// <param name="name">Attribute name for which retrieve the id.</param>
    /// <returns>Id of the attribute.</returns>
    public uint? GetAttributeLocation(string name)
    {
        if (missingAttributeLocations.Contains(name)) return null;
        var res = GL.GetAttribLocation(Program, name);
        if (res < 0)
        {
            missingAttributeLocations.Add(name);

            var msg = $"{prefix}Given attribute location [{name}] not found.";

            if (ExceptionOnMissingLocation)
                throw new Exception(msg);

            else if (TraceWarnings)
                Debug.WriteLine(msg);

            return null;
        }

        return (uint)res;
    }

    /// <summary>
    /// Eetrieve program resource index of given ssbo and retrieve associated binding.
    /// a shader unique shared storage binding is auto incremented if no default associated binding found in shader.
    /// </summary>    

    /// <summary>
    /// Retrieve program resource index of given SSBO and retrieve associated binding.<br/>
    /// Unique shader storage binding is auto incremented if no default associated binding was found in the shader.
    /// </summary>
    /// <param name="name">SSBO name.</param>
    /// <param name="storageBlockIndex">Resulting SSBO block index.</param>
    /// <param name="storageBlockBinding">Resulting SSBO block binding.</param>
    /// <returns>True if SSBO found.</returns>
    public bool GetStorageBlockBinding(string name, out uint storageBlockIndex, out uint storageBlockBinding)
    {
        storageBlockIndex = GL.GetProgramResourceIndex(Program, GLEnum.ShaderStorageBlock, name);

        if ((int)storageBlockIndex == -1)
            throw new Exception($"can't find storage block [{name}] in shader [{this.FriendlyName}]");

        var props = (new ProgramResourceProperty[] { ProgramResourceProperty.BufferBinding }).AsSpan();
        var resParams = (new int[props.Length]).AsSpan();

        GL.GetProgramResource(Program, ProgramInterface.ShaderStorageBlock, storageBlockIndex,
            props, out var length, resParams);

        storageBlockBinding = (uint)resParams[0];

        if (storageBlockBinding == 0) return false;

        return true;
    }

    /// <summary>
    /// Set bool uniform variable.
    /// </summary>
    /// <param name="name">Uniform variable name.</param>
    /// <param name="value">Value to set.</param>
    public void SetBool(string name, bool value)
    {
        var location = GetUniformLocation(name);
        if (location is not null) GL.Uniform1((int)location, value ? 1 : 0);
    }

    /// <summary>
    /// Set int uniform variable.
    /// </summary>
    /// <param name="name">Uniform variable name.</param>
    /// <param name="value">Value to set.</param>
    public void SetInt(string name, int value)
    {
        var location = GetUniformLocation(name);
        if (location is not null) GL.Uniform1((int)location, value);
    }

    /// <summary>
    /// Set float uniform variable.
    /// </summary>
    /// <param name="name">Uniform variable name.</param>
    /// <param name="value">Value to set.</param>
    public void SetFloat(string name, float value)
    {
        var location = GetUniformLocation(name);
        if (location is not null) GL.Uniform1((int)location, value);
    }

    /// <summary>
    /// Set Vector2 uniform variable.
    /// </summary>
    /// <param name="name">Uniform variable name.</param>
    /// <param name="value">Value to set.</param>
    public void SetVec2(string name, in Vector2 value)
    {
        var location = GetUniformLocation(name);
        if (location is not null) GL.Uniform2((int)location, value);
    }

    /// <summary>
    /// Set Vector3 uniform variable.
    /// </summary>
    /// <param name="name">Uniform variable name.</param>
    /// <param name="value">Value to set.</param>
    public void SetVec3(string name, in Vector3 value)
    {
        var location = GetUniformLocation(name);
        if (location is not null) GL.Uniform3((int)location, value);
    }

    /// <summary>
    /// Set Vector4 uniform variable.
    /// </summary>
    /// <param name="name">Uniform variable name.</param>
    /// <param name="value">Value to set.</param>
    public void SetVec4(string name, in Vector4 value)
    {
        var location = GetUniformLocation(name);
        if (location is not null) GL.Uniform4((int)location, value);
    }

    /// <summary>
    /// Set Matrix4x4 uniform variable.
    /// </summary>
    /// <param name="name">Uniform variable name.</param>
    /// <param name="value">Value to set.</param>
    public void SetMat4(string name, Matrix4x4 value)
    {
        var location = GetUniformLocation(name);
        unsafe
        {
            if (location is not null) GL.UniformMatrix4((int)location, 1, false, (float*)&value);
        }
    }
}