// IndirectLight HasShadows
#version 400

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
uniform sampler2D specularTex; 
uniform sampler2D AOTex; 
uniform sampler2D roughnessTex; 
uniform sampler2D shadowMap; // dontUse

uniform float bias; // dontUse
uniform float AOIntensity = 0.2;
uniform vec4 color = vec4(1,1,1,1);

// point light
struct Light
{
    float intensity; // = 1;     // outerCutOff = 0.91
    int  type      ;
    vec3 color     ;
    vec3 position  ;
    vec3 direction ; // for spot lights
};

const int maxLightDistance = 100;
const int LightSize = 10;
uniform Light pointLights[LightSize];
uniform int lightCount; // dontUse

// pbr

// specular
uniform vec3 specularColor = vec3(1);
uniform float specPower = 1;
uniform int mode; // enum Blinn Beckmann GGX

uniform float roughnessPow;
uniform float metalic;

const float PI = 3.14159265359;

// UniformEnd
// a lot of functions in here https://gist.github.com/galek/53557375251e1a942dfa
float Distance(in vec3 a, in vec3 b) {
    float diff_x = a.x - b.x;
    float diff_y = a.y - b.y;
    float diff_z = a.z - b.z;
    return sqrt(diff_x * diff_x + diff_y * diff_y + diff_z * diff_z);
}

float saturate(in float x) {
  return max(0, min(1, x));
}

// compute fresnel specular factor for given base specular and product
vec3 fresnelSchlick(in float cosTheta, in vec3 F0, in float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
}

// product could be NdV or VdH depending on used technique
vec3 fresnel_factor(in vec3 f0, in float product)
{
    return mix(f0, vec3(1.0 ), pow(1.01 - product, 5.0));
}

float G_schlick(in float roughness, in float NdV, in float NdL)
{
    float k = roughness * roughness * 0.5;
    float V = NdV * (1.0 - k) + k;
    float L = NdL * (1.0 - k) + k;
    return 0.25 / (V * L);
}

// following functions are copies of UE4
// for computing cook-torrance specular lighting terms

float D_blinn(in float roughness, in float NdH)
{
    float m = roughness * roughness;
    float m2 = m * m;
    float n = 2.0 / m2 - 2.0;
    return (n + 2.0) / (2.0 * PI) * pow(NdH, n);
}

float D_beckmann(in float roughness, in float NdH)
{
    float m = roughness * roughness;
    float m2 = m * m;
    float NdH2 = NdH * NdH;
    return exp((NdH2 - 1.0) / (m2 * NdH2)) / (PI * m2 * NdH2 * NdH2);
}

float D_GGX(in float roughness, in float NdH)
{
    float m = roughness * roughness;
    float m2 = m * m;
    float d = (NdH * m2 - NdH) * NdH + 1.0;
    return m2 / (PI * d * d);
}

// cook-torrance specular calculation                      
vec3 cooktorrance_specular(in float NdL, in float NdV, in float NdH, in vec3 specular, in float roughness)
{
    // float D = D_blinn(roughness, NdH);
    // float D = D_beckmann(roughness, NdH);
    float D = D_GGX(roughness, NdH);

    float G = G_schlick(roughness, NdV, NdL);

    float rim = mix(1.0 - roughness * metalic * 0.9, 1.0, NdV);

    return (1.0 / rim) * specular * G * D;
}

float CalculateSpotLight(in Light light, in vec3 surfaceToLight)
{
     float theta = dot(normalize(surfaceToLight) , light.direction) ;
     
     if (theta * 180 > 30) // for now its 30 we can add angle property in light struct 
     {
        return theta / length(surfaceToLight);
     }
     return 0;
}

float CalculatePointLight(in float intensity, in vec3 surfaceToLight) 
{
    float theta = dot(Normal, normalize(surfaceToLight));
    
    if (theta > 0) { // light in correct direction
        return (theta / length(surfaceToLight)) * intensity;
    }
    return 0;
}

vec3 CalculateIndirectLights(
    in float ao        ,
    in float roughness ,
    in float specValue ,
    in vec3 F0         )
{
    vec3 result = vec3(0);

    for (int i = 0; i < lightCount; i++)
    {
        Light light = pointLights[i];
        
        vec3 L = normalize(light.position - FragPos);
        vec3 N = Normal;
        vec3 V = normalize(viewpos - FragPos);
        vec3 H = normalize(V + L);

        float NdL = max(0.000, dot(N, L));
        float NdV = max(0.001, dot(N, V));
        float NdH = max(0.001, dot(N, H));
        float HdV = max(0.001, dot(H, V));
        
        vec3 specfresnel = fresnel_factor(F0, HdV);
        vec3 specular = cooktorrance_specular(NdL, NdV, NdH, specfresnel, roughness);
        
        // decreasing specular value
        specular *= specValue * NdL * ao;
        
        vec3 surfaceToLight = light.position - FragPos;
        float attenuation = 0;
        
        if (light.type == 1) { // point light
            attenuation = CalculatePointLight(light.intensity, surfaceToLight);
        }
        else if (light.type == 2) { // spotLight
            attenuation = CalculatePointLight(light.intensity, surfaceToLight);
        }

        result = specular * light.color * attenuation;
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
    // float shadow = currentDepth - bias > closestDepth  ? .44: 1.0;  
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    float realBias = max((bias * 10) * (1.0 - dot(Normal, vec3(0, sin(sunAngle), cos(sunAngle)))), bias);  
    for(int x = -1; x <= 1; ++x)
    {
        for(int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r; 
            shadow += currentDepth - realBias > pcfDepth ? .44 : 1;        
        }    
    }
    shadow /= 9.0;

    return shadow;
}

void main()
{
    // sun direction
    vec3 L = -vec3(0, sin(sunAngle), cos(sunAngle)); // wi
    
    vec4 albedo = texture(albedoTex, texCoord);

    if (albedo.a == 0) discard; // alpha clipping
    
    vec3 tex = albedo.xyz * color.xyz;

    float ao        = 1 - max(.2 , min(1, texture(AOTex, texCoord).r * AOIntensity));
    float roughness = texture(roughnessTex, texCoord).r * roughnessPow;
    float specValue = texture(specularTex, texCoord).r * max(specPower, 0);

    vec3 N = Normal;
    vec3 V = normalize(viewpos - FragPos);
    vec3 H = normalize(V + L);

    float NdL = max(0.015, dot(N, L));
    float NdV = max(0.015, dot(N, V));
    float NdH = max(0.015, dot(N, H));
    float HdV = max(0.015, dot(H, V));

    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, tex, metalic);

    vec3 specfresnel = fresnel_factor(F0, HdV);
    vec3 specular = cooktorrance_specular(NdL, NdV, NdH, specfresnel, roughness) * specValue * NdL * ao;

    vec3 ambient = ambientColor * (ambientStrength / 10);

    vec3 diffuse = tex * NdL * sunColor * ao;
    vec3 indirectLights = CalculateIndirectLights(ao, roughness, specValue, F0);

    outputColor = vec4((diffuse + specular + ambient + indirectLights) * ShadowCalculation(), 1);
}


