// IndirectLight
#version 400 core

// gbuffer stuff and returning color
layout (location = 0) out vec3 gPosition;
layout (location = 1) out vec3 gNormal;
layout (location = 2) out vec4 gAlbedoSpec;

in vec3 pworldSpacePosition;
in vec3 pLocalPosition;
in vec3 Normal;
in vec2 texCoords;
in vec3 FragPos;
in vec3 ScreenNormal; // for ssao

//these are same for all shaders
uniform float ambientStrength; // dontUse
uniform vec3 ambientColor    ; // dontUse
uniform float sunIntensity   ; // dontUse
uniform float sunAngle       ; // dontUse
uniform vec3 sunColor        ; // dontUse
uniform vec3 viewpos         ; // dontUse

uniform sampler2D albedoTex;
uniform sampler2D AOTex;

uniform float AOIntensity = .2;
uniform vec4 color = vec4(1,1,1,1);

// point light
struct Light
{
    float angle;
    float intensity; // = 1;     // outerCutOff = 0.91
    int  type      ;
    vec3 color     ;
    vec3 position  ;
    vec3 direction ; // for spot lights
};

const int LightSize = 4;
uniform Light lights[LightSize];
uniform int lightCount;

// specular
uniform vec3 specularColor = vec3(1);
uniform float specPower = 1;

uniform int mode; // enum ambient specular

// UniformEnd

#define spotLight 60
#define zero  vec3(0)
#define specular 1

const int maxLightDistance = 100;

float Distance(in vec3 a, in vec3 b) {
    float diff_x = a.x - b.x;
    float diff_y = a.y - b.y;
    float diff_z = a.z - b.z;
    return sqrt(diff_x * diff_x + diff_y * diff_y + diff_z * diff_z);
}

float saturate(in float x) {
  return max(0, min(1, x));
}

float CalculateSpotLight(in Light light, in vec3 surfaceToLight)
{
     float theta = dot(normalize(surfaceToLight) , light.direction) ;
     
     if (theta * 180 > light.angle) {
        return theta / length(surfaceToLight);
     }
     return 0;
}

float CalculatePointLight(in Light light, in vec3 surfaceToLight)
{
    float theta = dot(Normal, surfaceToLight);
    
    if (theta > 0) { // light in correct direction
        return ((theta / length(surfaceToLight))) * light.intensity;
    }
    return 0;
}

vec3 CalculateLights()
{
    vec3 result = zero;

    for (int i = 0; i < lightCount; i++) {
        
        Light light = lights[i];
        float pointDistance = Distance(light.position, FragPos);

        if (pointDistance < maxLightDistance)
        {
            vec3 surfaceToLight = light.position - FragPos; // to light vector
            vec3 viewDir = normalize(viewpos - FragPos);

            vec3 attenuation = light.color * (light.type == spotLight ? CalculateSpotLight (light, surfaceToLight) 
                                                                      : CalculatePointLight(light, surfaceToLight));
            // diffuse
            vec3 diffuse = sunColor * clamp(dot(Normal, normalize(surfaceToLight) * sunIntensity), 0.0, 1.0); 
            result += diffuse * attenuation;
            // specular
            if (mode == specular) {
                vec3 spec = specularColor * pow(saturate(dot(viewDir + normalize(surfaceToLight), Normal)), specPower); 
                result += spec * attenuation;
            }
        }
    }

    return result;
}

void main()
{
     vec3 SunDirection = vec3(0, sin(sunAngle), cos(sunAngle));
     
     vec3 tex = texture(albedoTex, texCoords).xyz;
     float ao = -max(.2 , min(1, texture(AOTex, texCoords).r * AOIntensity));
     
     // directional light calculation
     float NdotL = clamp(dot(Normal, SunDirection * sunIntensity),0.2,1.0);
     vec3 diffuseTerm = sunColor * NdotL;
     
     // specular
     if (mode == specular) {
         vec3 viewDir = normalize(viewpos - FragPos);
         vec3 spec = specularColor * pow(max(dot(viewDir, reflect(-SunDirection, Normal)), 0), specPower); 
         diffuseTerm += spec;
     }
     
     vec3 result = (ambientColor * ambientStrength + diffuseTerm + CalculateLights()) * tex;
     
     gPosition = FragPos;
     gNormal = ScreenNormal;
     gAlbedoSpec = vec4(result * ao, 1.0) * color;
}


