#version 450 core

// one dimensional 0-1.0
float random(float r) 
{
    return fract(sin(r)* 9243758.5453123);
}

// from book of shaders
vec2 random2B(vec2 st)
{
    st = vec2( dot(st,vec2(127.1,311.7)),
              dot(st,vec2(269.5,183.3)) );
    return -1.0 + 2.0*fract(sin(st)*43758.5453123);
}

// two dimensional random number between 0-1.
float random2A(vec2 st) 
{
    return fract(sin(dot(st.xy, vec2(12.9898,78.233)))* 43758.5453123);
}

vec3 random3A( vec3 p ) 
{
	p = vec3( dot(p,vec3(127.1,311.7, 74.7)),
			  dot(p,vec3(269.5,183.3,246.1)),
			  dot(p,vec3(113.5,271.9,124.6)));

	return -1.0 + 2.0*fract(sin(p)*43758.5453123);
}