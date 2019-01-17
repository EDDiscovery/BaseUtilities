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
    public abstract class SimpleWebServerDynamicNode : ISimpleWebServerNode
    {
        public virtual byte[] Response(string partialpath, HttpListenerRequest request)
        {
            return new byte[] { 0 };
        }
    }
}
