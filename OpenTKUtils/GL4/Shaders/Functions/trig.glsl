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

const float PI = 3.1415926535897932384626433832795;

vec2 AzEl(vec3 curpos, vec3 target)		//vec2.x = inclination, vec2.y = azimuth
{
	vec3 delta = target-curpos;
	float radius = length(delta);
	if ( radius < 0.000001)
		return vec2(0,0);	
	float inclination = acos(delta.y/radius);
	float azimuth = delta.x==0 ? (delta.z>=0 ? PI/2 : -PI/2) : atan(delta.z/delta.x);

    if (delta.x >= 0)      // atan wraps -90 (south)->+90 (north), then -90 to +90 around the y axis, going anticlockwise
        azimuth = PI/2 - azimuth;     // adjust
    else
        azimuth = -PI/2 - azimuth;

	return vec2(inclination,azimuth);
}
