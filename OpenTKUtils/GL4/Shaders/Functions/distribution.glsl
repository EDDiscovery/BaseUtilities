#version 450 core

// x = value, centre = of distribution, stddist = std deviation

float gaussian(float x, float centre, float stddist)
{
    return exp(-(x-centre)*(x-centre)/(2*stddist*stddist));
}


