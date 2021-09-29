#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 2) in vec2 aTexCoords;
layout(location = 3) in vec3 aNormal;

out vec3 FragPos;
out vec2 TexCoords;
out vec3 Normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
    vec4 viewPos = vec4(aPosition, 1.0) * model * view ;
    FragPos = viewPos.xyz; 
    TexCoords = aTexCoords;
    
    mat3 normalMatrix = inverse(mat3(view * model));
    Normal = normalMatrix * aNormal;

	gl_Position = vec4(aPosition, 1) * model * view * projection;
}
