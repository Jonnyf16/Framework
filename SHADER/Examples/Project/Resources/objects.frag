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

float lambert(vec3 n, vec3 l)
{
	return max(0, dot(n, l));
}

float specular(vec3 n, vec3 l, vec3 v, float shininess)
{
	//if(0 > dot(n, l)) return 0;
	vec3 r = reflect(-l, n);
	return pow(max(0, dot(r, v)), shininess);
}

void main() 
{
	bool dir_light = false;
	bool point_light = true;
	bool spot_light = false;
	
	vec4 light1 = vec4(0.0);
	vec4 light2 = vec4(0.0);
	vec4 light3 = vec4(0.0);
	
	// general setup
	vec3 normal = normalize(n);
	vec3 v = normalize(cameraPosition - pos);
	//ambient lighting
	vec4 ambient = ambientLightColor * materialColor;

	//directional light
	if (dir_light)
	{
		light1 = materialColor * light1Color * lambert(normal, -light1Direction);
				 + light1Color * specular(normal, -light1Direction, v, 100);
	}

	//point light
	if (point_light)
	{
		vec3 light2l = normalize(light2Position - pos);
		light2 = materialColor * light2Color * lambert(normal, light2l)
					+ light2Color * specular(normal, light2l, v, 100);
	}

	//spot light
	if (spot_light)
	{
		vec3 light3l = normalize(light3Position - pos);
		if(acos(dot(light3l, -light3Direction)) < light3Angle)
		{
			light3 = materialColor * light3Color * lambert(normal, light3l)
					+ light3Color * specular(normal, light3l, v, 100);
		}
	}

	//combine
	color = ambient	+ light1 + light2 + light3;
}