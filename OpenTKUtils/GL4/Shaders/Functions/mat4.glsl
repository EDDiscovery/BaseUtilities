/*
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

mat4 mat4identity()
{
	return mat4(	1.0,0,0,0,
					0,1,0,0,
					0,0,1,0,
					0,0,0,1);
}

mat4 mat4rotateX(float radians)
{
	float cosv = cos(radians);
	float sinv = sin(radians);
	mat4 v = mat4(	1.0,	0,		0,		0,
					0,		cosv,	sinv,	0,
					0,		-sinv,	cosv,	0,
					0,		0,		0,		1);
	return v;
}

mat4 mat4rotateY(float radians)
{
	float cosv = cos(radians);
	float sinv = sin(radians);
	mat4 v = mat4(	cosv,	0,		-sinv,	0,
					0,		1,		0,		0,
					sinv,	0,		cosv,	0,
					0,		0,		0,		1);
	return v;
}

mat4 mat4rotateXthenY(float radiansx, float radiansy)		// Column major order, counterintuitive for Rob, but X CM mult by Y rows
{
	float cx = cos(radiansx);
	float sx = sin(radiansx);
	float cy = cos(radiansy);
	float sy = sin(radiansy);
	mat4 v = mat4(	cy,-sx*-sy,cx*-sy,0,			// X Columns 1-4 (1,0,0,0) (0,cx,-sx,0) | (0,sx,cx,0) | (0,0,0,1) with Y rows 1 (cy,0,-sy,0)
					0, cx,sx, 0,					// X Columns 1-4 (1,0,0,0) (0,cx,-sx,0) | (0,sx,cx,0) | (0,0,0,1) with Y rows 2 (0,1,0,0)
					sy,cy*-sx,cx*cy,0,				// X Columns 1-4 (1,0,0,0) (0,cx,-sx,0) | (0,sx,cx,0) | (0,0,0,1) with Y rows 2 (sy,0,cy,0)
					0,0,0,1); 						// X Columns 1-4 (1,0,0,0) (0,cx,-sx,0) | (0,sx,cx,0) | (0,0,0,1) with Y rows 3 (0,0,0,1)
	return v;
}


mat4 mat4rotateZ(float radians)
{
	float cosv = cos(radians);
	float sinv = sin(radians);
	mat4 v = mat4(	cosv,sinv,0,0,
					-sinv,cosv,0,0,
					0,0,1,0,
					0,0,0,1);
	return v;
}

mat4 mat4translation(vec3 translate)
{
	mat4 v = mat4(	1,0,0,0,
					0,1,0,0,
					0,0,1,0,
					translate.x,translate.y,translate.z,1);
	return v;
}

mat4 mat4scale(float scale)
{
	mat4 v = mat4(	scale,0,0,0,
					0,scale,0,0,
					0,0,scale,0,
					0,0,0,1);
	return v;
}

