using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Environment;

namespace Utilities
{

    public class Logging
    {
        /// <summary>
        /// log levels - no restart required to change log levels in app configuration file
        /// </summary>
        public enum LogLevel {
            Info  = 2,
            Warning = 1,
            Debug = 3,
            Exception = 0,
            Data = 4
        }

        
        /// <summary>
        /// write log to appropriate folder and file 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLog(string message, string name,string thread="main", LogLevel logLevel=LogLevel.Info)
        {
            if (checkLoglevels(logLevel) == true)
            {
                string filename = "AppLog";
                if (ConfigurationManager.AppSettings["AppName"] != null)
                {
                    filename = ConfigurationManager.AppSettings["AppName"];
                }
                if (thread != "main")
                {
                    filename += "_" + thread;
                }

                string loglevelname = LoglevelToString(logLevel);

                string testtimestamp = DateTime.Now.ToString();

                string parentfolder = @"C:\";//Environment.GetFolderPath(SpecialFolder.ApplicationData);
                string filePath = parentfolder + @"\JBTAerotech\" + ConfigurationManager.AppSettings["AppName"] + @"\" + DateTime.Now.ToString("MMddyyyy") + @"\" + filename + ".DataLog.txt";

                System.IO.FileInfo file = new System.IO.FileInfo(filePath);
                file.Directory.Create(); // If the directory already exists, this method does nothing.
                // check if we have data TODO
                // TODO : avoid file access violations
                System.IO.File.AppendAllText(file.FullName, testtimestamp + " #### " + 
                                                            name + " #### " + 
                                                            loglevelname+ " #### " + 
                                                            message + " #### " + Environment.NewLine);
            }
        }

        

        /// <summary>
        /// get log levels from config file 
        /// </summary>
        public static bool checkLoglevels(Logging.LogLevel loglevel)
        {
            bool res = false;
            //<add key="logLevels" value ="debug|info|warning|exception"/>
            string loglevels = ConfigurationManager.AppSettings["logLevels"].ToString();
            switch (loglevel)
            {
                case Logging.LogLevel.Exception:
                    if (loglevels.Contains("exception"))
                        res = true;
                    else
                        res = false;
                    break;
                case Logging.LogLevel.Warning:
                    if (loglevels.Contains("warning"))
                        res = true;
                    else
                        res = false;
                    break;
                case Logging.LogLevel.Info:
                    if (loglevels.Contains("info"))
                        res = true;
                    else
                        res = false;
                    break;
                case Logging.LogLevel.Debug:
                    if (loglevels.Contains("debug"))
                        res = true;
                    else
                        res = false;
                    break;
                case Logging.LogLevel.Data:
                    if (loglevels.Contains("data"))
                        res = true;
                    else
                        res = false;
                    break;
                default:
                    break;
            }
            return res;
        }

        /// <summary>
        /// loglevel enum to string representation
        /// </summary>
        /// <param name="loglevel"></param>
        /// <returns></returns>
        public static string LoglevelToString(Logging.LogLevel loglevel)
        {
            string res = "info";
            //<add key="logLevels" value ="data|debug|info|warning|exception"/>
            
            switch (loglevel)
            {
                case Logging.LogLevel.Exception:
                    res = "Exception";
                    break;
                case Logging.LogLevel.Warning:
                    res = "Warning";
                    break;
                case Logging.LogLevel.Info:
                    res = "Info";
                    break;
                case Logging.LogLevel.Debug:
                    res = "Debug";
                    break;
                case Logging.LogLevel.Data:
                    res = "Data";
                    break;
                default:
                    res = "Info";
                    break;
            }
            return res;
        }
    }
}
