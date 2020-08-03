#version 330 core

// variables from code
uniform vec3 ObjCol;

// output variables
out vec4 FragColor;

void main() { FragColor = vec4(ObjCol, 1.0); }