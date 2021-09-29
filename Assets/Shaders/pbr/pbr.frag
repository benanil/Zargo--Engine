// IndirectLight HasShadows
#version 400

#define SpecularLighting 1
#define spotLight 60
#define zero  vec3(0);

layout(location = 0) out vec4 outputColor;

in vec3 Normal;
in vec2 texCoord;
in vec3 FragPos;
in vec4 lightSpaceFrag; 

// these are same for all shaders
// dontUse is an attribute for material parsing (required here)
uniform float ambientStrength; // dontUse     
uniform vec3 ambientColor;     // dontUse
uniform float sunIntensity;    // dontUse
uniform float sunAngle;        // dontUse
uniform vec3 sunColor;         // dontUse
uniform vec3 viewpos;          // dontUse

uniform sampler2D albedoTex; 
uniform sampler2D AOTex; 
uniform sampler2D roughnessTex; // black
uniform sampler2D shadowMap; // dontUse

uniform float bias; // dontUse
uniform float AOIntensity = 0.2;
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

const int maxLightDistance = 100;
const int LightSize = 10;
uniform Light lights[LightSize];
uniform int lightCount; // dontUse

// specular
uniform vec3 specularColor = vec3(1);
uniform float specPower = 1;

uniform int mode; // enum ambient specular

// pbr
uniform float roughnessPow;
uniform float metalic;

const float PI = 3.14159265359;

// UniformEnd

float DistributionGGX(in vec3 H, in float roughness)
{
    float a  = roughness * roughness;
    float a2 = a * a;
    float NdotH  = max(dot(Normal, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return a2 / max(denom, 0.0000001); // prevent divide by zero for roughness=0.0 and NdotH=1.0
}

float GeometrySchlickGGX(in float NdotV, in float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / PI;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}

float GeometrySmith(in vec3 V, in vec3 L, in float roughness)
{
    float NdotV = max(dot(Normal, V), 0.0);
    float NdotL = max(dot(Normal, L), 0.2);
    float ggx2  = GeometrySchlickGGX(NdotV, roughness);
    float ggx1  = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

vec3 fresnelSchlick(in float cosTheta, in vec3 F0, in float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
}

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
        return (theta / length(surfaceToLight)) * light.intensity;
    }
    return 0;
}

// this function calculates point and specular lights

vec3 CalculateLightsPBR(in vec3 V, in vec3 F0, in vec3 tex, in float roughness)
{
    vec3 result = zero;

    for (int i = 0; i < lightCount; i++) {
        
        Light light = lights[i];
        float lightDistance = Distance(light.position, FragPos);

        if (lightDistance < maxLightDistance)
        {
            vec3 surfaceToLight = light.position - FragPos;
            vec3 lightDirection = normalize(surfaceToLight);
            vec3 H = normalize(V + lightDirection);
            
            float attenuation = light.type == spotLight ? CalculateSpotLight(light, surfaceToLight) : 1.0 / (lightDistance * lightDistance);
            vec3 radiance = light.color * attenuation;
            
            if (attenuation < 0.1) continue;
            vec3 lightColor = light.color * attenuation;
            
            vec3 specular = zero;
            vec3  F   = fresnelSchlick(saturate(dot(H, V)), F0, roughness);

            if (mode == SpecularLighting)
            {   
                float NDF = DistributionGGX(H, roughness);   
                float G   = GeometrySmith(V, lightDirection, roughness);      
                vec3 numerator    = NDF * G * F; 
                float denominator = 4 * max(dot(Normal, V), 0.0) * max(dot(Normal, lightDirection), 0.0);
                specular     = numerator / max(denominator, 0.001); // prevent divide by zero for NdotV=0.0 or NdotL=0.0
            }
            
            vec3 kD     = (vec3(1.0) - F) * 1.0 - metalic;
            float NdotL = max(dot(Normal, lightDirection), 0.0);
            vec3 lightingResult = (kD * tex / PI + specular * specPower) * radiance * NdotL;
            result += lightingResult * lightColor;
        }
    }

    return result;
}


float ShadowCalculation()
{
    // perform perspective divide
    vec3 projCoords = lightSpaceFrag.xyz / lightSpaceFrag.w;
    // transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = texture(shadowMap, projCoords.xy).r; 
    // get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;
    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    for(int x = -1; x <= 1; ++x)
    {
        for(int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r; 
            shadow += currentDepth - bias > pcfDepth ? .44 : 1;        
        }    
    }
    shadow /= 9.0;

    return shadow;
}

 // in learn opengl he didnt calculate sun light, I add sun light and some modifications and result is good
void main()
{
    vec3 SunDirection = vec3(0, sin(sunAngle), cos(sunAngle));
    
    vec3 tex = texture(albedoTex, texCoord).xyz * color.xyz;
    float ao = 1 - max(.2 , min(1, texture(AOTex, texCoord).r * AOIntensity));
    float roughness = texture(roughnessTex, texCoord).r + roughnessPow;
    
    vec3 V = normalize(viewpos - FragPos);
    
    float NdotL = max(dot(Normal, SunDirection), 0.15);
    
    // calculate reflectance at normal incidence; if dia-electric (like plastic) use F0 
    // of 0.04 and if it's a metal, use the albedo color as F0 (metallic workflow)    
    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, tex, metalic);
    
    vec3 specular = vec3(0);
    
    vec3 H = normalize(V + SunDirection);
    vec3 F = fresnelSchlick(saturate(dot(H, V)), F0, roughness) * sunColor;
    
    if (mode == SpecularLighting)
    {   
        float NDF = DistributionGGX(H, roughness);   
        float G   = GeometrySmith(V, SunDirection, roughness);      
        vec3  numerator   = NDF * G * F; 
        float denominator = 4 * max(dot(Normal, V), 0.0) * NdotL;
        specular    = numerator / max(denominator * specularColor, 0.001); // prevent divide by zero for NdotV=0.0 or NdotL=0.0
    }
    
    vec3  kD    = (vec3(1.0) - F) * 1.0 - metalic;
    vec3  pbr   = (kD * tex / PI + (specular * specPower)) * (NdotL * sunIntensity);

    pbr *= ShadowCalculation();
    
    vec3 ambient = (ambientColor * (ambientStrength / 10)  + CalculateLightsPBR(V, F0, tex, roughness));

    outputColor = vec4((pbr + ambient) * ao, 1);
}