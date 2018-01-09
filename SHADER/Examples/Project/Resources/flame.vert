#version 430 core				

uniform vec2 iResolution;
uniform mat4 camera;
in vec4 instance_position;
in vec3 position;

out vec2 uvs;

void main() 
{
	vec4 pos = camera * vec4(position, 1.0) + instance_position;
	gl_PointSize = (1 - pos.z / pos.w) * 1000;
	gl_Position = pos;
}