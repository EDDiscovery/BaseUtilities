// volumetric geo shader. 
// Inputs are two vertex's describing point 0 and 6 in the volumetric box.

#version 450 core

layout(std140, binding=0) uniform MatrixCalc
{
    mat4 ProjectionModelMatrix;
    mat4 ProjectionMatrix;
    mat4 ModelMatrix;
    vec4 TargetPosition;
    vec4 EyePosition;
    float EyeDistance;
} mc;

layout (lines) in;              // get two vertexes discribing p0 and p6 in
layout (triangle_strip) out;
layout (max_vertices=32) out;

in int instance[];

out vec4 vs_color;
out vec3 vs_texcoord;

layout (binding = 5, std430) buffer DataBack
{
    vec4[] databack;
};

layout (binding = 6, offset=0) uniform atomic_uint atomiccounter;

void main(void)
{
    // Compute the intecepts of a z plane across this box
	//       p5----v5----p6    backface: p4,p5,p6,p7
	//      /|           /|
	//     / v4         / v6
	//   v9  |         /  |
	//   /   p4----v5----p7
	//  /   /        /   /
	// p1----v1----p2   /		front face: p0,p1,p2,p3.
	// |  /	        |  v11
	//v0 /v8       v2 /
	// |/	        |/
	// p0----v3----p3

	// each vertex is multipled by the modelmatrix and then the z plane intercept is found
	// vectors are front face : v0 = p0->p1, v1 = p1->p2, v2 = p2->p3, v3 = p3->p0
	// vectors are back face : v4 = p4->p5, v5 = p5->p6, v6 = p6->p7, v7 = p6->p4
	// vectors are sides : v8 = p0->p4, v9 = p1->p5, v10 = p2->p6, v11 = p3->p7
	
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
        if (p[i].z > maxzv)		// don't elseif
        {
            maxzv = p[i].z;
            maxz = i;
        }
    }

	vec4 eyeposition = mc.ModelMatrix * mc.EyePosition;
	//vec4 eyeposition = mc.EyePosition;

	int instanceid = instance[0];
	// find the z point
	float zdist =p[maxz].z-p[minz].z;
	float z = p[minz].z + zdist * (0.95+instanceid * 0.1);		// first is painted at the back, then forward..

	uint counter = atomicCounterIncrement(atomiccounter);

	if ( eyeposition.z < z-2 )		// we should be able to clip the computation if our eye z is in front of the z plane.
		return;

	int i1lookup[12] = {0,1,3,0, 4,5,7,4, 0,1,2,3};	// comparision index to check for ic=0 to 12
	int i2lookup[12] = {1,2,2,3, 5,6,6,7, 4,5,6,7};	
	vec3 texdef[12] = { vec3(0,9,0), vec3(9,1,0), vec3(1,9,0), vec3(9,0,0),		// front face.  9 is replace by %,  x or y varies, z = 0
					    vec3(0,9,1), vec3(9,1,1), vec3(1,9,1), vec3(9,0,1),		// back face, x or y varies, z= 1
					    vec3(0,0,9), vec3(0,1,9), vec3(1,1,9), vec3(1,0,9)};	// sides, z varies, x/y set

	vec4 interceptpoints[6];	// holds model view points of intercept
	vec3 texpoints[6];			// intercept tex-coords..
	int interceptcount = 0;		// count
	vec4 average = vec4(0,0,z,1);// average of found intercepts
	vec3 texaverage = vec3(0,0,0);	// average of tex coords
	int icnumber[6];

	int ic = 0;
    for ( ic = 0 ; ic < 12 ; ic++ )
    {
		int i1 = i1lookup[ic];// computes the pairs to compare..
		int i2 = i2lookup[ic];		

		float interceptv = (z - p[i1].z) / (p[i2].z - p[i1].z);	// z% intercept on those pairs

		if ( interceptv >= 0 && interceptv <=1)	// we have an intercept
		{
			vec4 pos = vec4( p[i1].x + (p[i2].x-p[i1].x) * interceptv , p[i1].y + (p[i2].y-p[i1].y) * interceptv, z,1);
			interceptpoints[interceptcount] = pos;	// set intercept point, on the z plane

			average.x += pos.x;		// update average
			average.y += pos.y;

			icnumber[interceptcount] = ic;

			if ( bool(ic & 8))	// sides, z varying (v8-11)
			{
				texpoints[interceptcount] = vec3(texdef[ic].x,texdef[ic].y,interceptv);
			}
			else if ( bool(ic&1))	// X's (v1,v3,v5,v7)
			{
				texpoints[interceptcount] = vec3(interceptv,texdef[ic].y,texdef[ic].z);
			}
			else	// Y's (v0,v2,v4,v6)
			{
				texpoints[interceptcount] = vec3(texdef[ic].x,interceptv,texdef[ic].z);
			}

			texaverage = texaverage + texpoints[interceptcount];

			interceptcount++;
		}
	}

	if ( interceptcount >= 3 )	// 3 to 6 only
	{
		average.x /= interceptcount;
		average.y /= interceptcount;		// find average x/y.  z is obj the same

		texaverage /= interceptcount;

		float angles[6];

		for(  i =0 ; i < interceptcount ; i++ )	// find the angle to the average
		{
			angles[i] = -atan(interceptpoints[i].y-average.y, interceptpoints[i].x-average.x);	// - due to our geometry of x/y vertices directions and correct winding
		}

		int j;
		for( j = 1 ; j < interceptcount; j++ )		// insert sort leaving the least angle at 0, most at end
		{
			float key = angles[j];
			i = j-1;

			if ( angles[i] > key )
			{
				vec4 keyi = interceptpoints[j];
				vec3 texi = texpoints[j];
				int icn = icnumber[j];

				do
				{
					angles[i+1] = angles[i];
					interceptpoints[i+1] = interceptpoints[i];
					texpoints[i+1] = texpoints[i];
					icnumber[i+1] = icnumber[i];
					i--;
				} while( i>=0 && angles[i] > key);

				angles[i+1] = key;
				interceptpoints[i+1] = keyi;
				texpoints[i+1] = texi;
				icnumber[i+1] = icn;
			}
		}

		for(  i =0 ; i < interceptcount ; i++ )	// find the angle to the average
		{
			databack[i*2+3] = vec4(texpoints[i],9);
		}


		// RGBYCW
		vec4 colours[6] = { vec4(1,0,0,1),vec4(0,1,0,1), vec4(0,0,1,1),vec4(1,1,0,1),vec4(0,1,1,1),vec4(1,1,1,1)};

		for( int i = 0 ; i < interceptcount ; i++ )		// since we can only output a triangle strip, we must do each set individually.
		{
			vec4 c = vec4(1,instanceid*0.1,1,0.4);
			vs_color = colours[0];
		    gl_Position = mc.ProjectionMatrix * average;
			databack[i*3] =  gl_Position;
			vs_texcoord = texaverage;
			EmitVertex();

			vs_color = colours[1];
		    gl_Position = mc.ProjectionMatrix * interceptpoints[i];
			databack[i*3+1] =  gl_Position;
			vs_texcoord = texpoints[i];
		    EmitVertex();

			int iother = (i+1)%interceptcount;
			vs_color = colours[2];
		    gl_Position = mc.ProjectionMatrix * interceptpoints[ iother ];
			databack[i*3+2] =  gl_Position;
			vs_texcoord = texpoints[iother];
		    EmitVertex();

			EndPrimitive();
		}
	}
}


