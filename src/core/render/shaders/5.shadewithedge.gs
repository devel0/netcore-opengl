#version 440 core

// 5.shadewidthedge.gs (optional)

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

layout(line_strip, max_vertices = 4) out;

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
    for (int i = 0; i < 4; ++i) {
        int idx = i;
        if (i == 3)
            idx = 0;

        vs_out.pos = gs_in[idx].pos;
        vs_out.normal = gs_in[idx].normal;
        {
            vec3 rgb = gs_in[idx].matColor.xyz;

            // Photometric/digital ITU BT.709:
            float Y = .2126 * rgb.x + .7152 * rgb.y + .0722 * rgb.z;
            float f = .1;

            if (Y > .5) {
                vs_out.matColor = vec4(rgb * f, 1);
            } else {
                float maxrgb = max(max(rgb.x, rgb.y), rgb.z);

                if (maxrgb == 1)
                    vs_out.matColor = vec4(1, 1, 1, 1);
                else
                    vs_out.matColor = vec4(rgb + rgb * (1 - maxrgb) * (1 - f), 1);
            }
        }
        vs_out.matProp = gs_in[idx].matProp;
        vs_out.texCoord = gs_in[idx].texCoord;
        vs_out.cameraPos = gs_in[idx].cameraPos;
        gl_Position = gl_in[idx].gl_Position;
        EmitVertex();
    }

    EndPrimitive();
}