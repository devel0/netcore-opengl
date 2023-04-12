#version 440 core

// 3.depth.fs

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

layout(std430, binding = 1) buffer PointLightsBlock
{
    GLPointLightNfo ptLights[];
};

//---------------------------------------------------------------
// variables from previous stage
//---------------------------------------------------------------

in vec4 FragPos;

//---------------------------------------------------------------
// uniform from the code
//---------------------------------------------------------------

uniform mat4 uModel;
uniform int uLightIdx;
uniform float uFarPlaneLight;

//---------------------------------------------------------------
// BODY
//---------------------------------------------------------------

void main()
{
    vec3 lightPos = ptLights[uLightIdx].position;

    float lightDistance = length(FragPos.xyz - lightPos);

    lightDistance /= uFarPlaneLight;

    gl_FragDepth = lightDistance;
}