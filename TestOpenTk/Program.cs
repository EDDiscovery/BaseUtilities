using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestOpenTk
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] stringargs)
        {
            OpenTKUtils.CommandArgs args = new OpenTKUtils.CommandArgs(stringargs);

            using (OpenTK.Toolkit.Init(new OpenTK.ToolkitOptions { EnableHighResolution = false, Backend = OpenTK.PlatformBackend.PreferNative }))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                string arg1 = args.Next();

                Type t = Type.GetType("TestOpenTk." + arg1, false, true);

                if (t == null)
                    t = Type.GetType("TestOpenTk.Test" + arg1, false, true);

                if (t == null)
                    t = Type.GetType("TestOpenTk.Shader" + arg1, false, true);

                if ( t == null )
                    t = Type.GetType("TestOpenTk.ShaderTest" + arg1,false,true);

                if (t != null)
                {
                    Application.Run((Form)Activator.CreateInstance(t));
                }
                else
                {
                    Application.Run(new TestMain());
                }

            }
        }
    }
}
