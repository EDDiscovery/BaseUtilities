#version 450 core
            
layout (std140, binding = 1) uniform PointBlock
{
	vec4 p[8];      // model positions
	float minz;
	float maxz;
	vec4 eyeposition; // model positions
	float slicestart;
	float slicedist;
} pb;

