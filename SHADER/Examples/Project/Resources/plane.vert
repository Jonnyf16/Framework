#version 430 core				

uniform mat4 camera;

in vec4 position;
in vec2 texCoord;

out vec2 texCoordV;

void main() 
{
	texCoordV = texCoord;
	gl_Position = camera * position;
}