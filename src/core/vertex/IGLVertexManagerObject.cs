namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Interface for GL objects such as <see cref="IGLFigure"/>, <see cref="IGLPrimitive"/> and <see cref="IGLVertex"/>.<br/>
/// The purpose is to ensure that each object implementing this interface
/// allow parent traversal till root vertex manager object which purpouse
/// is to manage the collection of vertexes.
/// </summary>
public interface IGLVertexManagerObject
{

    /// <summary>
    /// Vertex manager which this object is registered to.<br/>
    /// Before the object is added to the <see cref="SearchAThing.OpenGL.Render.GLModel"/>,
    /// therefore to the <see cref="GLVertexManager"/>,
    /// or after the object is removed from, this pointer is null.
    /// </summary>    
    GLVertexManager? ParentVertexManager { get; }

    /// <summary>
    /// Create a copy of this gl vertex manager object copying all relevant
    /// data except the <see cref="ParentVertexManager"/> that will default to null for copies
    /// object until added to a vertex manager.
    /// </summary>    
    IGLVertexManagerObject Copy();    

}

public static partial class Ext
{

    /// <summary>
    /// States if this object is currently attached to a gl vertex manager.
    /// </summary>    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAttached(this IGLVertexManagerObject obj) =>
        obj.ParentVertexManager is not null;

    /// <summary>
    /// Throws an exception if this object isn't detached from any vertex managers.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EnsureDetached(this IGLVertexManagerObject obj)
    {
        if (obj.IsAttached())
            throw new Exception($"object [{obj.GetType().Name}] already attached. use a copy instead.");
    }

}