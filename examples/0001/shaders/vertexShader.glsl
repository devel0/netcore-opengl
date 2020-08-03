// Using version GLSL version 3.3
#version 330 core

// input variables
layout(location = 0) in vec4 vPos;

void main() { gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0); }