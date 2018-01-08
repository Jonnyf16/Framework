#version 430 core				

uniform float iGlobalTime;
in vec2 in_position;
in vec2 in_velocity;

void main() 
{
	// ToDo: get PointSize right
	gl_PointSize = 512.0;
	vec2 newPos = in_position;
	gl_Position = vec4(newPos, 0.0, 1.0);
}