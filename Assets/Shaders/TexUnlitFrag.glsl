#version 330 core
layout (location = 0) out vec4 gPosition;
layout (location = 1) out vec3 gNormal;
layout (location = 2) out vec4 gAlbedoSpec;

in vec3 FragPos;
in vec3 Normal;
in vec2 texCoords;

uniform sampler2D texture0;
uniform vec4 color;

void main()
{
	vec4 col = texture(texture0, texCoords) * color;
	
	if(col.w == 0) discard;

	gAlbedoSpec = col;
}