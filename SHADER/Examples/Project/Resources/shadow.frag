#version 430 core


in vec3 pos;

out vec4 color;

void main() 
{
	color = vec3(pos.z);
}