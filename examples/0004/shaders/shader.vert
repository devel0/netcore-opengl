#version 330 core

// input variables from buffers
layout(location = 0) in vec3 vPos;
layout(location = 1) in vec3 vNormal;

// input variables from code
uniform mat4 uModel;
uniform mat4 uProjection;
uniform mat4 uView;

// variables flowing through next pipeline ( fragment shader )
out vec3 fPos;
out vec3 fNormal;

void main() {
  gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);

  fPos = vPos;
  fNormal = normalize(vec3(vec4(vNormal, 1.0)));
}
