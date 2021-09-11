#version 330 core
#extension GL_ARB_explicit_uniform_location : enable

layout(location = 0) in vec3 aPosition;

layout(location = 12) uniform mat4 model;
layout(location = 28) uniform mat4 lightSpaceMatrix;

void main(void)
{
	gl_Position = vec4(aPosition, 1) * model * lightSpaceMatrix;
}
