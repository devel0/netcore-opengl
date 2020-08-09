#version 330 core

// variables coming from vertex shader
in vec3 fPos;

// variables coming from code
uniform vec3 uObjCol;

// output variables
out vec4 FragColor;

void main() {
  FragColor = vec4(uObjCol, 1.0);
}