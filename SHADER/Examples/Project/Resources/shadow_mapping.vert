#version 430 core	
			
uniform mat4 camera;
uniform mat4 light;

in vec3 position;
in vec3 instancePosition;
in vec3 normal;
in vec2 uv;

out vec4 shadowLightPosition;
out vec2 uvs;
out vec3 n;

void main() 
{
	shadowLightPosition = light * position;
	n = normal;
	uvs = uv;
	vec3 pos = 0.3 * position + instancePosition;
	gl_Position = camera * position;
}