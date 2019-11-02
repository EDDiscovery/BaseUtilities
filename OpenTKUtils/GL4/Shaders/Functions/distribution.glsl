#version 450 core

float gaussian(float x, float centre, float stddist)
{
    return exp(-(x-centre)*(x-centre)/(2*stddist*stddist));
}


