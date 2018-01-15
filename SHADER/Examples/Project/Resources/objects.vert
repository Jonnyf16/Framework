#version 430 core	
			
uniform mat4 camera;
uniform mat4 light;
uniform int id;
uniform int noShadow;
uniform vec3 cameraPosition;

in vec4 position;
in vec3 instancePosition;
in vec4 materialColor;
in vec3 normal;
in vec2 uv;

out vec2 uvs;
out vec3 pos;
out vec3 n;
out vec4 materialColor1;
out vec4 shadowLightPosition;

void main() 
{
	n = normal;
	uvs = uv;
	materialColor1 = materialColor;
	pos = position.xyz;
	
	// environment
	if (1 == id)
	{
		gl_Position = camera * vec4(pos, 1.0);
	}
	
	
	// objects
	if (2 == id || 3 == id || 4 == id)
	{
		pos = 0.3 * position.xyz + instancePosition;
		shadowLightPosition = light * vec4(pos, position.w);
		gl_Position = camera * vec4(pos, position.w);
	}
}