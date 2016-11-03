using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime.InteropServices;
using System.Configuration;
using static Utilities.Logging;
using Utilities;
using JBTData;
using System.Threading;

namespace JBTCDataPort
{
    public partial class JBTCDataPortService : ServiceBase
    {
        #region service status 
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);


        /// <summary>
        /// serv status eum
        /// </summary>
        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        /// <summary>
        /// iterop interf.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public long dwServiceType;
            public ServiceState dwCurrentState;
            public long dwControlsAccepted;
            public long dwWin32ExitCode;
            public long dwServiceSpecificExitCode;
            public long dwCheckPoint;
            public long dwWaitHint;
        };

        #endregion

        private Dictionary<string, AsyncSocketClient> asyncClients = new Dictionary<string, AsyncSocketClient>();
        private Dictionary<string, OPCTagHandler> opcHandlers = new Dictionary<string, OPCTagHandler>();
        private Dictionary<string, string> opcTagInfo = new Dictionary<string, string>();
        private string OPCServerName;
        private int sleepTimeBetweenOPCTagWritesInSecs;
        private List<Thread> allThreads = new List<Thread>();

        /// <summary>
        /// Initializes a new instance of the <see cref="JBTCDataPortService"/> class.
        /// </summary>
        public JBTCDataPortService()
        {
            InitializeComponent();
            #region event log
            // event log update
            //evtJBTCDataPort = new System.Diagnostics.EventLog();
            //if (!System.Diagnostics.EventLog.SourceExists("evtJBTCDataPortSource"))
            //{
            //    System.Diagnostics.EventLog.CreateEventSource(
            //        "evtJBTCDataPortSource", "evtJBTCDataPortLog");
            //}
            //evtJBTCDataPort.Source = "evtJBTCDataPortSource";
            //evtJBTCDataPort.Log = "evtJBTCDataPortLog";
            #endregion

        }
        
        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            try
            {
                #region system status - start pending
                // Update the service state to Start Pending.
                ServiceStatus serviceStatus = new ServiceStatus();
                serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
                serviceStatus.dwWaitHint = 100000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
                writeEventLog("JBTCDataPort starting", "OnStart", null, LogLevel.Info);
                #endregion

                #region biz logic
                writeEventLog("run biz logic prep and load configurations", "OnStart", null, LogLevel.Info);
                DoGPUOPCTagBusiness();
                //Thread bizWorkerThread = new Thread(DoGPUOPCTagBusiness);

                // Start the worker thread.
                //bizWorkerThread.Start();

                //writeEventLog("Starting bizWorkerThread ", "OnStart", null, LogLevel.Info);

                // Loop until worker thread activates. 
                //while (!bizWorkerThread.IsAlive) ;

                //writeEventLog("Starting bizWorkerThread ...done", "OnStart", null, LogLevel.Info);
                #endregion

                #region watchdog timer
                // Set up a timer to trigger every minute.
                System.Timers.Timer timer = new System.Timers.Timer();
                string watchDogTimerVal = ConfigurationManager.AppSettings["watchDogTimerVal"];
                timer.Interval = 60000; // 60 seconds
                timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
                timer.Start();
                #endregion

                #region system status - started
                // Update the service state to Running.
                serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
                #endregion
            }
            catch (Exception esd)
            {
                string msg = "Exception:" + esd.Message;
                if (esd.InnerException != null)
                {
                    msg += " inner: " + esd.InnerException.Message;
                }
                writeEventLog(msg, "OnStart", null, LogLevel.Exception);
            }
        }

        /// <summary>
        /// collect data from GPUs and write to OPC
        /// </summary>
        private void DoGPUOPCTagBusiness()
        {
            try
            {
                OPCServerName = ConfigurationManager.AppSettings["OPCServerName"];

                sleepTimeBetweenOPCTagWritesInSecs = Convert.ToInt32(ConfigurationManager.AppSettings["sleepTimeBetweenOPCTagWritesInSecs"]);

                #region test
                // test 
                //List<string> tagNames = new List<string>(ConfigurationManager.AppSettings["OPCServerTagNames"].Split('|'));


                //OPCTagHandler oth = new OPCTagHandler(tagNames, OPCServerName, sleepTimeBetweenOPCTagWritesInSecs);

                // OPC successful write !!!!!  works
                //oth.FloatWrites = new Dictionary<string, double>();
                //oth.FloatWrites.Add("Airport.RDU.Term2.ZoneC.GateC8.GPU.RAOUTA", 52.0);
                //oth.start();

                // data models for GPU data - works
                //JBTData.GPURealTimeData  gpuRealTimeData = new GPURealTimeData(inputstringfromSocket);
                //JBTData.GPUEventHistory gpuEventHistory = new GPUEventHistory(inputstringfromSocket);
                //JBTData.GPUData gpuData = new GPUData();
                //gpuData.ProcessData(inputStringFromSocket);
                // end test 
                #endregion
                
                switch (ConfigurationManager.AppSettings["mode"])
                {
                    case "socket":
                        {
                            #region socket connection
                            
                            #region opc tags
                            opcTagInfo = ConfigurationManager.AppSettings.AllKeys
                                        .Where(key => key.StartsWith("OPCServerTagNames$"))
                                        .ToDictionary(k => k, v => ConfigurationManager.AppSettings[v]);

                            #endregion

                            #region get network endpoints from configuration, prepare thread and start
                            string[] socketInfo = ConfigurationManager.AppSettings.AllKeys
                                                    .Where(key => key.StartsWith("SocketPort_"))
                                                    .Select(key => (ConfigurationManager.AppSettings[key] + "|" + key))
                                                    .ToArray();
                            string[] socketlogginginfo = ConfigurationManager.AppSettings.AllKeys
                                                    .Where(key => key.StartsWith("logLevelsP$"))
                                                    .Select(key => (ConfigurationManager.AppSettings[key] + "|" + key))
                                                    .ToArray();
                            Dictionary<string, string> portLoggingSettings = new Dictionary<string, string>();

                            foreach (string set in socketlogginginfo)
                            {
                                string[] keyVal = set.Split('$');
                                string sysname = keyVal[1];
                                string loglevels = keyVal[0].Replace("|logLevelsP", "");
                                portLoggingSettings.Add(sysname, loglevels);
                            }
                            int threadindex = 0;

                            foreach (string set in socketInfo)
                            {
                                threadindex++;
                                string[] keyVal = set.Split('|');
                                string sysname = keyVal[1];
                                string[] setinfo = keyVal[0].Split(':');
                                if (setinfo.Length != 2)
                                {                                    
                                    writeEventLog("Socket config is incorrect : " + set + " --> skipping set", "DoBusiness", null, LogLevel.Warning);
                                    continue;
                                }

                                int port = 4440;
                                // parse port config and on success start socket thread
                                if (int.TryParse(setinfo[1], out port))
                                {
                                    AsyncSocketClient asc = new AsyncSocketClient(sysname);
                                    asc.ip = setinfo[0];
                                    asc.port = port;
                                    asc.loglevels = portLoggingSettings[sysname];
                                    asyncClients.Add(sysname, asc);
                                    Thread socketWorkerThread = new Thread(asc.StartClient);

                                    // Start the worker thread.
                                    socketWorkerThread.Start();
                                    
                                    writeEventLog("Starting data worker thread for " + asc.ip, "DoBusiness", null, LogLevel.Info);

                                    // Loop until worker thread activates. 
                                    while (!socketWorkerThread.IsAlive) ;

                                    writeEventLog("Starting data worker thread...done", "DoBusiness", null, LogLevel.Info);

                                    // add socket thread to thread collection in order to be able to terminate thread at the end 
                                    allThreads.Add(socketWorkerThread);

                                    // open OPC connection                                                                        
                                    List<string> tagNames = getTags(sysname);
                                    writeEventLog("OPCConn start - adding tags (count = "+tagNames.Count.ToString()+")", "DoBusiness", null, LogLevel.Info);
                                    OPCTagHandler oth = new OPCTagHandler(sysname, tagNames, OPCServerName, sleepTimeBetweenOPCTagWritesInSecs);
                                    oth.loglevels = portLoggingSettings[sysname];
                                    opcHandlers.Add(sysname, oth);
                                    // create thread                               
                                    Thread opcWorkerThread = new Thread(oth.start);
                                    
                                    writeEventLog("OPCConn start...done", "DoBusiness", null, LogLevel.Info);
                                    // Start the worker thread.
                                    opcWorkerThread.Start();
                                    
                                    writeEventLog("Starting opc worker thread for " + sysname, "DoBusiness", null, LogLevel.Info);

                                    // Loop until worker thread activates. 
                                    while (!opcWorkerThread.IsAlive) ;
                                                                        
                                    writeEventLog("Starting opc worker thread...done", "DoBusiness", null, LogLevel.Info);

                                    // add opc thread to thread collection in order to terminate at the end
                                    allThreads.Add(opcWorkerThread);
                                    writeEventLog(sysname+" done", "DoBusiness", null, LogLevel.Info);
                                }
                                else
                                {                                    
                                    writeEventLog("Port info invalid. Address skipped. ->" + set, "DoBusiness", null, LogLevel.Warning);
                                }
                            }
                            #endregion

                            #region check for termination - disabled
                            //while (_continue)
                            //{
                            //    Thread.Sleep(500);
                            //    CheckForReads();                                
                            //}


                            //foreach (Thread workerObject in allThreads)
                            //{
                            //    // Use the Join method to block the current thread  
                            //    // until the object's thread terminates.
                            //    workerObject.Abort();
                            //    workerObject.Join();
                            //    writeEventLog("terminating data worker thread...", "DoBusiness", null, LogLevel.Info);
                            //}                            
                            #endregion
                            #endregion
                        }
                        break;

                    #region serial connection disabled
                    //case "comport":
                    //    {
                    //        #region serial connection
                    //        List<string> _portNames = new List<string>();
                    //        List<SerialPortClass> _ports = new List<SerialPortClass>();

                    //        // get all port of the machine
                    //        _portNames = SerialConfiguration.GetAvailablePorts();

                    //        // loop through the ports and add all configured ports to the running collection
                    //        // each configure port will start it's own thread to run on 
                    //        #region start read threads
                    //        foreach (string pName in _portNames)
                    //        {
                    //            Utilities.SerialPortClass port = new SerialPortClass();
                    //            port.UsePort = SerialConfiguration.isPortConfigured(pName);
                    //            if (port.UsePort)
                    //            {
                    //                port.Configure(pName);
                    //                _ports.Add(port);                                    
                    //                writeEventLog("Added port " + pName, "DoBusiness", null, LogLevel.Debug);
                    //            }
                    //        }
                    //        #endregion

                    //        if (_ports.Count > 0)
                    //        {
                    //            #region check for termination
                    //            while (_continue)
                    //            {
                    //                Thread.Sleep(500);                                    
                    //            }

                    //            foreach (SerialPortClass sp in _ports)
                    //            {
                    //                // terminate thread
                    //                sp._continue = _continue;

                    //            }                                
                    //            #endregion
                    //        }
                    //        else
                    //        {                                
                    //            writeEventLog("No ports configured.", "DoBusiness", null, LogLevel.Info);
                    //        }
                    //        #endregion
                    //    }
                    //    break;
                    #endregion

                    default:
                        break;
                }
            }
            catch (Exception esd)
            {
                string msg = "Exception:" + esd.Message;
                if (esd.InnerException != null)
                {
                    msg += " inner: " + esd.InnerException.Message;
                }
                writeEventLog(msg, "DoBusiness", null, LogLevel.Exception);
            }
        }
        
        /// <summary>
        /// Called when [timer].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void OnTimer(object sender, ElapsedEventArgs e)
        {
            try
            {
                // watch dog logic
                writeEventLog("WatchDog timer elapsed - check for reads", "OnTimer", null, LogLevel.Info);
                CheckForReads();
                foreach (KeyValuePair<string, AsyncSocketClient> ascsw in asyncClients)
                {
                    ascsw.Value.saveLog();
                }
                foreach (KeyValuePair<string,OPCTagHandler>opcset in opcHandlers)
                {
                    opcset.Value.saveLog();
                }
            }
            catch (Exception esd)
            {
                string msg = "Exception:" + esd.Message;
                if (esd.InnerException != null)
                {
                    msg += " inner: " + esd.InnerException.Message;
                }
                writeEventLog(msg, "OnTimer", null, LogLevel.Exception);
            }


        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                writeEventLog("JBTCDataPort stopping", "OnStop", null, LogLevel.Info);
//                _continue = false;
                foreach (Thread workerObject in allThreads)
                {
                    // Use the Join method to block the current thread  
                    // until the object's thread terminates.
                    workerObject.Abort();
                    workerObject.Join();
                }
            }
            catch (Exception esd)
            {
                string msg = "Exception:" + esd.Message;
                if (esd.InnerException != null)
                {
                    msg += " inner: " + esd.InnerException.Message;
                }
                writeEventLog(msg, "OnStop", null, LogLevel.Exception);
            }
        }

        /// <summary>
        /// write event log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="name"></param>
        /// <param name="thread"></param>
        /// <param name="logLevel"></param>
        private void writeEventLog(string message, string functionName, string thread = "main", LogLevel logLevel = LogLevel.Info)
        {
            string loglevelname = LoglevelToString(logLevel);
            //string testtimestamp = DateTime.Now.ToString();
            string eventmessage = functionName + " #### " + message + " ####";


            Logging.WriteLog(message, functionName, thread, logLevel);
            //Utilities.EventLogLogging evtlg = new Utilities.EventLogLogging(evtJBTCDataPort);// TODO: move event log into Utility lib completely
            //evtlg.WriteEntry(eventmessage, logLevel);
            //evtJBTCDataPort.WriteEntry(eventmessage);
        }

        /// <summary>
        /// record results
        /// </summary>
        private void CheckForReads()
        {
            try
            {

                List<string> sysWritten = new List<string>();

                foreach (KeyValuePair<string, AsyncSocketClient> item in asyncClients)
                {
                    string sysname = item.Key;

                    if (item.Value.ResultList != null)
                    {
                        List<List<byte>> results = item.Value.ResultList;
                        if (results.Count > 0)
                        {
                            foreach (List<byte> res in results)
                            {
                                JBTData.GPUData gpuData = new GPUData();
                                string stringRes = ListsAndCollections.ConvertByteArrayToString(res);
                                gpuData.ProcessData(stringRes);
                                writeOPCTags(sysname, gpuData);
                                Utilities.Logging.WriteLog(sysname + " - result received: " + stringRes, "Main", null, Logging.LogLevel.Data);
                            }
                        }
                    }
                    sysWritten.Add(sysname);
                }
                foreach (string syss in sysWritten)
                {
                    asyncClients[syss].cleanList = true;// this will do the below stated
                                                        //ResultList= new List<List<byte>>();   // empty list after processing
                                                        // reinitialize list rather than
                                                        // removing items individually
                                                        // OPC class takes care of retrying 
                                                        // writes automatically
                }
            }
            catch (Exception esd)
            {
                string msg = "Exception:" + esd.Message;
                if (esd.InnerException != null)
                {
                    msg += " inner: " + esd.InnerException.Message;
                }
                writeEventLog(msg, "CheckForReads", null, LogLevel.Exception);
            }
        }
        
        /// <summary>
        /// get all OPC tags based on sysname
        /// </summary>
        /// <param name="sysname"></param>
        /// <returns></returns>
        private List<string> getTags(string sysname)
        {
            //example : OPCServerTagNames$SocketPort_c25
            string tagskey = "OPCServerTagNames$" + sysname;
            List<string> tagNames = new List<string>(opcTagInfo[tagskey].Split('|'));
            return tagNames;
        }

        /// <summary>
        /// write tags detected after gpu data conversion
        /// map gpu data tags to opc tags
        /// </summary>
        /// <param name="sysname"></param>
        /// <param name="gpuData"></param>
        private void writeOPCTags(string sysname, GPUData gpuData)
        {
            try
            {
                Utilities.OPCTagHandler oth = opcHandlers[sysname];
                if (oth.canAddTags == false)
                {
                    //Utilities.Logging.WriteLog("writeOPCTags skipped - tags locked", "Main", null, Logging.LogLevel.Debug);
                }
                else
                {
                    #region real time data 
                    Utilities.Logging.WriteLog("writeOPCTags: gpudata intQueue length:" + gpuData.InputQueueGPURealTimeData.Count.ToString(), "Main", null, Logging.LogLevel.Data);

                    if (gpuData.InputQueueGPURealTimeData.Count > 0)
                    {
                        // write real time data
                        foreach (JBTData.GPURealTimeData set in gpuData.InputQueueGPURealTimeData)
                        {
                            #region old sample
                            // float VinØA	Airport.RDU.Term2.ZoneC.GateC15.GPU.RVINA
                            //if (oth.FloatWrites.ContainsKey("Airport.RDU.Term2.ZoneC.GateC15.GPU.RVINA") == false)
                            //{
                            //    oth.FloatWrites.Add("Airport.RDU.Term2.ZoneC.GateC15.GPU.RVINA", Convert.ToDouble(set.VinØA));
                            //}
                            //else
                            //{
                            //    oth.FloatWrites["Airport.RDU.Term2.ZoneC.GateC15.GPU.RVINA"] = Convert.ToDouble(set.VinØA);
                            //}
                            #endregion
                            // lineup tags with members TODO
                            string[] tagset = oth.tags;
                            foreach (string tag in oth.tags)
                            {
                                string[] parts = tag.Split('.');
                                string tagdetail = parts[parts.Length - 1];
                                Utilities.Logging.WriteLog("writeOPCTags - tagdetail:" + tagdetail, "Main", null, Utilities.Logging.LogLevel.Data);
                                switch (tagdetail)
                                {
                                    case "RDCVOLTS":
                                    case "400HZUNITFAULT":
                                        {
                                            int setvalue = 0;
                                            switch (tagdetail)
                                            {
                                                case "RDCVOLTS":
                                                    // int DCVolts	Airport.RDU.Term2.ZoneC.GateC15.GPU.RDCVOLTS
                                                    setvalue = Convert.ToInt32(set.DCVolts);
                                                    break;
                                                case "400HZUNITFAULT":
                                                    // boolean Fault	Airport.RDU.Term2.ZoneC.GateC15.GPU.Alarm.400HZUNITFAULT
                                                    setvalue = Convert.ToInt32(set.Fault);
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (oth.IntWrites.ContainsKey(tag) == false)
                                            {
                                                oth.IntWrites.Add(tag, setvalue);
                                            }
                                            else
                                            {
                                                oth.IntWrites[tag] = setvalue;
                                            }
                                            Utilities.Logging.WriteLog("writeOPCTags done - tagdetail:" + tagdetail, "Main", null, Logging.LogLevel.Data);
                                        }
                                        break;
                                    case "RVINA":
                                    case "RVINB":
                                    case "RVINC":
                                    case "RVINAVG":
                                    case "RVOUTA":
                                    case "RVOUTB":
                                    case "RVOUTC":
                                    case "RVOUTAVG":
                                    case "RFREQ":
                                    case "RAOUTA":
                                    case "RAOUTB":
                                    case "RAOUTC":
                                    case "RAOUTAVG":
                                    case "RKVA":
                                    case "RAOCONT2":
                                    case "RAKWH":
                                    case "RDCAMPS":
                                        {
                                            double setvalue = 0;
                                            switch (tagdetail)
                                            {
                                                case "RVINA":
                                                    // float VinØA	Airport.RDU.Term2.ZoneC.GateC15.GPU.RVINA
                                                    setvalue = Convert.ToDouble(set.VinØA);
                                                    break;
                                                case "RVINB":
                                                    // float VinØB	Airport.RDU.Term2.ZoneC.GateC15.GPU.RVINB
                                                    setvalue = Convert.ToDouble(set.VinØB);
                                                    break;
                                                case "RVINC":
                                                    // float VinØC	Airport.RDU.Term2.ZoneC.GateC15.GPU.RVINC
                                                    setvalue = Convert.ToDouble(set.VinØC);
                                                    break;
                                                case "RVINAVG":
                                                    // float VinAvg	Airport.RDU.Term2.ZoneC.GateC15.GPU.RVINAVG
                                                    setvalue = Convert.ToDouble(set.VinAvg);
                                                    break;
                                                case "RVOUTA":
                                                    // float VoutØA	Airport.RDU.Term2.ZoneC.GateC15.GPU.RVOUTA
                                                    setvalue = Convert.ToDouble(set.VoutØA);
                                                    break;
                                                case "RVOUTB":
                                                    // float VoutØB	Airport.RDU.Term2.ZoneC.GateC15.GPU.RVOUTB
                                                    setvalue = Convert.ToDouble(set.VoutØB);
                                                    break;
                                                case "RVOUTC":
                                                    // float VoutØC	Airport.RDU.Term2.ZoneC.GateC15.GPU.RVOUTC
                                                    setvalue = Convert.ToDouble(set.VoutØC);
                                                    break;
                                                case "RVOUTAVG":
                                                    // float VoAvg	Airport.RDU.Term2.ZoneC.GateC15.GPU.RVOUTAVG
                                                    setvalue = Convert.ToDouble(set.VoutAvg);
                                                    break;
                                                case "RFREQ":
                                                    // float Freq	Airport.RDU.Term2.ZoneC.GateC15.GPU.RFREQ
                                                    setvalue = Convert.ToDouble(set.Freq);
                                                    break;
                                                case "RAOUTA":
                                                    // float AoutØA	Airport.RDU.Term2.ZoneC.GateC15.GPU.RAOUTA
                                                    setvalue = Convert.ToDouble(set.AoutØA);
                                                    break;
                                                case "RAOUTB":
                                                    // float AoutØB	Airport.RDU.Term2.ZoneC.GateC15.GPU.RAOUTB
                                                    setvalue = Convert.ToDouble(set.AoutØB);
                                                    break;
                                                case "RAOUTC":
                                                    // float AoutØC	Airport.RDU.Term2.ZoneC.GateC15.GPU.RAOUTC
                                                    setvalue = Convert.ToDouble(set.AoutØC);
                                                    break;
                                                case "RAOUTAVG":
                                                    // float AoAvg	Airport.RDU.Term2.ZoneC.GateC15.GPU.RAOUTAVG
                                                    setvalue = Convert.ToDouble(set.AoutAvg);
                                                    break;
                                                case "RKVA":
                                                    // byte[] kVA	Airport.RDU.Term2.ZoneC.GateC15.GPU.RKVA
                                                    setvalue = Convert.ToDouble(set.kVA);
                                                    break;
                                                case "RAOCONT2":
                                                    // float AoCont2	Airport.RDU.Term2.ZoneC.GateC15GPU.RAOCONT2
                                                    setvalue = Convert.ToDouble(set.AoCont2);
                                                    break;
                                                case "RAKWH":
                                                    // AinAvg	N/A - not transmitted
                                                    // float AkWh	Airport.RDU.Term2.ZoneC.GateC15.GPU.RAKWH
                                                    setvalue = Convert.ToDouble(set.AkWh);
                                                    break;
                                                case "RDCAMPS":
                                                    // float DCAmp	Airport.RDU.Term2.ZoneC.GateC15.GPU.RDCAMPS
                                                    setvalue = Convert.ToDouble(set.DCAmp);
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (oth.FloatWrites.ContainsKey(tag) == false)
                                            {
                                                oth.FloatWrites.Add(tag, setvalue);
                                            }
                                            else
                                            {
                                                oth.FloatWrites[tag] = setvalue;
                                            }
                                        }
                                        Utilities.Logging.WriteLog("writeOPCTags done - tagdetail:" + tagdetail, "Main", null, Logging.LogLevel.Data);
                                        break;
                                    case "STATUS":
                                    case "KVASET":
                                    case "RAOCONT1":
                                        {
                                            string setvalue = "";
                                            switch (tagdetail)
                                            {
                                                case "STATUS":
                                                    // string Status	Airport.RDU.Term2.ZoneC.GateC15.GPU.GPUSTATUS
                                                    setvalue = set.Status;
                                                    break;
                                                case "KVASET":
                                                    // string kVAset	Airport.RDU.Term2.ZoneC.GateC15.GPU.KVASET_TEXT
                                                    setvalue = set.kVAset;
                                                    break;
                                                case "RAOCONT1":
                                                    // string AoCont1	Airport.RDU.Term2.ZoneC.GateC15.GPU.RAOCONT1
                                                    setvalue = set.AoCont1;
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (oth.StringWrites.ContainsKey(tag) == false)
                                            {
                                                oth.StringWrites.Add(tag, setvalue);
                                            }
                                            else
                                            {
                                                oth.StringWrites[tag] = setvalue;
                                            }

                                        }
                                        Utilities.Logging.WriteLog("writeOPCTags done - tagdetail:" + tagdetail, "Main", null, Logging.LogLevel.Debug);
                                        break;
                                    default:
                                        Utilities.Logging.WriteLog("writeOPCTags done - tag unknown :" + tagdetail, "Main", null, Logging.LogLevel.Debug);
                                        break;
                                }

                            }
                        }
                        gpuData.InputQueueGPURealTimeData = new List<GPURealTimeData>();// clear list
                    }
                    #endregion

                    #region event history
                    if (gpuData.InputQueueGPUEventHistory.Count > 0)
                    {
                        Utilities.Logging.WriteLog("writeOPCTags: gpuData.InputQueueGPUEventHistory.Count >0 : Not implemented yet in OPC ", "Main", null, Logging.LogLevel.Debug);
                        //foreach (GPUEventHistory set in gpuData.InputQueueGPUEventHistory)
                        //{
                        //    //TODO: this is just temporary code .. types will need to adjust to controls needs
                        //    //oth.StringWrites.Add("Airport.RDU.Term2.ZoneC.GateC15.GPU.Header", set.Header);
                        //    //oth.StringWrites.Add("Airport.RDU.Term2.ZoneC.GateC15.GPU.Fault", set.Fault);
                        //    //oth.StringWrites.Add("Airport.RDU.Term2.ZoneC.GateC15.GPU.CkWh", set.CkWh);
                        //    //oth.StringWrites.Add("Airport.RDU.Term2.ZoneC.GateC15.GPU.Month", set.Month);
                        //    //oth.StringWrites.Add("Airport.RDU.Term2.ZoneC.GateC15.GPU.Day", set.Day);
                        //    //oth.StringWrites.Add("Airport.RDU.Term2.ZoneC.GateC15.GPU.Hour", set.Hour);
                        //    //oth.StringWrites.Add("Airport.RDU.Term2.ZoneC.GateC15.GPU.Min", set.Min);
                        //    //oth.StringWrites.Add("Airport.RDU.Term2.ZoneC.GateC15.GPU.VoMax", set.VoMax);
                        //    //oth.StringWrites.Add("Airport.RDU.Term2.ZoneC.GateC15.GPU.AoMax", set.AoMax);
                        //    //oth.StringWrites.Add("Airport.RDU.Term2.ZoneC.GateC15.GPU.VoLast", set.VoLast);
                        //    //oth.StringWrites.Add("Airport.RDU.Term2.ZoneC.GateC15.GPU.AoLast", set.AoLast);
                        //    //oth.StringWrites.Add("Airport.RDU.Term2.ZoneC.GateC15.GPU.DCA", set.DCA);
                        //}
                        gpuData.InputQueueGPUEventHistory = new List<GPUEventHistory>();// clear list
                    }
                    #endregion

                    #region test
                    //oth.FloatWrites.Add("Airport.RDU.Term2.ZoneC.GateC8.GPU.RAOUTA", 52.0);
                    // OPC successful write !!!!!  works
                    //oth.FloatWrites = new Dictionary<string, double>();
                    //oth.FloatWrites.Add("Airport.RDU.Term2.ZoneC.GateC8.GPU.RAOUTA", 52.0);
                    #endregion


                }
            }
            catch (Exception esd)
            {
                string msg = "WriteOPC tags " + sysname + "Exception:" + esd.Message;
                if (esd.InnerException != null)
                {
                    msg += " inner: " + esd.InnerException.Message;
                }
                writeEventLog(msg, "Main", null, LogLevel.Exception);                
            }
        }
    }
}
