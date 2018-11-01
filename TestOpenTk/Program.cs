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

                {
                    Application.Run(new RandomStars());
                }

            }
        }
    }
}
