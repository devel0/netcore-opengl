#version 440 core

#define VERTEX_FLAG_SELECTED 1

// 6.main.fs

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

layout(std430, binding = 2) buffer PointLightsBlock
{
    GLPointLightNfo ptLights[];
};

//---------------------------------------------------------------
// variables from previous stage
//---------------------------------------------------------------

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
fs_in;

//---------------------------------------------------------------
// uniform from the code
//---------------------------------------------------------------

uniform mat4 uModel;
uniform bool uFigureScreenCoord;

uniform bool uUseTexture;
uniform bool uShadows;
uniform float uFarPlaneLight;
uniform bool uEvalLight;
uniform bool uIsText;
uniform vec4 uTextColor;
uniform bool uFigureHighlight;
uniform float uFigureAlpha;

uniform float uOverrideAmbient;
uniform bool uOverrideAmbientEnabled;
uniform float uOverrideDiffuse;
uniform bool uOverrideDiffuseEnabled;
uniform float uOverrideSpecular;
uniform bool uOverrideSpecularEnabled;

uniform sampler2D uMainTexture;
uniform samplerCubeArray uDepthMap;

//---------------------------------------------------------------
// output variables to next stage
//---------------------------------------------------------------

out vec4 FragColor;

//---------------------------------------------------------------
// BODY
//---------------------------------------------------------------

float ShadowCalculation(int lightIdx, vec3 lightPos)
{
    vec3 fragToLight = fs_in.pos - lightPos;

    float closestDepth = texture(uDepthMap, vec4(fragToLight, lightIdx)).r;

    closestDepth *= uFarPlaneLight;

    float currentDepth = length(fragToLight);

    float bias = 0.15;
    float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;

    return shadow;
}

void main()
{
    int ptLightCnt = ptLights.length();

    vec4 materialColor = fs_in.matColor;

    float materialAmbientStrength = uOverrideAmbientEnabled ? uOverrideAmbient : fs_in.matProp.x;
    float materialDiffuseStrength = uOverrideDiffuseEnabled ? uOverrideDiffuse : fs_in.matProp.y;
    float materialSpecularStrength = uOverrideSpecularEnabled ? uOverrideSpecular : fs_in.matProp.z;

    if (uUseTexture) {
        vec4 tColor = texture(uMainTexture, fs_in.texCoord);

        if (tColor.w < 0.5)
            discard;

        if (uIsText) {
            materialColor = uTextColor;
        } else {
            materialColor = tColor;
        }
    }

    if (uFigureAlpha >= 0)
        materialColor = vec4(materialColor.xyz, uFigureAlpha);

    if (uFigureHighlight)
        materialColor = vec4(1, 1, 0, 1);

    if ((fs_in.flags & VERTEX_FLAG_SELECTED) != 0)
        materialColor = vec4(0, 1, 0, 1);

    if (uFigureScreenCoord) {
        FragColor = materialColor;
    } else {
        vec4 result = vec4(0, 0, 0, 0);

        if (ptLightCnt == 0) {
            result = materialColor;
        } else {
            if (!uEvalLight) {
                result = materialColor;
            } else {
                int lightIdx = 0;

                while (lightIdx < ptLightCnt) {
                    vec3 _lightPos = ptLights[lightIdx].position;

                    // light pos [world]
                    vec3 lightPos = _lightPos;

                    vec3 lightAmbient = ptLights[lightIdx].ambient * materialAmbientStrength;
                    vec3 lightDiffuse = ptLights[lightIdx].diffuse * materialDiffuseStrength;
                    vec3 lightSpecular = ptLights[lightIdx].specular * materialSpecularStrength;

                    vec3 lightDir = normalize(lightPos - fs_in.pos);
                    // dot(u,v) = 0 (perpendicular)
                    // dot(u,v) > 0 (in light)
                    // dot(u,v) < 0 (off light)
                    float diff = max(dot(fs_in.normal, lightDir), 0.0);

                    vec3 viewDir = normalize(fs_in.cameraPos - fs_in.pos);
                    vec3 reflectDir = reflect(-lightDir, normalize(fs_in.normal));
                    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
                    if (diff == 0)
                        spec = 0;

                    vec3 ambient = lightAmbient;
                    vec3 diffuse = diff * lightDiffuse;
                    vec3 specular = spec * lightSpecular;

                    float shadow = uShadows ? ShadowCalculation(lightIdx, lightPos) : 0.0;

                    //

                    float distance = length(lightPos - fs_in.pos);

                    float lightConstant = ptLights[lightIdx].constant;
                    float lightLinear = ptLights[lightIdx].linear;
                    float lightQuadratic = ptLights[lightIdx].quadratic;

                    float attenuation = 1.0 / (lightConstant + lightLinear * distance + lightQuadratic * (distance * distance));

                    ambient *= attenuation;
                    diffuse *= attenuation;
                    specular *= attenuation;

                    //

                    vec4 lv = vec4(ambient + (1.0 - shadow) * (diffuse + specular), 1);
                    vec4 thisLightResult = lv * materialColor;

                    result = vec4(max(result.x, thisLightResult.x),
                        max(result.y, thisLightResult.y),
                        max(result.z, thisLightResult.z),
                        max(result.w, thisLightResult.w));

                    // if (shadow > 0) result = vec3(1,0,0);

                    ++lightIdx;
                }
            }
        }

        FragColor = result;
    }
}