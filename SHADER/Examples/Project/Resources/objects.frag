#version 430 core
uniform int id;
uniform sampler2D tex;
uniform sampler2D texShadowMap;
uniform mat4 camera;
uniform vec3 cameraPosition;
uniform vec4 ambientLightColor;
uniform vec4 moonLightColor;
uniform vec3 moonLightDirection;
uniform vec3 candleLightPosition;
uniform vec4 candleLightColor;
uniform vec3 spotLightPosition;
uniform vec3 spotLightDirection;
uniform float spotLightAngle;
uniform vec4 spotLightColor;

in vec2 uvs;
in vec3 pos;
in vec3 n;
in vec4 materialColor1;
in vec4 shadowLightPosition;

out vec4 color;

const float PI = 3.14159265359;

vec2 projectLongLat(vec3 direction) {
  float theta = atan(direction.x, -direction.z) + PI;
  float phi = acos(-direction.y);
  return vec2(theta / (2*PI), phi / PI);
}

float lambert(vec3 n, vec3 l)
{
	return max(0, dot(n, l));
}

float specular(vec3 n, vec3 l, vec3 v, float shininess)
{
	// only specular when no texture
	if (3 != id)
	{
		vec3 r = reflect(-l, n);
		return pow(max(0, dot(r, v)), shininess);
	}
	else
		return 0;
}

void main() 
{
	// environment
	if (1 == id)
	{
		vec3 normal = normalize(n);
		vec3 dir = normalize(pos); //for sky dome camera should stay fixed in the center
		if (id == 2)
		{
			dir = normalize(pos - cameraPosition); //for sky dome camera should stay fixed in the center
			dir = reflect(dir, normal);
		}
		if (id == 3)
		{
			dir = normalize(pos - cameraPosition); //for sky dome camera should stay fixed in the center
			dir = refract(dir, normal, 1.45);
		}
		color = texture(tex, projectLongLat(dir));
	}
	// objects
	if (2 == id || 3 == id)
	{
		
		
		bool dir_light = true;
		bool point_light = true;
		bool spot_light = false;
		
		vec4 moonLight = vec4(0.0);
		vec4 candleLight = vec4(0.0);
		vec4 spotLight = vec4(0.0);
		
		// general setup
		vec3 normal = normalize(n);
		vec3 v = normalize(cameraPosition - pos);
		//ambient lighting
		vec4 ambientLight = ambientLightColor * materialColor1;

		// moon light (directional light)
		if (dir_light)
		{
			moonLight = materialColor1 * moonLightColor * lambert(normal, -moonLightDirection);
		}

		// candle light (point light)
		if (point_light)
		{
			vec3 normal = normalize(n);
			vec3 v = normalize(cameraPosition - pos);
			vec3 l = normalize(candleLightPosition - pos);

			//point light
			candleLight = candleLightColor * (ambientLightColor + candleLightColor * lambert(n, l)) + candleLightColor * specular(n, l, v, 100);
		}

		//spot light
		if (spot_light)
		{
			vec3 spotLightl = normalize(spotLightPosition + pos);
			if(acos(dot(spotLightl, -spotLightDirection)) < spotLightAngle)
			{
				spotLight = materialColor1 * spotLightColor * lambert(normal, spotLightl) + spotLightColor * specular(normal, spotLightl, v, 100);
			}
		}

		//combine
		// objects without texture
		//if (2 == id)
			color = ambientLight + moonLight + candleLight + spotLight;
		
		/**
		// objects with texture
		if (3 == id)
			color = texture(tex, uvs) * (ambientLight + moonLight + candleLight + spotLight);
		**/
		
				// shadow calculation
		vec3 coord = shadowLightPosition.xyz / shadowLightPosition.w;
		float depth = texture(texShadowMap, coord.xy * .5 + 0.5).r;
		if (depth + 0.001 < coord.z)
			color *= 0.3;
	}
}