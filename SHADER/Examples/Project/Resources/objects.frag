#version 430 core

uniform mat4 camera;
uniform vec3 cameraPosition;
uniform vec4 ambientLightColor;
uniform vec4 materialColor;
uniform vec4 light1Color;
uniform vec3 light1Direction;
uniform vec3 light2Position;
uniform vec4 light2Color;
uniform vec3 light3Position;
uniform vec3 light3Direction;
uniform float light3Angle;
uniform vec4 light3Color;

in vec3 pos;
in vec3 n;
in vec4 geroud_color;

out vec4 color;

void main() 
{
	vec3 normal = normalize(n);
	vec3 cameraDirection = normalize(pos - cameraPosition);
	vec4 color_ambient = vec4(0.0);
	vec4 color_diffuse = vec4(0.0);
	vec4 color_specular = vec4(0.0);
	vec3 r;
	
	
	// Phong shading because lighting calculation is inside fragment shader
	// directional light
	color_ambient = ambientLightColor * materialColor;
	color_diffuse = materialColor * light1Color * dot(normal, -light1Direction);
	r = normalize(2 * dot(normal, -light1Direction) * normal - (-light1Direction));
	color_specular = materialColor * light1Color * max(0, pow(dot(r, -cameraDirection), 64));
	
	// point light
	vec3 pointlightDirection = normalize(pos -light2Position);
	color_ambient += ambientLightColor * materialColor;
	color_diffuse += materialColor * light2Color * dot(normal, -pointlightDirection);
	r = normalize(2 * dot(normal, -pointlightDirection) * normal - (-pointlightDirection));
	color_specular += materialColor * light2Color * max(0, pow(dot(r, -cameraDirection), 64));
	
	
	// spot light
	vec3 spotlightDirection = normalize(pos -light3Position);
	// check if point is inside light cone
	if (acos(dot(-spotlightDirection, -light3Direction)) < light3Angle)
	{
		color_ambient += ambientLightColor * materialColor;
		color_diffuse += materialColor * light3Color * dot(normal, -spotlightDirection);
		r = normalize(2 * dot(normal, -spotlightDirection) * normal - (-spotlightDirection));
		color_specular += materialColor * light3Color * max(0, pow(dot(r, -cameraDirection), 64));
	}
	
	vec4 phong_color = color_ambient + color_diffuse + color_specular;
	
	color =  phong_color;
	//color = geroud_color;
}