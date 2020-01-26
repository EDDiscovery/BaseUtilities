/*
 * Copyright 2019 Robbyxp1 @ github.com
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
 
// volumetric geo shader. 
// Inputs are two vertex's describing point 0 and 6 in the volumetric box.

#version 450 core

layout(std140, binding=0) uniform MatrixCalc
{
    mat4 ProjectionModelMatrix;
    mat4 ProjectionMatrix;		
    mat4 ModelMatrix;
    vec4 TargetPosition;		// vertex position, before ModelMatrix
    vec4 EyePosition;			// vertex position, before ModelMatrix
    float EyeDistance;			// between eye and target
	mat4 ScreenMatrix;			// for co-ordinate transforms between screen coords and display coords

} mc;

layout (lines) in;              // get two vertexes discribing p0 and p6 in
layout (triangle_strip) out;
layout (max_vertices=32) out;

in int instance[];

out vec4 vs_color;

layout (binding = 5, std430) buffer DataBack
{
    vec4[] databack;
};

layout (binding = 6, offset=0) uniform atomic_uint atomiccounter;

void main(void)
{
    // first compute the vertex points, clockwise from bottom left, front face: p0,p1,p2,p3.   backface: p4,p5,p6,p7
	// each must be done invididually to apply the modelmatrix to it
    vec4 p[8];
    p[0] = mc.ModelMatrix * gl_in[0].gl_Position;
    p[6] = mc.ModelMatrix * gl_in[1].gl_Position;
    p[1] = mc.ModelMatrix * vec4(gl_in[0].gl_Position.x,gl_in[1].gl_Position.y,gl_in[0].gl_Position.z,1);
    p[2] = mc.ModelMatrix * vec4(gl_in[1].gl_Position.x,gl_in[1].gl_Position.y,gl_in[0].gl_Position.z,1);
    p[3] = mc.ModelMatrix * vec4(gl_in[1].gl_Position.x,gl_in[0].gl_Position.y,gl_in[0].gl_Position.z,1);

    p[4] = mc.ModelMatrix * vec4(gl_in[0].gl_Position.x,gl_in[0].gl_Position.y,gl_in[1].gl_Position.z,1);
    p[5] = mc.ModelMatrix * vec4(gl_in[0].gl_Position.x,gl_in[1].gl_Position.y,gl_in[1].gl_Position.z,1);
    p[7] = mc.ModelMatrix * vec4(gl_in[1].gl_Position.x,gl_in[0].gl_Position.y,gl_in[1].gl_Position.z,1);

	// Find the min z and max z
    int maxz=0,minz=0;
    float maxzv = -10000000,minzv = +10000000;
    int i;
    for ( i = 0 ; i < 8 ; i++ )
    {
        if (p[i].z < minzv)
        {
            minzv = p[i].z;
            minz = i;
        }
        if (p[i].z > maxzv)
        {
            maxzv = p[i].z;
            maxz = i;
        }
    }

	int instanceid = instance[0];
	// find the z point
	float zdist =p[maxz].z-p[minz].z;
	float z = p[minz].z + zdist * (0.1+instanceid * 0.1);		// first is painted at the back, then forward..

	databack[0] = vec4(minz,maxz,zdist,0);

	float intercept[6];		// holds floating intercept
	vec4 interceptpoints[6];	// holds model view points of intercept
	int interceptcount = 0;		// count
	vec4 average = vec4(0,0,z,1);// average of found intercepts

	int i1lookup[12] = {0,1,3,0, 4,5,7,4, 0,1,2,3};	// comparision index to check for ic=0 to 12
	int i2lookup[12] = {1,2,2,3, 5,6,6,7, 4,5,6,7};	
	int ic = 0;
    for ( ic = 0 ; ic < 12 ; ic++ )
    {
		int i = i1lookup[ic];				// find the indexes to compare.
		int i2 = i2lookup[ic];
		float interceptv = (z - p[i].z) / (p[i2].z - p[i].z);
		if ( interceptv >= 0 && interceptv <=1)	// we have an intercept
		{
			intercept[interceptcount] = interceptv;		// they are all on the same z plane (thats the point!) interpolate the x/y points
			vec4 pos = vec4( p[i].x + (p[i2].x-p[i].x) * interceptv , p[i].y + (p[i2].y-p[i].y) * interceptv, z,1);
			interceptpoints[interceptcount++] = pos;
			average.x += pos.x;
			average.y += pos.y;
		}
	}

	float angles[7];

	if ( interceptcount >= 3 )
	{
		average.x /= interceptcount;
		average.y /= interceptcount;		// find average x/y.  z is obj the same

		for(  i =0 ; i < interceptcount ; i++ )	// find the angle to the average
		{
			angles[i] = -atan(interceptpoints[i].y-average.y, interceptpoints[i].x-average.x);	// - due to our geometry of x/y vertices directions and correct winding
		}

		uint counter = atomicCounterIncrement(atomiccounter);

		int j;
		for( j = 1 ; j < interceptcount; j++ )		// insert sort leaving the least angle at 0, most at end
		{
			float key = angles[j];
			i = j-1;

			if ( angles[i] > key )
			{
				vec4 keyi = interceptpoints[j];

				do
				{
					angles[i+1] = angles[i];
					interceptpoints[i+1] = interceptpoints[i];
					i--;
				} while( i>=0 && angles[i] > key);

				angles[i+1] = key;
				interceptpoints[i+1] = keyi;
			}
		}

		// RGBYCW
		vec4 colours[6] = { vec4(1,0,0,1),vec4(0,1,0,1), vec4(0,0,1,1),vec4(1,1,0,1),vec4(0,1,1,1),vec4(1,1,1,1)};

		for( int i = 0 ; i < interceptcount ; i++ )		// since we can only output a triangle strip, we must do each set individually.
		{
			vec4 c = vec4(1,instanceid*0.1,0,0.4);
			vs_color = c;
		    gl_Position = mc.ProjectionMatrix * average;
			EmitVertex();
			vs_color = c;
		    gl_Position = mc.ProjectionMatrix * interceptpoints[i];
		    EmitVertex();
			vs_color = c;
		    gl_Position = mc.ProjectionMatrix * interceptpoints[(i+1)%interceptcount ];
		    EmitVertex();
			EndPrimitive();
		}
	}
}
