#version 330 core
out vec4 FragColor;

in vec3 TexCoords;
in vec3 FragPos;

uniform samplerCube texture0;

uniform float angle = 2.14;          // uniform
const float sunSize = 40;          // uniform
const vec4 sunColor = vec4(1,1,.8,1); // uniform

const float skySize = 1000;
const float MaxDistance = 2000; // to sun from fragpos //skySize * 2;

float Distance(in vec3 a, in vec3 b) {
    float diff_x = a.x - b.x;
    float diff_y = a.y - b.y;
    float diff_z = a.z - b.z;
    return sqrt(diff_x * diff_x + diff_y * diff_y + diff_z * diff_z);
}

void main() {
    // sun calculation
    vec3 sunPos = vec3(0, sin(angle), cos(angle)) * skySize;

    float centerDistance = Distance(FragPos, vec3(0));
    vec3 centerAngle = normalize(vec3(0) - FragPos);
    vec3 realSunPos = sunPos + (centerAngle * (skySize - centerDistance));

    float sunDistance = Distance(realSunPos, FragPos);
   
    if (sunDistance < sunSize){
        FragColor = sunColor;
    }
    else { // horizon calculation

        vec4 edges = 1 - vec4(length(FragPos) / 3 / 5100);
        vec4 horizon = abs(vec4(1200 / (FragPos.y + MaxDistance),.45,.9,1));

        vec4 skyMultipler = vec4(1,.7,.9,1);

        horizon *= skyMultipler;
        edges *= .4;

        float sunDistNormalized = sunDistance / MaxDistance;
        
        const float maxEdgeSize = 1;
        edges = min(max(edges,.5), vec4(maxEdgeSize)) * (sunColor * (1 - sunDistNormalized));
        FragColor = (horizon + edges);
    }
}