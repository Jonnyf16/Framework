#version 430 core
uniform sampler2D texShadowMap;

uniform vec3 ambient;

in vec4 shadowLightPosition;
in vec2 uvs;
in vec3 n;

out vec4 color;

void main() 
{
	vec3 coord = shadowLightPosition.xyz / shadowLightPosition.w;
	float depth = texture(texShadowMap, coord.xy * .5 + 0.5).r;
	color = depth + 0.001 > coord.z ? vec4(1) : vec4(0);
}