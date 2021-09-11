#version 400

layout(location = 0) in vec2 aVertex;
layout(location = 1) in vec2 aTexCoord;

out vec2 pTexCoords;

uniform vec2 ScreenScale;
uniform vec2 position;

vec2 ScreenPointToNDC(in vec2 vertexPos) // to normalize device coordinates
{
	return (((position + vertexPos) / ScreenScale) - vec2(.5)) * 2;
}

void main(void) 
{
	pTexCoords = aTexCoord;
	gl_Position = vec4(ScreenPointToNDC(aVertex), 0 , 1);
}