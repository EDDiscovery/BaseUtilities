/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
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

 using System;

namespace OpenTKUtils.GL4
{
    // This is a memory class in which you can register GL type items and it will manage them
    // items have names to find them again
    // Items to be held are
    //      Textures
    //      Program shaders
    //      Uniform blocks
    //      Storage blocks
    //      Atomic blocks
    //      Buffers
    //      Vertex Arrays

    public class GLItemsList : IDisposable
    {
        DisposableDictionary<string, IDisposable> items = new DisposableDictionary<string, IDisposable>();
        private int unnamed = 0;

        public void Dispose()
        {
            items.Dispose();
        }

        public bool Contains(string name )
        {
            return items.ContainsKey(name);
        }

        // Add existing items

        public IGLTexture Add(string name, IGLTexture disp)
        {
            items.Add(name, disp);
            return disp;
        }

        public IGLProgramShader Add(string name, IGLProgramShader disp)
        {
            items.Add(name, disp);
            return disp;
        }

        public IGLPipelineShader Add(string name, IGLPipelineShader disp)
        {
            items.Add(name, disp);
            return disp;
        }

        public GLVertexArray Add(string name, GLVertexArray disp)
        {
            items.Add(name, disp);
            return disp;
        }

        public GLUniformBlock Add(string name, GLUniformBlock disp)
        {
            items.Add(name, disp);
            return disp;
        }

        public GLStorageBlock Add(string name, GLStorageBlock disp)
        {
            items.Add(name, disp);
            return disp;
        }

        public GLAtomicBlock Add(string name, GLAtomicBlock disp)
        {
            items.Add(name, disp);
            return disp;
        }

        public GLBuffer Add(string name, GLBuffer disp)
        {
            items.Add(name, disp);
            return disp;
        }

        // Get existing items

        public GLTextureBase Tex(string name)
        {
            return (GLTextureBase)items[name];
        }

        public IGLProgramShader Shader(string name)
        {
            return (IGLProgramShader)items[name];
        }

        public IGLPipelineShader PLShader(string name)
        {
            return (IGLPipelineShader)items[name];
        }

        public GLVertexArray VA(string name)
        {
            return (GLVertexArray)items[name];
        }

        public GLUniformBlock UB(string name)
        {
            return (GLUniformBlock)items[name];
        }

        public GLStorageBlock SB(string name)
        {
            return (GLStorageBlock)items[name];
        }

        public GLAtomicBlock AB(string name)
        {
            return (GLAtomicBlock)items[name];
        }

        public GLBuffer B(string name)
        {
            return (GLBuffer)items[name];
        }

        public GLBuffer LastBuffer(int c = 1)
        {
            return (GLBuffer)items.Last(typeof(GLBuffer), c);
        }

        public T Last<T>(int c = 1) where T:class
        {
            return (T)items.Last(typeof(T), c);
        }

        public T Get<T>(string name)
        {
            return (T)items[name];
        }

        // New items

        public GLVertexArray NewArray(string name = null)
        {
            GLVertexArray b = new GLVertexArray();
            items[EnsureName(name)] = b;
            return b;
        }

        public GLUniformBlock NewUniformBlock(int bindingindex, string name = null)
        {
            GLUniformBlock sb = new GLUniformBlock(bindingindex);
            items[EnsureName(name)] = sb;
            return sb;
        }

        public GLStorageBlock NewStorageBlock(int bindingindex, bool std430 = false, string name = null)
        {
            GLStorageBlock sb = new GLStorageBlock(bindingindex, std430);
            items[EnsureName(name)] = sb;
            return sb;
        }

        public GLAtomicBlock NewAtomicBlock(int bindingindex, string name = null)
        {
            GLAtomicBlock sb = new GLAtomicBlock(bindingindex);
            items[EnsureName(name)] = sb;
            return sb;
        }

        public GLBuffer NewBuffer(string name = null)
        {
            GLBuffer b = new GLBuffer();
            items[EnsureName(name)] = b;
            return b;
        }

        // helpers

        private string EnsureName(string name)
        {
            return (name == null) ? ("Unnamed_" + (unnamed++)) : name;
        }

    }
}
