/*
 * Copyright © 2019 Robbyxp1 @ github.com
 * Part of the EDDiscovery Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */
#version 450 core

vec3 random3A(vec3 st);	// from random

// From https://www.shadertoy.com/view/Xsl3Dl 

float gradientnoise( in vec3 p )
{
    vec3 i = floor( p );
    vec3 f = fract( p );
	
	vec3 u = f*f*(3.0-2.0*f);

    return mix( mix( mix( dot( random3A( i + vec3(0.0,0.0,0.0) ), f - vec3(0.0,0.0,0.0) ), 
                          dot( random3A( i + vec3(1.0,0.0,0.0) ), f - vec3(1.0,0.0,0.0) ), u.x),
                     mix( dot( random3A( i + vec3(0.0,1.0,0.0) ), f - vec3(0.0,1.0,0.0) ), 
                          dot( random3A( i + vec3(1.0,1.0,0.0) ), f - vec3(1.0,1.0,0.0) ), u.x), u.y),

                mix( mix( dot( random3A( i + vec3(0.0,0.0,1.0) ), f - vec3(0.0,0.0,1.0) ), 
                          dot( random3A( i + vec3(1.0,0.0,1.0) ), f - vec3(1.0,0.0,1.0) ), u.x),
                     mix( dot( random3A( i + vec3(0.0,1.0,1.0) ), f - vec3(0.0,1.0,1.0) ), 
                          dot( random3A( i + vec3(1.0,1.0,1.0) ), f - vec3(1.0,1.0,1.0) ), u.x), u.y), u.z );
}

float gradientnoiseT1( in vec3 p )	// same as gradientnoise above but readable.
{
    vec3 i = floor( p );
    vec3 f = fract( p );
	
	vec3 u = f*f*(3.0-2.0*f);

	float dp1 = dot( random3A( i + vec3(0.0,0.0,0.0) ), f - vec3(0.0,0.0,0.0) );
	float dp2 = dot( random3A( i + vec3(1.0,0.0,0.0) ), f - vec3(1.0,0.0,0.0) );
	float dp3 = dot( random3A( i + vec3(0.0,1.0,0.0) ), f - vec3(0.0,1.0,0.0) );
	float dp4 = dot( random3A( i + vec3(1.0,1.0,0.0) ), f - vec3(1.0,1.0,0.0) );
	float dp5 = dot( random3A( i + vec3(0.0,0.0,1.0) ), f - vec3(0.0,0.0,1.0) );
	float dp6 = dot( random3A( i + vec3(1.0,0.0,1.0) ), f - vec3(1.0,0.0,1.0) );
	float dp7 = dot( random3A( i + vec3(0.0,1.0,1.0) ), f - vec3(0.0,1.0,1.0) );
	float dp8 = dot( random3A( i + vec3(1.0,1.0,1.0) ), f - vec3(1.0,1.0,1.0) );

	float dp1234 = mix( mix( dp1, dp2, u.x), mix( dp3 , dp4 , u.x), u.y);
	float dp5678 = mix( mix( dp5, dp6, u.x), mix( dp7,  dp8, u.x), u.y);

    return mix( dp1234, dp5678 , u.z );
}

