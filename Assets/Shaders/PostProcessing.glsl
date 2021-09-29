#version 440 core

out vec4 out_color;
in vec2 texCoords;

layout(location = 1) uniform float gamma = 2.2;
layout(location = 2) uniform float saturation;
layout(location = 3) uniform int mode;

uniform sampler2D texture0;
uniform sampler2D texture1; // ssao only has .r

vec3 aces(in vec3 x) {
  const float a = 2.51; const float b = 0.03;
  const float c = 2.43; const float d = 0.59;
  const float e = 0.14;
  return clamp((x * (a * x + b)) / (x * (c * x + d) + e), 0.0, 1.0);
}

vec3 tonemapFilmic(in vec3 x) {
  vec3 X = max(vec3(0.0), x - 0.004);
  vec3 result = (X * (6.2 * X + 0.5)) / (X * (6.2 * X + 1.7) + 0.06);
  return pow(result, vec3(2.2));
}

vec3 lottes(in vec3 x) {
  const vec3 a = vec3(1.6);      const vec3 d = vec3(0.977);
  const vec3 hdrMax = vec3(8.0); const vec3 midIn = vec3(0.18);
  const vec3 midOut = vec3(0.267);
  
  const vec3 b = (-pow(midIn, a) + pow(hdrMax, a) * midOut) /
                 ((pow(hdrMax, a * d) - pow(midIn, a * d)) * midOut);
  const vec3 c = (pow(hdrMax, a * d) * pow(midIn, a) - pow(hdrMax, a) * pow(midIn, a * d) * midOut) /
                 ((pow(hdrMax, a * d) - pow(midIn, a * d)) * midOut);
  
  return pow(x, a) / (pow(x, a * d) * b + c);
}

vec3 reinhard(in vec3 x) {
  return x / (1.0 + x);
}   

vec3 reinhard2(in vec3 x) {
  const float L_white = 4.0;
  return (x * (1.0 + x / (L_white * L_white))) / (1.0 + x);
}

vec3 uchimura(in vec3 x, in float P, in float a, in float m, in float l, in float c, in float b) {
  float l0 = ((P - m) * l) / a;
  float L0 = m - m / a;
  float L1 = m + (1.0 - m) / a;
  float S0 = m + l0;
  float S1 = m + a * l0;
  float C2 = (a * P) / (P - S1);
  float CP = -C2 / P;

  vec3 w0 = vec3(1.0 - smoothstep(0.0, m, x));
  vec3 w2 = vec3(step(m + l0, x));
  vec3 w1 = vec3(1.0 - w0 - w2);

  vec3 T = vec3(m * pow(x / m, vec3(c)) + b);
  vec3 S = vec3(P - (P - S1) * exp(CP * (x - S0)));
  vec3 L = vec3(m + a * (x - m));

  return T * w0 + L * w1 + S * w2;
}

vec3 uchimura(in vec3 x) {
  const float P = 1.0;  // max display brightness
  const float a = 1.0;  // contrast
  const float m = 0.22; // linear section start
  const float l = 0.4;  // linear section length
  const float c = 1.33; // black

  return uchimura(x, P, a, m, l, c, 0.0);
}

vec3 uncharted2Tonemap(in vec3 x) {
  const float A = 0.15; const float B = 0.50;
  const float C = 0.10; const float D = 0.20;
  const float E = 0.02; const float F = 0.30;
  const float W = 11.2;
  return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E / F;
}

vec3 uncharted2(in vec3 color) {
  const float W = 11.2;
  const float exposureBias = 2.0;
  vec3 curr = uncharted2Tonemap(exposureBias * color);
  vec3 whiteScale = 1.0 / uncharted2Tonemap(vec3(W));
  return curr * whiteScale;
}

vec3 unreal(in vec3 x) {
  return x / (x + 0.155) * 1.019;
}

float ColTone(in float x, in vec4 p) { 
    float z = pow(x, p.r); 
    return z / (pow(z, p.g)*p.b + p.a); 
}

// https://github.com/GPUOpen-LibrariesAndSDKs/Cauldron/blob/master/src/VK/shaders/tonemappers.glsl
vec3 AMDTonemapper(vec3 color)
{
    const float contrast = 2.0;        const float crosstalk = 4.0; 
    const float saturation = contrast; const float white = 1.0;
    const float crossSaturation = contrast * 16.0;
    // precomputed values performance efficent I mean tons of calculations
    const float b = 0.9994231; const float c = 0.14761868;
    const float EPS = 1e-6f;
    
    float peak = max(color.r, max(color.g, color.b));
    peak = max(EPS, peak);
    vec3 ratio = color / peak;
    peak = ColTone(peak, vec4(contrast, 1, b, c) );
    ratio = pow(abs(ratio), vec3(saturation / crossSaturation));
    ratio = mix(ratio, vec3(white), vec3(pow(peak, crosstalk)));
    ratio = pow(abs(ratio), vec3(crossSaturation));
    color = peak * ratio;
    return color;
}

vec3 DX11DSK(vec3 color)
{
    const float  MIDDLE_GRAY = 0.72;
    const float  LUM_WHITE = 1.5;

    // Tone mapping
    color.rgb *= MIDDLE_GRAY;
    color.rgb *= (1.0 + color/LUM_WHITE);
    color.rgb /= (1.0 + color);
    
    return color;
}

void Saturation(inout vec3 In)
{
    const vec3 saturate = vec3(0.2126729, 0.7151522, 0.0721750);
    float luma = dot(In, saturate);
    In =  luma.xxx + saturation.xxx * (In - luma.xxx);
}

void main()
{
	vec3 color = texture(texture0, texCoords).rgb;
    // color *= texture(texture1, texCoords).r; //ssao integration

    // tone mapping
    vec3 mapped;
    switch(mode)
    {
        case 0: mapped = aces(color);          break;
        case 1: mapped = tonemapFilmic(color); break;
        case 2: mapped = lottes(color);        break;
        case 3: mapped = reinhard(color);      break;
        case 4: mapped = reinhard2(color);     break;
        case 5: mapped = uchimura(color);       break;
        case 6: mapped = uncharted2(color);    break;
        case 7: mapped = unreal(color);        break;
        case 8: mapped = AMDTonemapper(color); break;
        case 9: mapped = DX11DSK(color);       break;
        case 10: mapped = color;                break;
    }
    Saturation(mapped);
    // gamma correction 
    mapped = pow(mapped, vec3(1.0 / gamma));
	out_color = vec4(mapped, 1);
}