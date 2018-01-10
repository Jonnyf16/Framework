#version 430 core	
			
uniform mat4 camera;

in vec4 position;
in vec3 instancePosition;

out vec4 pos;

void main() 
{
	pos = camera * vec4(vec3(0.3 * position.xyz + instancePosition), position.w);
	gl_Position = pos;
}