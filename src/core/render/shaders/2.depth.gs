#version 440 core

// 2.depth.gs

//---------------------------------------------------------------
// storage blocks
//---------------------------------------------------------------

struct GLPointLightNfo {
    // light position [world]
    vec3 position;
    float constant;

    vec3 ambient;
    float linear;

    vec3 diffuse;
    float quadratic;

    vec3 specular;
    float specular_pad;
};

layout(std430, binding = 3) buffer PointLightsMatrixesBlock
{
    mat4 shadowMatrices[];
};

//---------------------------------------------------------------
// variables from previous stage
//---------------------------------------------------------------

layout(triangles) in;

//---------------------------------------------------------------
// uniform from the code
//---------------------------------------------------------------

uniform int uLightIdx;

#define CUBE_FACES 6
#define TRI_VERTEXES 3

//---------------------------------------------------------------
// variables to next stage
//---------------------------------------------------------------

layout(triangle_strip, max_vertices = CUBE_FACES * TRI_VERTEXES) out;

out vec4 FragPos;

//---------------------------------------------------------------
// BODY
//---------------------------------------------------------------

void main()
{

    for (int face = 0; face < 6; ++face) {
        gl_Layer = uLightIdx * 6 + face;
        for (int i = 0; i < 3; ++i) {
            FragPos = gl_in[i].gl_Position;
            gl_Position = shadowMatrices[uLightIdx * 6 + face] * FragPos;
            EmitVertex();
        }
        EndPrimitive();
    }
}