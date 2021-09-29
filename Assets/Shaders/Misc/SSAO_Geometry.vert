#version 330 core
#extension GL_ARB_explicit_uniform_location : enable

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoords;
layout (location = 2) in vec3 aNormal;

layout (location = 0)  uniform mat4 model;
layout (location = 16) uniform mat4 view;
layout (location = 32) uniform mat4 projection;

out vec3 FragPos;
out vec3 Normal;

void main()
{
    vec4 viewPos = vec4(aPos, 1.0) * model * view;
   
    FragPos = viewPos.xyz; 
    
    Normal = aNormal * transpose(inverse(mat3(view * model)));
    
    gl_Position = viewPos * projection;
}