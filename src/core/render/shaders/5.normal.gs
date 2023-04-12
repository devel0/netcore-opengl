#version 440 core

// 5.normal.gs (optional)

//---------------------------------------------------------------
// uniform from the code
//---------------------------------------------------------------

uniform mat4 uObject;
uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

uniform vec3 uBBoxMin;
uniform vec3 uBBoxMax;
uniform float uNormalFactor;

uniform bool uFigureScreenCoord;

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
    // camera pos [world]
    vec3 cameraPos;
}
gs_in[];

//---------------------------------------------------------------
// variables to next stage
//---------------------------------------------------------------

layout(line_strip, max_vertices = 6) out;

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
    // camera pos [world]
    vec3 cameraPos;
}
vs_out;

//---------------------------------------------------------------
// BODY
//---------------------------------------------------------------

void main()
{
    if (uFigureScreenCoord)
        return;

    mat4 m1 = uModel * uObject;
    mat4 m2 = uProjection * uView;
    mat4 m = uProjection * uView * uModel * uObject;

    vec3 bboxSize = abs(uBBoxMax - uBBoxMin);
    float nFactor = max(max(bboxSize.x, bboxSize.y), bboxSize.z) * uNormalFactor;

    vs_out.oPos = vec3(0, 0, 0);
    vs_out.oNormal = vec3(0, 0, 0);
    vs_out.pos = vec3(0, 0, 0);
    vs_out.normal = vec3(0, 0, 0);
    vs_out.matColor = vec4(1, 1, 0, 1);
    vs_out.matProp = vec3(1, 1, 1);
    vs_out.texCoord = vec2(0, 0);
    vs_out.cameraPos = vec3(0, 0, 0);

    for (int i = 0; i < 3; ++i) {
        int idx = i;

        gl_Position = m * vec4(gs_in[idx].oPos, 1);
        EmitVertex();

        gl_Position = m * vec4(gs_in[idx].oPos + gs_in[idx].oNormal * nFactor, 1);
        EmitVertex();

        EndPrimitive();
    }
}