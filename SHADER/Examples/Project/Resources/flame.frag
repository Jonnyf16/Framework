#version 430 core

uniform float iGlobalTime;
uniform float camElevation;
uniform float camAzimuth;
uniform float smokeState;
in vec3 wind_dir;

out vec4 fragColor;

const float PI = 3.14159265359;

vec2 hash( vec2 p )
{
	p = vec2( dot(p,vec2(127.1,311.7)),
			 dot(p,vec2(269.5,183.3)) );
	return -1.0 + 2.0*fract(sin(p)*43758.5453123);
}

float noise( in vec2 p )
{
	const float K1 = 0.366025404;
	const float K2 = 0.211324865;
	
	vec2 i = floor( p + (p.x+p.y)*K1 );
	
	vec2 a = p - i + (i.x+i.y)*K2;
	vec2 o = (a.x>a.y) ? vec2(1.0,0.0) : vec2(0.0,1.0);
	vec2 b = a - o + K2;
	vec2 c = a - 1.0 + 2.0*K2;
	
	vec3 h = max( 0.5-vec3(dot(a,a), dot(b,b), dot(c,c) ), 0.0 );
	
	vec3 n = h*h*h*h*vec3( dot(a,hash(i+0.0)), dot(b,hash(i+o)), dot(c,hash(i+1.0)));
	
	return dot( n, vec3(70.0) );
}

float fbm(vec2 uv)
{
	float f;
	mat2 m = mat2( 1.6,  1.2, -1.2,  1.6 );
	f  = 0.5000*noise( uv ); uv = m*uv;
	f += 0.2500*noise( uv ); uv = m*uv;
	f += 0.1250*noise( uv ); uv = m*uv;
	f += 0.0625*noise( uv ); uv = m*uv;
	f = 0.5 + 0.5*f;
	return f;
}

void main()
{
	vec2 flameCoord = gl_PointCoord;
    
    // set flame coordinates
    flameCoord.x -= 0.5;
	flameCoord.y -= 0.12;
	flameCoord.x *= 5.0;
	flameCoord.y *= 2.0;

	// wind
	float wind_x = clamp(wind_dir.x, -2, 2) * cos(camAzimuth * 0.00556 * PI);
	float wind_z = clamp(wind_dir.z, -2, 2) * cos((camAzimuth * 0.00556 - 0.5) * PI);

    // flame parameters
	float strength = 1.0 + clamp(wind_dir.x + wind_dir.z, 0.0, 1.0) + 2.0 * smokeState;
    float speed = 2.0 + clamp(abs(wind_dir.x) + abs(wind_dir.z), 0.0, 1.0) + 2.0 * smokeState;
    float disort_ver = wind_x + wind_z;
    float disort_hor = disort_ver + sign(-(disort_ver+.0001)) * sin(camElevation * 0.00556 * PI * .5) + sign(-(disort_ver+.0001)) * smokeState;

	// create horizonatal 'anti' movement to balance out horizontal distortion
	flameCoord.x -= disort_ver * .45;
    
    // creates background noise
	float fbm_ = fbm(strength * flameCoord - vec2(0, iGlobalTime * speed));
    
    // hight
    float height = (1.0 - flameCoord.y * 0.55) * 2.0;

    // shape
    float shape_ = max(0.0, (length(flameCoord * vec2(1.0 + flameCoord.y * 2.5, 0.7 * disort_ver) - flameCoord.y * disort_hor)) - (fbm_ * max(0.0, flameCoord.y + 0.25)));
	float shape = 1.0 - 20. * pow(shape_, 0.5);

    // create the flame...
	float flame = fbm_ * shape * height;
	flame = clamp(flame,0.,1.);
	    
    // clamp top
    if(flameCoord.y < -0.25)
        flame = 0.0;
    
    // clamp bottom
    if(flameCoord.y > 1.82)
        flame = 0.0;
    
    // color
	vec3 col = vec3(1.5 * flame, 1.5 * pow(flame, 3.0), pow(flame, 6.0));
	fragColor = vec4(vec3(col), 1.0);
}
