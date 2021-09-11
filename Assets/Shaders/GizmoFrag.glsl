#version 330 core
layout(location = 0) out vec4 output_color;

uniform vec4 color;

void main()
{
	output_color = color;
}