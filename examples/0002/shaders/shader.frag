#version 330 core

// variables coming from vertex shader
in vec3 fPos;
in vec3 fNormal;

// variables coming from code
uniform float uAmb;
uniform float uAlpha;
uniform vec3 uObjCol;
uniform vec3 uLightPos;

// output variables
out vec4 FragColor;

void main() {
  vec3 lightColor = vec3(1.0, 1.0, 1.0); // white light

  vec3 norm = normalize(fNormal);
  vec3 lightDir = normalize(uLightPos - fPos);
  float diff = max(dot(norm, lightDir), 0.0);
  vec3 result = (uAmb * lightColor + diff * lightColor) * uObjCol;

  FragColor = vec4(result, uAlpha);
}