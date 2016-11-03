using System;
using System.Configuration;
using System.IO.Ports;
using System.Threading;

namespace Utilities
{
    public class SerialPortClass
    {
        
        public SerialPort SerialPort;
        public bool UsePort;
        public bool _continue;
        public Thread readThread;
        public string portData;
        public string portControllerName;
        private int portControllerNoOfHeads;
        private string portName;
        
        private bool completeMessage = false;
        private bool logDataToFile;
        private string implementation;

        /// <summary>
        /// Gets the data logging configuration.
        /// </summary>
        /// <returns></returns>
        private bool GetDataLoggingConfiguration()
        {

            if (ConfigurationManager.AppSettings["LogData_" + portName] != null)
            {
                return (ConfigurationManager.AppSettings["LogData_" + portName] == "true" ? true : false);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// read content from port 
        /// </summary>
        /// <param name="_serialPort"></param>
        /// <returns></returns>
        public void Read()
        {
            string[] set = portControllerName.Split('.');
            bool isConfiguredPrimary = (set[1] == "A" ? true : false);
            implementation = ConfigurationManager.AppSettings["Implementation_"+portName];
            logDataToFile = GetDataLoggingConfiguration();
            while (_continue)
            {                                   
                    
                    Thread.Sleep(10000);
                
            }
            Utilities.Logging.WriteLog("Closing port " + portName, "SerialPortClass");
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="scn">The SCN.</param>
        private void SendResponse(string data)
        {            
            SerialPort.WriteLine(data);            
        }

        /// <summary>
        /// get port configuration from config file
        /// </summary>
        /// <param name="pName">Name of the p.</param>
        public void Configure(string pName)
        {
            portName = pName;
            _continue = false;
            readThread = new Thread(Read);
                        
            SerialPort = new SerialPort();
            SerialPort.PortName = pName;
            portControllerName = SerialConfiguration.GetPortControllerName(pName);
            portControllerNoOfHeads = SerialConfiguration.GetPortControllerNumberOfHeads(pName);
            SerialPort.BaudRate = SerialConfiguration.GetPortBaudRate(pName);
            SerialPort.Parity = SerialConfiguration.GetPortParity(pName);
            SerialPort.DataBits = SerialConfiguration.GetPortDataBits(pName);
            SerialPort.StopBits = SerialConfiguration.GetPortStopBits(pName);
            SerialPort.Handshake = SerialConfiguration.GetPortHandshake(pName);

            // Set the read/write timeouts
            //SerialPort.ReadTimeout = SerialConfiguration.GetReadTimeout(pName);//500;
            //SerialPort.WriteTimeout = SerialConfiguration.GetWriteTimeout(pName);//500;

            // event handler for data
            SerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            SerialPort.Open();
            _continue = true;
            readThread.Start();
        }

        /// <summary>
        /// Data received handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SerialDataReceivedEventArgs"/> instance containing the event data.</param>
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            // TODO - implement protocol






            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            portData += indata;
            
            String s = portData;
            if (logDataToFile)
            {
                Utilities.Logging.WriteLog(portData, "PortData_" + portName);
            }

            
            

        }
    }
}