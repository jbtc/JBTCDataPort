using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Utilities
{
    public class LoggingWithQueue : Logging
    {
        private Dictionary<string, List<string>> logsToBeWrittenA = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> logsToBeWrittenB = new Dictionary<string, List<string>>();
        private string logQueueSwitch = "a";

        public LoggingWithQueue()
        {
            //
        }

        ~LoggingWithQueue()
        {
            //
        }

        /// <summary>
        /// Saves the logs.
        /// </summary>
        public void SaveLogs()
        {
            Dictionary<string, List<string>> logsToBeWritten;

            switch (logQueueSwitch)
            {
                case "a":
                    logQueueSwitch = "b";
                    logsToBeWritten = new Dictionary < string, List < string >> (logsToBeWrittenA);
                    logsToBeWrittenA = new Dictionary<string, List<string>>();
                    break;
                default:
                    logQueueSwitch = "a";
                    logsToBeWritten = new Dictionary<string, List<string>>(logsToBeWrittenB);
                    logsToBeWrittenB = new Dictionary<string, List<string>>();
                    break;
            }

            
            foreach (KeyValuePair<string, List<string>> logset in logsToBeWritten)
            {
                foreach (string logvalue in logset.Value)
                {
                    string filePath = logset.Key;
                    System.IO.FileInfo file = new System.IO.FileInfo(filePath);
                    file.Directory.Create(); // If the directory already exists, this method does nothing.
                                             // check if we have data TODO
                                             // TODO : avoid file access violations
                    System.IO.File.AppendAllText(file.FullName, logvalue);
                }
            }
            logsToBeWritten = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// write log to appropriate folder and file 
        /// </summary>
        /// <param name="message"></param>
        public void WriteLog(string message, string name, string thread = "main", LogLevel logLevel = LogLevel.Info)
        {
            if (Logging.checkLoglevels(logLevel))
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
                string filePath = @"C:\" + @"\JBTAerotech\" + ConfigurationManager.AppSettings["AppName"] +
                    @"\" + DateTime.Now.ToString("MMddyyyy") + @"\" + filename + ".DataLog.txt";

                Dictionary<string, List<string>> logsToBeWritten;

                switch (logQueueSwitch)
                {
                    case "a":
                        logsToBeWritten = logsToBeWrittenA;
                        break;
                    default:
                        logsToBeWritten = logsToBeWrittenB;
                        break;
                }

                if (logsToBeWritten.ContainsKey(filePath))
                {
                    logsToBeWritten[filePath].Add(testtimestamp + " #### " +
                                                    name + " #### " +
                                                    loglevelname + " #### " +
                                                    message + " #### " +
                                                    Environment.NewLine);
                }
                else
                {
                    List<string> logCollection = new List<string>();
                    logCollection.Add(testtimestamp + " #### " +
                                            name + " #### " +
                                            loglevelname + " #### " +
                                            message + " #### " +
                                            Environment.NewLine);
                    logsToBeWritten.Add(filePath, logCollection);
                }
            }
        }

        public void WriteLog(string loglevels, string message, string name, string thread = "main", LogLevel logLevel = LogLevel.Info)
        {
            string ll = LoglevelToString(logLevel).ToUpper();

            if((loglevels.ToUpper()).Contains(ll)== true)
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
                string filePath = @"C:\" + @"\JBTAerotech\" + ConfigurationManager.AppSettings["AppName"] +
                    @"\" + DateTime.Now.ToString("MMddyyyy") + @"\" + filename + ".DataLog.txt";

                Dictionary<string, List<string>> logsToBeWritten;

                switch (logQueueSwitch)
                {
                    case "a":
                        logsToBeWritten = logsToBeWrittenA;
                        break;
                    default:
                        logsToBeWritten = logsToBeWrittenB;
                        break;
                }

                if (logsToBeWritten.ContainsKey(filePath))
                {
                    logsToBeWritten[filePath].Add(testtimestamp + " #### " +
                                                    name + " #### " +
                                                    loglevelname + " #### " +
                                                    message + " #### " +
                                                    Environment.NewLine);
                }
                else
                {
                    List<string> logCollection = new List<string>();
                    logCollection.Add(testtimestamp + " #### " +
                                            name + " #### " +
                                            loglevelname + " #### " +
                                            message + " #### " +
                                            Environment.NewLine);
                    logsToBeWritten.Add(filePath, logCollection);
                }
            }
        }
    }
}
