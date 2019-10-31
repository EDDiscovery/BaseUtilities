/*
 * Copyright © 2015 - 2019 EDDiscovery development team
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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 * Noise from the seeds of andromeda MIT licence.
 */


namespace OpenTKUtils.GL4
{

    public static class GLShaderFunctionsDist
    {
        public static string DistFunctions =
@"
float gaussian(float x, float centre, float stddist)
{
    return exp(-(x-centre)*(x-centre)/(2*stddist*stddist));
}


//{
//    double xd = double(x)-centre;
//    xd = -xd * xd;
//    double d = 2 * stddist * stddist;
//    return exp(float(xd / d);
";

    }
}
