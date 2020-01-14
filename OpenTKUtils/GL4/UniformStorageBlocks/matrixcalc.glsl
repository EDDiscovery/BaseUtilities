/*
 * Copyright © 2019 Robbyxp1 @ github.com
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
