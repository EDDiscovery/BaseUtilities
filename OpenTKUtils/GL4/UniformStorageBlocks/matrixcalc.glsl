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
