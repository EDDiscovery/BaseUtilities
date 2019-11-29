#version 450 core
            
layout (std140, binding = 1) uniform PointBlock
{
	vec4 p[8];      // model matrix multipled positions
	float slicestart;  // z start and 
	float slicedist;	// distance between
} pb;

