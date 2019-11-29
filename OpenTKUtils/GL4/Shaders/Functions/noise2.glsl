#version 450 core

float random2A(vec2 st);	// from random
vec2 random2B(vec2 st);

// Gradient Noise by Inigo Quilez - iq/2013. MIT
// https://www.shadertoy.com/view/XdXGW8
float gradientnoise(vec2 st) 
{
    vec2 i = floor(st);
    vec2 f = fract(st);

    vec2 u = f*f*(3.0-2.0*f);

    return mix( mix( dot( random2B(i + vec2(0.0,0.0) ), f - vec2(0.0,0.0) ),
                     dot( random2B(i + vec2(1.0,0.0) ), f - vec2(1.0,0.0) ), u.x),
                mix( dot( random2B(i + vec2(0.0,1.0) ), f - vec2(0.0,1.0) ),
                     dot( random2B(i + vec2(1.0,1.0) ), f - vec2(1.0,1.0) ), u.x), u.y);
}

float gradientnoise4octave(vec2 p)	// 4 octave mix
{
     mat2 m = mat2( 1.6,  1.2, -1.2,  1.6 );
	 float f  = 0.5000*gradientnoise( p ); 
	 p = m*p;
	 f += 0.2500*gradientnoise( p ); 
	 p = m*p; 
	 f+= 0.1250*gradientnoise( p ); 
	 p = m*p; 
	 return f + 0.0625*gradientnoise( p ); 
}

// inspired from book of shaders

float noiseA(vec2 p)
{
    vec2 i = floor(p);
    vec2 f = fract(p);
        
    float a = random2A(i);                    //  C  D
    float b = random2A(i + vec2(1,0));        //  A  B
    float c = random2A(i + vec2(0,1));
    float d = random2A(i + vec2(1,1));

    vec2 u = f*f*(3.0-2.0*f);

    float bot = mix(a,b,u.x);
    float top = mix(c,d,u.x);
    return mix(bot,top,u.y);
}

// from https://www.shadertoy.com/view/Msf3WH shadertoy. Not the perlin triangluar versrion, a simpler version

float simplexnoiseA( in vec2 p )
{
    const float K1 = 0.366025404; // (sqrt(3)-1)/2;
    const float K2 = 0.211324865; // (3-sqrt(3))/6;

	vec2  i = floor( p + (p.x+p.y)*K1 );
    vec2  a = p - i + (i.x+i.y)*K2;
    float m = step(a.y,a.x); 
    vec2  o = vec2(m,1.0-m);
    vec2  b = a - o + K2;
	vec2  c = a - 1.0 + 2.0*K2;
    vec3  h = max( 0.5-vec3(dot(a,a), dot(b,b), dot(c,c) ), 0.0 );
	vec3  n = h*h*h*h*vec3( dot(a,random2B(i+0.0)), dot(b,random2B(i+o)), dot(c,random2B(i+1.0)));
    return dot( n, vec3(70.0) );
}

float simplexnoiseA4octave(vec2 p )
{
    mat2 m = mat2( 1.6,  1.2, -1.2,  1.6 );
	float f  = 0.5000*simplexnoiseA( p ); 
	p = m * p;
	f  += 0.2500*simplexnoiseA( p ); 
	p = m * p;
	f  = 0.1250*simplexnoiseA( p ); 
	p = m * p;
	return f + 0.0625*simplexnoiseA( p ); 
}

// from https://thebookofshaders.com/edit.php#11/lava-lamp.frag Another simplex noise function
vec2 mod289(vec2 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
vec3 mod289(vec3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
vec3 permute(vec3 x) { return mod289(((x*34.0)+1.0)*x); }

float simplexnoiseB(vec2 v) {
    const vec4 C = vec4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
                        0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
                        -0.577350269189626,  // -1.0 + 2.0 * C.x
                        0.024390243902439); // 1.0 / 41.0
    vec2 i  = floor(v + dot(v, C.yy) );
    vec2 x0 = v -   i + dot(i, C.xx);
    vec2 i1;
    i1 = (x0.x > x0.y) ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
    vec4 x12 = x0.xyxy + C.xxzz;
    x12.xy -= i1;
    i = mod289(i); // Avoid truncation effects in permutation
    vec3 p = permute( permute( i.y + vec3(0.0, i1.y, 1.0 ))
        + i.x + vec3(0.0, i1.x, 1.0 ));

    vec3 m = max(0.5 - vec3(dot(x0,x0), dot(x12.xy,x12.xy), dot(x12.zw,x12.zw)), 0.0);
    m = m*m ;
    m = m*m ;
    vec3 x = 2.0 * fract(p * C.www) - 1.0;
    vec3 h = abs(x) - 0.5;
    vec3 ox = floor(x + 0.5);
    vec3 a0 = x - ox;
    m *= 1.79284291400159 - 0.85373472095314 * ( a0*a0 + h*h );
    vec3 g;
    g.x  = a0.x  * x0.x  + h.x  * x0.y;
    g.yz = a0.yz * x12.xz + h.yz * x12.yw;
    return 130.0 * dot(m, g);
}