#version 330 core
layout(location = 0) out vec4 out_color;

uniform sampler2D texture0;
uniform vec4 color;

void main()
{
	vec4 tex = texture(texture0, gl_PointCoord) ;
	
	if(length(tex) < 0.1) tex = vec4(1); // check texture exist

	vec4 col = color * tex;

	if(col.a == 0) discard;

	out_color = col;
}