#version 330 core
layout(location = 0) out vec4 out_color;

in vec2 texCoords;

uniform sampler2D texture0;
uniform vec4 color;

void main()
{
	vec4 col = texture(texture0, texCoords) * color;
	
	if(col.w == 0) discard;

	out_color = col;
}