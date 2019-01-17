using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace BaseUtils.Web
{
    public class SimpleWebServerFileNode : ISimpleWebServerNode
    {
        private string path;

        public SimpleWebServerFileNode(string pathbase)
        {
            this.path = pathbase;
        }

        public virtual byte[] Response(string partialpath, HttpListenerRequest request)
        {
            string file = Path.Combine(path, partialpath);
            string ext = Path.GetExtension(file);

            System.Diagnostics.Debug.WriteLine("In " + Thread.CurrentThread.Name + " file " + file);
            try
            {
                if (ext.Equals(".png"))
                    return File.ReadAllBytes(file);
                else
                {
                    string data = File.ReadAllText(file, Encoding.UTF8);
                    return Encoding.UTF8.GetBytes(data);
                }
            }
            catch (Exception ex)
            {
                string text = "File not found " + request.Url + " local " + file + Environment.NewLine + ex.Message;
                return Encoding.UTF8.GetBytes(text);
            }
        }
    }
}
