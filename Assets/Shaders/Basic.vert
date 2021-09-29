#version 330 core
#extension GL_ARB_explicit_uniform_location : enable

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 FragPos;
out vec3 Normal;
out vec3 ScreenNormal;// for ssao

out vec2 texCoord;

void main(void)
{
    Normal = aNormal * mat3(transpose(inverse(model)));
        
    FragPos = vec3(vec4(aPosition, 1.0) * model);
    ScreenNormal = aNormal * inverse(mat3(view * model));
    texCoord = aTexCoord;
    
    gl_Position = vec4(aPosition, 1) * model * view * projection;
}