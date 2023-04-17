namespace SearchAThing.OpenGL.Core;

public partial class GLModel
{

    /// <summary>
    /// Json serialized representation of this model.
    /// </summary>    
    public string Serialize() => JsonConvert.SerializeObject(this, SerializerSettings);

    /// <summary>
    /// Save json serialized representation of this model to given pathfilename.
    /// </summary>    
    public void SerializeToFile(string pathfilename) => File.WriteAllText(pathfilename, Serialize());

    /// <summary>
    /// Deserialize given gl model retrieving gl figures.
    /// </summary>    
    public IEnumerable<GLFigureBase> DeserializeFigures(string json)
    {
        var obj = JsonConvert.DeserializeObject<GLModel>(json, SerializerSettings);

        if (obj is not null)
        {
            foreach (var fig in obj.Figures)
            {
                if (fig is GLTriangleFigure trifig && trifig.Texture2D is not null)
                    trifig.Texture2D.GLContext = GLContext;

                yield return fig;
            }
        }
    }

    /// <summary>
    /// Deserialize gl figures from given serialized gl model pathfilename.
    /// </summary>    
    public IEnumerable<GLFigureBase> DeserializeFiguresFromFile(string pathfilename) =>
        DeserializeFigures(File.ReadAllText(pathfilename));

    /// <summary>
    /// Default settings for json serialization of model.
    /// </summary>
    public static JsonSerializerSettings SerializerSettings => new JsonSerializerSettings
    {
        Error = errhandler,
        Formatting = Formatting.Indented,
        TypeNameHandling = TypeNameHandling.All,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    private static void errhandler(object? sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
    {
        throw new Exception($"{e.ErrorContext.Error.Message} ; Path: {e.ErrorContext.Path}");
    }

}

