uniform sampler2D image;
uniform float iGlobalTime;

in vec2 uv;

float func(float x)
{
	return x + 0.6 * sin(2 * x);
}

void main () {
	// range [0..1]� -> [-1..1]�
    vec2 range11 = 2 * uv - 1;

    float radius = length(range11); // distance to center

	//distort angle
	float amplitude = 7.5;
	float frequency = 0.05;
	float startOffset = 0.5;
	range11 /= radius;
	range11 *= func(radius);

	//range [-1..1]� -> [0..1]�
	vec2 newUv = (range11 + 1) * 0.5;
	
	vec3 color = texture(image, newUv).rgb;  
    gl_FragColor = vec4(color, 1.0);
}
