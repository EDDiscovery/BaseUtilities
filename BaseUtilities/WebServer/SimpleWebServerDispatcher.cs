using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;

namespace BaseUtils.Web
{
    public interface ISimpleWebServerNode
    {
        byte[] Response(string partialpath, HttpListenerRequest request);
    }

    public class SimpleWebServerDispatcher
    {
        private Dictionary<string, ISimpleWebServerNode> terminalnodes;
        private List<Tuple<string, ISimpleWebServerNode>> partialpathnodes;

        public ISimpleWebServerNode URLNotFound { get; set; }

        public SimpleWebServerDispatcher()
        {
            terminalnodes = new Dictionary<string, ISimpleWebServerNode>();
            partialpathnodes = new List<Tuple<string, ISimpleWebServerNode>>();
        }

        public void AddNode(string node, ISimpleWebServerNode disp)
        {
            terminalnodes[node] = disp;
        }

        public void AddPartialNode(string node, ISimpleWebServerNode disp)
        {
            partialpathnodes.Add(new Tuple<string, ISimpleWebServerNode>(node, disp));
        }

        public byte[] Response(HttpListenerRequest request)
        {
            string rawurl = request.RawUrl;

            if (terminalnodes.ContainsKey(rawurl))
                return terminalnodes[rawurl].Response("",request);

            var disp = partialpathnodes.Find(x => rawurl.StartsWith(x.Item1));
            if (disp != null)
                return disp.Item2.Response(rawurl.Substring(disp.Item1.Length), request);

            if (URLNotFound!=null)
                return URLNotFound.Response("",request);

            string notfound = "Not found boyo " + request.Url;
            return Encoding.UTF8.GetBytes(notfound);
        }
    }

}
