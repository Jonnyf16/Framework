#version 430 core


in vec4 pos;

out vec4 color;

void main() 
{
	color = vec4(pos.z / pos.w);
}