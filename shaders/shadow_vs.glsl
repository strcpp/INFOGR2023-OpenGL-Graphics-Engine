// Only used in Modern OpenGL

#version 330
 
// shader input
in vec3 vPosition;		// vertex position in normalized device coordinates

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 lightMatrix;

// vertex shader
void main()
{
	// forward vertex position; will be interpolated for each fragment
	// no transformation needed because the user already provided NDC
	gl_Position = lightMatrix * model * vec4(vPosition, 1.0);

}