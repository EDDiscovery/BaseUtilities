#version 450 core
            
layout(std140, binding=0) uniform MatrixCalc
{
    mat4 ProjectionModelMatrix;
    mat4 ProjectionMatrix;
    mat4 ModelMatrix;
    vec4 TargetPosition;		// vertex position, before ModelMatrix
    vec4 EyePosition;			// vertex position, before ModelMatrix
    float EyeDistance;			// between eye and target
} mc;
