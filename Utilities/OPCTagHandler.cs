using jbt_opc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Utilities
{
    public class OPCTagHandler
    {
        public volatile string[] tags;
        private volatile JBTOPC _jbtopc;
        public volatile string threadname ;

        public Dictionary<string, int> IntWrites = new Dictionary<string, int>();
        public Dictionary<string, double> FloatWrites = new Dictionary<string, double>();
        public Dictionary<string, string> StringWrites = new Dictionary<string, string>();
        public bool canAddTags = true;
        public bool terminateThread = false;
        public string loglevels = "warning|exception";

        private int tagwriteSleepTime;
        private string sysname;
        private List<string> tagNames;
        private string oPCServerName;
        private int sleepTimeBetweenOPCTagWritesInSecs;
        private Object memberLock = new Object();        
        
        private LoggingWithQueue lwq = new LoggingWithQueue();


        /// <summary>
        /// Initializes a new instance of the <see cref="OPCTagHandler"/> class. get tags and subscribe
        /// </summary>
        /// <param name="sysname">The sysname.</param>
        /// <param name="tagNames">The tag names.</param>
        /// <param name="oPCServerName">Name of the o pc server.</param>
        /// <param name="sleepTimeBetweenOPCTagWritesInSecs">The sleep time between opc tag writes in secs.</param>
        public OPCTagHandler(string sysname, List<string> tagNames, string oPCServerName, int sleepTimeBetweenOPCTagWritesInSecs)
        {
            threadname = sysname+"_opc_"+ oPCServerName;

            tagwriteSleepTime = sleepTimeBetweenOPCTagWritesInSecs;

            tags = new string[tagNames.Count];
            int tindex = 0;
            foreach (string set in tagNames)
            {
                tags[tindex] = set;
                tindex++;
            }

            _jbtopc = new JBTOPC(tagNames);
            // _jbtopc.OASServer sample  @"\\10.204.152.11\";
            _jbtopc.OASServer = @"\\" + oPCServerName + @"\";
        }

        




        /// <summary>
        /// remove tags from subscription
        /// </summary>
        ~OPCTagHandler()
        {
            // clean up
            _jbtopc = null;
            tags = null;
        }


        /// <summary>
        /// Saves the log.
        /// </summary>
        public void saveLog()
        {
            lwq.SaveLogs();
        }
        /// <summary>
        /// start task
        /// </summary>
        public void start()
        {
            try
            {
                string msg = "running opc connection";
                lwq.WriteLog(msg, "OPCTagHandler", threadname, Logging.LogLevel.Info);

                while (terminateThread == false)// check if app terminates
                {
                    //check for tags to write 
                    checkForTagWrites();
                    //Utilities.Logging.WriteLog("start - loop - checkForTagWrites done. sleeptime: "+tagwriteSleepTime.ToString(), "OPCTagHandler", threadname, Logging.LogLevel.Debug);

                    System.Threading.Thread.Sleep(tagwriteSleepTime * 1000);

                }
                msg = "closing opc connection";
                lwq.WriteLog(loglevels, msg, "OPCTagHandler", threadname, Logging.LogLevel.Info);
            }
            catch (Exception esd)
            {
                string emsg = "Exception:" + esd.Message;
                if (esd.InnerException != null)
                {
                    emsg += " inner: " + esd.InnerException.Message;
                }
                Utilities.Logging.WriteLog(emsg, "OPCTagHandler- start", threadname, Logging.LogLevel.Exception);
            }
        }

        /// <summary>
        /// check if tags need written
        /// </summary>
        private void checkForTagWrites()
        {
            List<string> tagsToRemove = new List<string>();
            try
            {
                
                lock(memberLock)
                {
                    canAddTags = false;// lock tags
                    if (IntWrites.Count > 0)
                    {
                        foreach (KeyValuePair<string, int> set in IntWrites)
                        {
                            Utilities.Logging.WriteLog("checkForTagWrites IntWrites data to write: " + set.Key, "OPCTagHandler", threadname, Logging.LogLevel.Debug);
                            string res = WriteTagInt(set.Key, set.Value);
                            res = res.Replace("|", "").Trim();
                            if (res == "Good Quality")
                            {
                                lwq.WriteLog(loglevels, "checkForTagWrites IntWrites data to write: " + set.Key + " good write", "OPCTagHandler", threadname, Logging.LogLevel.Debug);
                            }
                            else
                            {
                                lwq.WriteLog(loglevels, "Error: Int Write failed for " + set.Key + ". Write result: " + res, "OPCTagHandler", threadname, Logging.LogLevel.Warning);
                            }
                            tagsToRemove.Add(set.Key);
                        }
                    }


                    if (FloatWrites.Count > 0)
                    {
                        foreach (KeyValuePair<string, double> set in FloatWrites)
                        {
                            Utilities.Logging.WriteLog("checkForTagWrites FloatWrites data to write: " + set.Key, "OPCTagHandler", threadname, Logging.LogLevel.Debug);
                            string res = WriteTagDouble(set.Key, set.Value);
                            res = res.Replace("|", "").Trim();
                            if (res == "Good Quality")
                            {
                                lwq.WriteLog(loglevels,"checkForTagWrites FloatWrites data to write: " + set.Key + " good write", "OPCTagHandler", threadname, Logging.LogLevel.Debug);
                            }
                            else
                            {
                                lwq.WriteLog(loglevels, "Error: Float Write failed for " + set.Key + ". Write result: " + res, "OPCTagHandler", threadname, Logging.LogLevel.Warning);
                            }
                            tagsToRemove.Add(set.Key);
                        }
                    }

                    if (StringWrites.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> set in StringWrites)
                        {
                            Utilities.Logging.WriteLog("checkForTagWrites StringWrites data to write: " + set.Key, "OPCTagHandler", threadname, Logging.LogLevel.Debug);
                            string res = WriteTagString(set.Key, set.Value);
                            res = res.Replace("|", "").Trim();
                            if (res == "Good Quality")
                            {
                                lwq.WriteLog(loglevels, "checkForTagWrites StringWrites data to write: " + set.Key + " good write", "OPCTagHandler", threadname, Logging.LogLevel.Debug);
                            }
                            else
                            {
                                lwq.WriteLog(loglevels, "Error: String Write failed for " + set.Key + ". Write result: " + res, "OPCTagHandler", threadname, Logging.LogLevel.Warning);
                            }
                            tagsToRemove.Add(set.Key);
                        }
                    }

                    // remove tags that have been successfully written

                    foreach (string tag in tagsToRemove)
                    {
                        if (IntWrites.ContainsKey(tag))
                        {
                            IntWrites.Remove(tag);
                        }
                        if (FloatWrites.ContainsKey(tag))
                        {
                            FloatWrites.Remove(tag);
                        }
                        if (StringWrites.ContainsKey(tag))
                        {
                            StringWrites.Remove(tag);
                        }

                    }

                    WriteTimeStampToOPC();

                    canAddTags = true; // unlock tags
                }
                
            }
            catch (Exception esd)
            {
                string emsg = "Exception:" + esd.Message;
                if (esd.InnerException != null)
                {
                    emsg += " inner: " + esd.InnerException.Message;
                }
                Utilities.Logging.WriteLog(emsg, "OPCTagHandler- checkForTagWrites", threadname, Logging.LogLevel.Exception);
            }
        }

        /// <summary>
        /// Writes the time stamp to opc.
        /// </summary>
        private void WriteTimeStampToOPC()
        {
            string GPUTimeStampTag = ConfigurationManager.AppSettings["GPUTimeStampTag"];
            string res = WriteTagInt(GPUTimeStampTag, Convert.ToInt32(DateTime.Now.ToString("mm")));
            res = res.Replace("|", "").Trim();
            if (res == "Good Quality")
            {
                lwq.WriteLog(loglevels, "WriteTimeStampToOPC good write", "OPCTagHandler", threadname, Logging.LogLevel.Debug);
            }
            else
            {
                lwq.WriteLog(loglevels, "WriteTimeStampToOPC failed. Write result: " + res, "OPCTagHandler", threadname, Logging.LogLevel.Warning);
            }
        }

        /// <summary>
        /// write tag using OPC - string data value - non lib specific
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="tagvalue"></param>
        /// <returns></returns>
        private string WriteTagString(string tagname, string tagvalue)
        {
            return _jbtopc.WriteTag(tagname, tagvalue);
        }


        /// <summary>
        /// write tag using OPC - double data value - non lib specific
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="tagvalue"></param>
        public string WriteTagDouble(string tagname ,double tagvalue)
        {
            return _jbtopc.WriteTag(tagname, tagvalue);
        }

        /// <summary>
        /// write tag using OPC - int data value - non lib specific
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="tagvalue"></param>
        public string WriteTagInt(string tagname, int tagvalue)
        {
            return _jbtopc.WriteTag(tagname, tagvalue);            
        }

        
    }
}
