#version 430 core

in vec3 pos;

out vec4 color;

void main() 
{
    color = vec4(pos + 0..9, 1.0);
}
