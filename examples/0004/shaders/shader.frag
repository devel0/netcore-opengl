#version 330 core

// variables coming from vertex shader
in vec3 fPos;
in vec3 fNormal;

// variables coming from code
uniform float uAmb;
uniform float uAlpha;
uniform vec3 uObjCol;
uniform vec3 uLightPos;
uniform vec3 uLightPos2;

// output variables
out vec4 FragColor;

void main() {
  vec3 lightColor = vec3(1, 0.9, 0.34);
  vec3 lightColor2 = vec3(0.18, 0.55, 0.56);

  vec3 norm = normalize(fNormal);
  vec3 lightDir = normalize(uLightPos - fPos);
  vec3 lightDir2 = normalize(uLightPos2 - fPos);

  float diff = max(dot(norm, lightDir), 0.0);
  float diff2 = max(dot(norm, lightDir2), 0.0);
  vec3 result = (uAmb * lightColor + diff * lightColor + diff2 * lightColor2) * uObjCol;

  FragColor = vec4(result, uAlpha);
}