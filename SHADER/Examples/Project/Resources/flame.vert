#version 430 core				

uniform vec2 iResolution;
uniform mat4 camera;
in vec4 instance_position;
in vec3 position;

void main() 
{
	vec4 pos = camera * instance_position;
	gl_PointSize = (1 - pos.z / pos.w) * 1000 * (iResolution.y / 1920);
	gl_Position = pos;
}
