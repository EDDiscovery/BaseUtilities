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

        public void Dispose()
        {
            items.Dispose();
        }

        public void Add( string name, IDisposable disp )
        {
            items.Add(name, disp);
        }

        public IGLTexture Tex(string name)
        {
            return (IGLTexture)items[name];
        }

        public IGLProgramShader Shader(string name)
        {
            return (IGLProgramShader)items[name];
        }

        public GLUniformBlock UB(string name)
        {
            return (GLUniformBlock)items[name];
        }

        public GLStorageBlock SB(string name)
        {
            return (GLStorageBlock)items[name];
        }

    }
}
