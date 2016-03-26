using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace PDBundleModPatcher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        [STAThread]
        static void Main(string[] args)
        {
            //bool extract = false;

            //for (int i = 0; i < args.Length; i++)
            //{
                //switch (args[i])
                //{
                    //case "-extract":
                        //extract = true;
                        //break;
                    //case "-extract_one":
                        //extract = true;
                        //StaticStorage.extract.extract_one = true;
                        //StaticStorage.extract.extract_id = args[i + 1];
                        //break;
                    //case "-custom_extract_path":
                        //StaticStorage.settings.CustomExtractPath = args[i + 1];
                        //break;
                //}
            //}


            if (!AppDomain.CurrentDomain.FriendlyName.EndsWith("vshost.exe"))
            {
                Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            }

            //if (extract)
            //{
                //LoadSettings();
                //StaticStorage.extract.Start();
            //}
            //else
            //{
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            //}
        }

        public static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show("Please report this error and contents of exceptions.log to the author:\n\n" + e.Exception.Message, "Unhandled Thread Exception");
            // here you can log the exception ...

            try
            {
                using (StreamWriter sw_log = new StreamWriter("./exceptions.log", true))
                {
                    sw_log.WriteLine("====");
                    sw_log.WriteLine(DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " Unhandled Thread Exception");
                    sw_log.WriteLine("====");
                    sw_log.WriteLine(e.Exception.ToString());

                }
            }
            catch
            {
                Application.Exit();
            }
        }

        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Please report this error and contents of exceptions.log to the author:\n\n" + (e.ExceptionObject as Exception).Message, "Unhandled UI Exception");
            // here you can log the exception ...

            try
            {
                using (StreamWriter sw_log = new StreamWriter("./exceptions.log", true))
                {
                    sw_log.WriteLine("====");
                    sw_log.WriteLine(DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " Unhandled UI Exception");
                    sw_log.WriteLine("====");
                    sw_log.WriteLine(e.ExceptionObject.ToString());

                }
            }
            catch
            {
                Application.Exit();
            }
        }

        public static void LoadSettings()
        {
            if (!File.Exists(MainForm.SettingsFile))
            {
                StaticStorage.settings = new ProgramSettings();
            }
            else
            {
                var serializer = new XmlSerializer(StaticStorage.settings.GetType());
                using (var fs = new FileStream(MainForm.SettingsFile, FileMode.Open, FileAccess.Read))
                {
                    StaticStorage.settings = (ProgramSettings)serializer.Deserialize(fs);
                }
            }
        }

    }
}
