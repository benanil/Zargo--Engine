#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoords;
layout (location = 2) in vec3 aNormal;

out vec3 FragPos;
out vec2 TexCoords;
out vec3 Normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec2 texCoord;

void main()
{
    vec4 viewPos = vec4(aPos, 1.0) * model * view ;
    FragPos = viewPos.xyz; 
    TexCoords = aTexCoords;
    
    mat3 normalMatrix = inverse(mat3(view * model));
    Normal = normalMatrix * aNormal;
	
    texCoord = aTexCoords;
	gl_Position = vec4(aPos, 1) * model * view * projection;
}
