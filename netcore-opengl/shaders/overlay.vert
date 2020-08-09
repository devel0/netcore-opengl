#version 330 core

// input variables from buffers
layout(location = 0) in vec3 vPos;

// variables flowing through next pipeline ( fragment shader )
out vec3 fPos;
out vec3 fNormal;

void main() {  
  gl_Position = vec4(vPos, 1.0);

  fPos = vPos;  
}
