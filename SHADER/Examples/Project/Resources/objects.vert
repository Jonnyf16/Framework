#version 430 core				

uniform mat4 camera;
uniform vec3 cameraPosition;
uniform vec4 ambientLightColor;
uniform vec4 materialColor;
uniform vec4 light1Color;
uniform vec3 light1Direction;

in vec3 position;
in vec3 normal;
in vec3 instancePosition;

out vec3 pos;
out vec3 n;
out vec4 geroud_color;

void main() 
{
	pos = position;
	n = normal;

	vec3 pos = 0.3 * position + instancePosition;
	gl_Position = camera * vec4(pos, 1.0);
}