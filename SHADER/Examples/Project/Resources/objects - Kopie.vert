#version 430 core				

uniform int id;
uniform mat4 camera;
uniform vec3 cameraPosition;
uniform vec4 ambientLightColor;
uniform vec4 light1Color;
uniform vec3 light1Direction;

in vec2 uv;
in vec3 position;
in vec3 normal;
in vec3 instancePosition;
in vec4 materialColor;

out vec2 uvs;
out vec3 pos;
out vec3 n;
out vec4 materialColor1;

void main() 
{
	pos = position;
	n = normal;
	materialColor1 = materialColor;
	uvs = uv;
	
	// environment
	if (1 == id)
	{
		gl_Position = camera * vec4(pos, 1.0);
	}

	// objects
	if (2 == id || 3 == id)
	{
		pos = 0.3 * position + instancePosition;
		gl_Position = camera * vec4(pos, 1.0);
	}
}