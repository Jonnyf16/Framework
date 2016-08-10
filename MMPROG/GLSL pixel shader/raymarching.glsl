uniform vec2 iResolution;
uniform float iGlobalTime;
uniform float cameraZ;

const float epsilon = 0.0001;
const int maxSteps = 128;

#include "libs/distanceFields.glsl"

float distScene(vec3 point)
{
	point.y += sin(point.z - iGlobalTime * 6.0) * cos(point.x - iGlobalTime) * .25; //waves!
	float distPlane = plane(point, vec3(0.0, 1.0, 0.0), -0.5);
	float distBox = box(point, vec3(0.0, -1.4, 0.0), vec3(0.6, 0.1, 0.3));
	point = coordinateRep(point, vec3(1.0, 1.0, 1.0));
	float distTorus = torus(point, vec2(0.3, 0.06));
	float distSphere = sphere(point, vec3(0.0, 0.0, 0.0), 0.2);
	return min(distPlane, distSphere);
}

//by numerical gradient
vec3 getNormal(vec3 point)
{
	float d = epsilon;
	//get points a little bit to each side of the point
	vec3 right = point + vec3(d, 0.0, 0.0);
	vec3 left = point + vec3(-d, 0.0, 0.0);
	vec3 up = point + vec3(0.0, d, 0.0);
	vec3 down = point + vec3(0.0, -d, 0.0);
	vec3 behind = point + vec3(0.0, 0.0, d);
	vec3 before = point + vec3(0.0, 0.0, -d);
	//calc difference of distance function values == numerical gradient
	vec3 gradient = vec3(distScene(right) - distScene(left),
		distScene(up) - distScene(down),
		distScene(behind) - distScene(before));
	return normalize(gradient);
}

void main()
{
	float fov = 80.0;
	float tanFov = tan(fov / 2.0 * 3.14159 / 180.0) / iResolution.x;
	vec2 p = tanFov * (gl_FragCoord.xy * 2.0 - iResolution.xy);

	vec3 camP = vec3(0.0, 0.0, cameraZ);
	vec3 camDir = normalize(vec3(p.x, p.y, 1.0));
	
	vec3 point = camP; 	
	bool objectHit = false;
	float t = 0.0;
    for(int steps = 0; steps < maxSteps; ++steps)
    {
        float dist = distScene(point);
        if(epsilon > dist)
        {
			objectHit = true;
            break;
        }
        t += dist;
        point = camP + t * camDir;
    }
	vec3 color = vec3(0.0, 0.0, 0.0);
	if(objectHit)
	{
		vec3 lightDir = normalize(vec3(cos(iGlobalTime), 1.0, sin(iGlobalTime)));
		vec3 normal = getNormal(point);
		float lambert = max(0.2 ,dot(normal, lightDir));
		color = lambert * vec3(1.0);
	}
	//fog
	float tmax = 10.0;
	float factor = t/tmax;
	// factor = clamp(factor, 0.0, 1.0);
	color = mix(color, vec3(1.0, 0.8, 0.1), factor);
	
	gl_FragColor = vec4(color, 1.0);
}


