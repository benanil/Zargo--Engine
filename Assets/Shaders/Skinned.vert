#version 430 core

const int MAX_BONES = 70;
const int MAX_WEIGHTS = 4;

layout(location = 0) in vec3 pos;
layout(location = 1) in vec2 tex;
layout(location = 2) in vec3 norm;
layout(location = 3) in vec4 weights;
layout(location = 4) in ivec4 boneIds; 

uniform mat4 model;
uniform mat4 viewProjection;
uniform mat4 bones[MAX_BONES];

uniform int BoneCount;
uniform float time;

out vec2 TexCoord;
out vec3 FragPos;

out vec3 Normal;

void main()
{
    mat4 boneTransform = mat4(0);
    
    for (int i = 0; i < MAX_BONES; i++) {
    	if (weights[i] == -1) break;
    	boneTransform += bones[boneIds[i]] * weights[i];
    }
	
	vec4 newPos = vec4(pos, 1) * transpose(boneTransform);

	TexCoord = tex;
	gl_Position = newPos * model * viewProjection;
}