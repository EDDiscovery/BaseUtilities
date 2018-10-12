using BaseUtils;
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
            CommandArgs args = new CommandArgs(stringargs);

            using (OpenTK.Toolkit.Init(new OpenTK.ToolkitOptions { EnableHighResolution = false, Backend = OpenTK.PlatformBackend.PreferNative }))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                string arg1 = args.Next();

                if (arg1.Equals("ShaderTest", StringComparison.InvariantCultureIgnoreCase))
                {
                    Application.Run(new ShaderTest());
                }
                else if (arg1.Equals("ShaderTest2", StringComparison.InvariantCultureIgnoreCase))
                {
                    Application.Run(new ShaderTest2());
                }
                else if (arg1.Equals("ShaderTest3", StringComparison.InvariantCultureIgnoreCase))
                {
                    Application.Run(new ShaderTest3());
                }
                else if (arg1.Equals("ShaderTest4", StringComparison.InvariantCultureIgnoreCase))
                {
                    Application.Run(new ShaderTest4());
                }
                else
                {
                    Application.Run(new RandomStars());
                }

            }
        }
    }
}
