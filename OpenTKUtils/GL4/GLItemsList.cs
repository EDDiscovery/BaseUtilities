using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4
{
    // holds GL items and disposes of them..

    public class GLItemsList : IDisposable
    {
        BaseUtils.DisposableDictionary<string, IDisposable> items = new BaseUtils.DisposableDictionary<string, IDisposable>();
        private int unnamed = 0;

        public void Dispose()
        {
            items.Dispose();
        }

        public bool Contains(string name )
        {
            return items.ContainsKey(name);
        }

        public IGLTexture Add(string name, IGLTexture disp)
        {
            items.Add(name, disp);
            return disp;
        }

        public IGLTexture Tex(string name)
        {
            return (IGLTexture)items[name];
        }


        public IGLProgramShader Add(string name, IGLProgramShader disp)
        {
            items.Add(name, disp);
            return disp;
        }

        public IGLProgramShader Shader(string name)
        {
            return (IGLProgramShader)items[name];
        }

        public GLUniformBlock Add(string name, GLUniformBlock disp)
        {
            items.Add(name, disp);
            return disp;
        }

        public GLUniformBlock UB(string name)
        {
            return (GLUniformBlock)items[name];
        }

        public GLStorageBlock Add(string name, GLStorageBlock disp)
        {
            items.Add(name, disp);
            return disp;
        }

        public GLStorageBlock SB(string name)
        {
            return (GLStorageBlock)items[name];
        }

        public GLBuffer Add(string name, GLBuffer disp)
        {
            items.Add(name, disp);
            return disp;
        }

        public GLBuffer NewBuffer(string name = null)
        {
            if (name == null)
                name = "Unnamed_" + (unnamed++);

            GLBuffer b = new GLBuffer();
            items[name] = b;
            return b;
        }

        public GLBuffer B(string name)
        {
            return (GLBuffer)items[name];
        }

        public GLBuffer LastBuffer(int c= 1)
        {
            return (GLBuffer)items.Last(typeof(GLBuffer), c);
        }

        public GLVertexArray NewArray(string name = null)
        {
            if (name == null)
                name = "Unnamed_" + (unnamed++);

            GLVertexArray b = new GLVertexArray();
            items[name] = b;
            return b;
        }

        public GLVertexArray VA(string name)
        {
            return (GLVertexArray)items[name];
        }

    }
}
