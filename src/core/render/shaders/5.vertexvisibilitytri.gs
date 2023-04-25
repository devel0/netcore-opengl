#version 440 core

// 5.vertexvisibilitytri.gs (optional)

//---------------------------------------------------------------
// variables from previous stage
//---------------------------------------------------------------

layout(triangles) in;

in VertexData
{
    // vertex pos [object]
    vec3 oPos;
    // vertex normal [object]
    vec3 oNormal;
    // fragment pos [world]
    vec3 pos;
    vec3 normal;
    vec4 matColor;
    vec3 matProp;
    vec2 texCoord;
    flat uint flags;
    // camera pos [world]
    vec3 cameraPos;
}
gs_in[];

//---------------------------------------------------------------
// variables to next stage
//---------------------------------------------------------------

layout(points, max_vertices = 3) out;

//---------------------------------------------------------------
// variables to next stage
//---------------------------------------------------------------

out vec4 FragPos;

out VertexData
{
    // vertex pos [object]
    vec3 oPos;
    // vertex normal [object]
    vec3 oNormal;
    // fragment pos [world]
    vec3 pos;
    vec3 normal;
    vec4 matColor;
    vec3 matProp;
    vec2 texCoord;
    flat uint flags;
    // camera pos [world]
    vec3 cameraPos;
}
vs_out;

//---------------------------------------------------------------
// BODY
//---------------------------------------------------------------

void main()
{
    for (int i = 0; i < 3; ++i) {
        int idx = i;

        vs_out.pos = gs_in[idx].pos;
        vs_out.normal = gs_in[idx].normal;
        vs_out.matColor = gs_in[idx].matColor;
        vs_out.matProp = gs_in[idx].matProp;
        vs_out.texCoord = gs_in[idx].texCoord;
        vs_out.flags = gs_in[idx].flags;
        vs_out.cameraPos = gs_in[idx].cameraPos;
        gl_Position = gl_in[idx].gl_Position;              
        EmitVertex();
    }

    EndPrimitive();
}