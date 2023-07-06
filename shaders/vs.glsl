#version 330
 
// shader input
in vec3 vPosition;		// untransformed vertex position
in vec3 vNormal;		// untransformed vertex normal
in vec2 vUV;			// vertex uv coordinate
in vec3 vTangent;			// vertex uv coordinate
in vec3 vBitangent;			// vertex uv coordinate


// shader output
out vec4 normal;		// transformed vertex normal
out vec2 uv;				
out vec3 fragPos;
out vec4 fragPosLight;
out mat3 TBN;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 lightMatrix;



mat4 m_shadow_bias = mat4(
    0.5, 0.0, 0.0, 0.0,
    0.0, 0.5, 0.0, 0.0,
    0.0, 0.0, 0.5, 0.0,
    0.5, 0.5, 0.5, 1.0
);

// vertex shader
void main()
{

	normal = vec4(mat3(transpose(inverse(model))) * vNormal, 1.0);
	vec3 T = normalize(vec3(model * vec4(vTangent,   0.0)));
	vec3 B = normalize(vec3(model * vec4(vBitangent, 0.0)));
	vec3 N = normalize(vec3(model * vec4(vNormal,    0.0)));
	TBN = mat3(T, B, N);

	fragPos = vec3(model * vec4(vPosition, 1.0)); 
	fragPosLight = m_shadow_bias * lightMatrix * model * vec4(vPosition, 1.0);
	fragPosLight.z -= 0.005;

	// transform vertex using supplied matrix
	gl_Position = projection * view * model * vec4(vPosition, 1.0);

	uv = vUV;
}