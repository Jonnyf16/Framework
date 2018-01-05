#version 430 core

out vec4 color;

/**
void main() 
{
	color = vec4(0.1, 0.2, 0.5, 1.0);
}
**/

in vec2 texCoordV;
uniform vec2 iResolution;
uniform sampler2D planeTex;

void main()
{
	//vec2 uv = gl_FragCoord.xy / iResolution;
	
	//lookup color in texture at position uv
	//vec3 tex_col = texture(planeTex, texCoordV).rgb;
	color = vec4(texCoordV.xy, 0.0, 1.0);
}