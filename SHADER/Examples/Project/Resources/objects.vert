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

	// Geroud Shading
	vec4 color_ambient = ambientLightColor * materialColor;
	vec4 color_diffuse = materialColor * light1Color * dot(normal, -light1Direction);
	vec3 r = normalize(2 * dot(normal, -light1Direction) * normal - (-light1Direction));
	vec3 cameraDirection = normalize(pos - cameraPosition);
	vec4 color_specular = materialColor * light1Color * max(0, pow(dot(r, -cameraDirection), 64));
	
	geroud_color = color_ambient + color_diffuse + color_specular;
	vec3 pos = 0.3 * position + instancePosition;
	gl_Position = camera * vec4(pos, 1.0);
	

}