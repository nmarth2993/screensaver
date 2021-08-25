using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenSaver
{
    static class Program
    {
        // code closely followed from
        // https://sites.harding.edu/fmccown/screensaver/screensaver.html


        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Console.WriteLine("hard to quit. don't start this program");
            // return;

            if (args.Length > 0)
            {
                string firstArg = args[0].ToLower().Trim();
                string secondArg = null;

                // handle cases where arguments are colon delimited
                if (firstArg.Length > 2)
                {
                    secondArg = firstArg.Substring(3).Trim();
                    firstArg = firstArg.Substring(0, 2);
                }
                else if (args.Length > 1)
                {
                    secondArg = args[1];
                }

                if (firstArg == "/c")
                {
                    // TODO
                }
                else if (firstArg == "/p")
                {
                    Console.WriteLine("ctor for preview handle may not work");
                    return;
                    if (secondArg == null)
                    {
                        MessageBox.Show("Argument requires window handler", "ScreenSaver", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    IntPtr previewWndHandle = new IntPtr(long.Parse(secondArg));
                    Application.Run(new ScreenSaverForm(previewWndHandle));
                }
                else if (firstArg == "/s")
                {
                    ShowScreenSaver();
                    Application.Run();
                }
                else
                {
                    MessageBox.Show("Invalid command line argument", "ScreenSaver", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            else
            {
                // no argument, same as /c
            }

            // Application.Run(new ScreenSaverForm());
        }

        static void ShowScreenSaver()
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                ScreenSaverForm screensaver = new ScreenSaverForm(screen.Bounds);
                screensaver.Show();
            }
        }

    }
}
