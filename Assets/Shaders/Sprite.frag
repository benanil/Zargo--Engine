
#version 400
out vec4 out_color;

in vec2 pTexCoords;

uniform sampler2D texture0;
uniform vec4 color;

const int AtlasSize = 480;

void main()
{   
    out_color = texture(texture0, pTexCoords) * color;
}  