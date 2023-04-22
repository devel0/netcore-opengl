#version 440 core

// 4.main.vs

//---------------------------------------------------------------
// uniform from the code
//---------------------------------------------------------------

uniform mat4 uObject;
uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

// camera pos [world]
uniform vec3 uCameraPos;

uniform bool uFigureHighlight;
uniform bool uFigureSelected;
uniform float uFigureAlpha;
uniform bool uFigureScreenCoord;
uniform mat4 uScreenProjection;
uniform mat4 uScreenModel;

//---------------------------------------------------------------
// vertex buffer components
//---------------------------------------------------------------

// vertex position [object]
layout(location = 0) in vec3 vPos;

// vertex normal [object]
layout(location = 1) in vec3 vNormal;

// vertex color [rgba]
layout(location = 2) in vec4 vMatColor;

// vertex material [ads]
layout(location = 3) in vec3 vMatProp;

// vertex texcoord
layout(location = 4) in vec2 vTexCoord;

// vertex flags
layout(location = 5) in uint vFlags;

//---------------------------------------------------------------
// variables to next stage
//---------------------------------------------------------------

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
    // set gl_Position ( clip space pos )

    if (uFigureScreenCoord)
        gl_Position = uScreenProjection * uScreenModel * uObject * vec4(vPos, 1.0);

    else
        gl_Position = uProjection * uView * uModel * uObject * vec4(vPos, 1.0);

    vs_out.oPos = vPos;
    vs_out.oNormal = vNormal;
    vs_out.pos = vec3(uModel * uObject * vec4(vPos, 1.0));
    vs_out.normal = vec3(uModel * uObject * vec4(vNormal, 1.0)) - vec3(uModel * uObject * vec4(0, 0, 0, 1));
    vs_out.matColor = vMatColor;
    vs_out.matProp = vMatProp;
    vs_out.texCoord = vTexCoord;
    vs_out.flags = vFlags;
    vs_out.cameraPos = uCameraPos;    
}
