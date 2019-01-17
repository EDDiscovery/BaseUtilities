using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BaseUtils.Web;
using System.Web.UI;
using System.Net;

namespace TestWebServer
{
    public partial class Form1 : Form
    {
        SimpleWebServer ws;
        SimpleWebServerDispatcher wd;
        Example1 ex1;
        SimpleWebServerFileNode fn1;
        
        public Form1()
        {
            InitializeComponent();

            wd = new SimpleWebServerDispatcher();
            ws = new SimpleWebServer((req, obj) => { return ((SimpleWebServerDispatcher)obj).Response(req); }, wd, "http://localhost:8080/");


            ex1 = new Example1();

            fn1 = new SimpleWebServerFileNode(@"c:\code\html\");

            wd.AddNode("/", ex1);
            wd.AddPartialNode("/file/", fn1);

            ws.Run();

        }
    }

    public class Example1 : SimpleWebServerDynamicNode
    {
        public override byte[] Response(string partialpath, HttpListenerRequest request)
        {
            StringWriter stringWriter = new StringWriter();

            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Html); // Begin #1

                writer.RenderBeginTag(HtmlTextWriterTag.Body); // Begin #1

                writer.RenderBeginTag(HtmlTextWriterTag.P); // Begin #1

                writer.Write("EDDiscovery Web Server at " + DateTime.Now.ToStringZulu());

                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Href, "/file/a.txt");
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write("Click here" + Environment.NewLine);
                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Src, "/file/Logo.png");
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "256");
                writer.AddAttribute(HtmlTextWriterAttribute.Height, "256");
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, "Alt image");

                writer.RenderBeginTag(HtmlTextWriterTag.Img); // Begin #3
                writer.RenderEndTag(); // End #3

                writer.RenderEndTag();
                writer.RenderEndTag();
            }

            return Encoding.UTF8.GetBytes(stringWriter.ToString());
        }

    }
}
