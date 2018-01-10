#version 430 core	
			
uniform mat4 camera;

in vec3 position;
in vec3 instancePosition;

out vec3 pos;

void main() 
{
	vec3 pos = 0.3 * position + instancePosition;
	gl_Position = camera * vec4(pos, 1.0);
}