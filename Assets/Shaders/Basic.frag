//

#version 330 core

// g buffer stuff
layout (location = 0) out vec3 gPosition;
layout (location = 1) out vec4 gAlbedoSpec; 
layout (location = 2) out vec3 gNormal;

//these are same for all shaders
uniform float ambientStrength; // dontUse
uniform vec3  ambientColor   ; // dontUse
uniform float sunIntensity   ; // dontUse
uniform float sunAngle       ; // dontUse
uniform vec3  sunColor       ; // dontUse
uniform vec3  viewpos        ; // dontUse

in vec3 worldSpacePosition;
in vec3 LocalPosition;
in vec3 Normal;
in vec2 TexCoords;
in vec3 FragPos;
in vec3 ScreenNormal; // for ssao

uniform sampler2D albedoTex;
uniform sampler2D AOTex;
uniform float AOIntensity = .2;

uniform vec4 color = vec4(1,1,1,1);

// no point light

void main()
{
    vec3 LightDirection = vec3(0, sin(sunAngle), cos(sunAngle));
    
    vec3 tex = texture(albedoTex, TexCoords).xyz;
    float ao = 1 - max(.2 , min(1, texture(AOTex, TexCoords).r * AOIntensity));

    vec3 L = normalize(LightDirection) * sunIntensity;
    
    float NdotL = clamp(dot(Normal,L),0.0,1.0);

    vec3 diffuseTerm = sunColor * NdotL;

    vec3 result = (ambientColor * ambientStrength + diffuseTerm) * tex;

    gNormal = ScreenNormal;
    gPosition = FragPos;
    gAlbedoSpec = vec4(result * ao, 1.0) * color;
}
