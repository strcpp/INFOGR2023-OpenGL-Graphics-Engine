#version 330
 
struct Light {
  vec3 position;
  vec3 Ia; 
  vec3 Id; 
  vec3 Is;
};

// shader input
in vec2 uv;			// interpolated texture coordinates
in vec4 normal;			// interpolated normal
in vec3 fragPos;
in vec4 fragPosLight;
in mat3 TBN;

uniform sampler2D pixels;	// texture sampler
uniform sampler2DShadow shadowMap; // shadow map sampler
uniform samplerCube skybox;	// texture sampler
uniform sampler2D normalMap;	// texture sampler


uniform vec3 camPos;
uniform Light light;
uniform bool mirror;
uniform bool useNormalMap;

// shader output
out vec4 outputColor;

float calculateShadow(vec3 normal, vec3 lightDir) {
    float shadow = textureProj(shadowMap, fragPosLight);
    return shadow;
}

vec3 calculateLighting(vec3 Normal) {
    vec3 ambient = light.Ia;
    
    vec3 dir = normalize(light.position - fragPos);
    vec3 diffuse = light.Id * max(0, dot(dir, Normal));
    
    vec3 viewDir = normalize(camPos - fragPos);
    vec3 reflectDir = reflect(-dir, Normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec3 specular = light.Is * spec;
    
    // Attenuation
    float constant = 1.0;
    float linear = 0.09;
    float quadratic = 0.032;
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (constant + linear * distance + quadratic * (distance * distance));
    
    float shadow = calculateShadow(Normal, dir);
    return (ambient + (shadow) * (diffuse + specular));
}

// fragment shader
void main()
{
    vec3 Normal = normalize(normal.xyz);    
    if(useNormalMap) {
        Normal = texture(normalMap, uv).rgb;
        Normal = Normal * 2.0 - 1.0;   
        Normal = normalize(TBN * Normal); 
    }

    if(!mirror) {
        outputColor = texture( pixels, uv ) * vec4(calculateLighting(Normal), 1);
    } else {
        vec3 I = normalize(fragPos - camPos);
        vec3 R = reflect(I, Normal);
        outputColor = vec4(texture(skybox, R).rgb, 1.0);
    }
}