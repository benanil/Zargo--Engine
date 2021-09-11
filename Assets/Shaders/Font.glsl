#version 400 core
out vec4 out_color;

in vec2 pTexCoords;

uniform sampler2D texture0;
uniform vec4 color;
uniform vec2 fontCoords;

const float smoothing = 1.0/16.0;
const int AtlasSize = 480;

void main()
{   
    vec2 realTexCoords = vec2(pTexCoords.x, AtlasSize - pTexCoords.y) / vec2(AtlasSize);
    // float dist = texture(texture0, realTexCoords + fontCoords).r;
    // float alpha = smoothstep(0.5 - smoothing, 0.5 + smoothing, dist);

    out_color = vec4(color.rgb, texture(texture0, realTexCoords + fontCoords).r);
}  