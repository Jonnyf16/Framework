#version 430 core	
			
uniform mat4 camera;

in vec3 position;
in vec3 instancePosition;

out vec4 pos;

void main() 
{
	vec4 pos = vec4(vec3(0.3 * position + instancePosition), 1.0);
	gl_Position = camera * pos;
}