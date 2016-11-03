using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;

namespace Utilities
{
    public class SerialConfiguration
    {

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public static void GetConfiguration()
        {
            string configvalue2 = ConfigurationManager.AppSettings["logfilelocation"];
        }

        

        /// <summary>
        /// Determines whether [is port configured] [the specified port name].
        /// </summary>
        /// <param name="portName">Name of the port.</param>
        /// <returns></returns>
        public static bool isPortConfigured (string portName)
        {
            return (ConfigurationManager.AppSettings["BaudRate_" + portName] != null);
        }

        /// <summary>
        /// Gets the port baud rate.
        /// </summary>
        /// <param name="portName">Name of the port.</param>
        /// <returns></returns>
        public static int GetPortBaudRate(string portName)
        {
            
            if (ConfigurationManager.AppSettings["BaudRate_" + portName] != null)
            {
                return int.Parse(ConfigurationManager.AppSettings["BaudRate_" + portName]);
            }
            else
            {
                return 9600;
            }
        }

        /// <summary>
        /// Gets the port data bits.
        /// </summary>
        /// <param name="portName">Name of the port.</param>
        /// <returns></returns>
        public static int GetPortDataBits(string portName)
        {            
            if (ConfigurationManager.AppSettings["DataBits_" + portName] != null)
            {
                string s = ConfigurationManager.AppSettings["DataBits_" + portName];
                int res = 0;
                if (int.TryParse(s,out res))
                {
                    return res;
                }
                else
                {
                    return 8;
                }                
            }
            else
            {
                return 8;
            }
        }



        /// <summary>
        /// Gets the port handshake.
        /// </summary>
        /// <param name="portName">Name of the port.</param>
        /// <returns></returns>
        public static Handshake GetPortHandshake(string portName)
        {
            if (ConfigurationManager.AppSettings["HandShake_" + portName] != null)                
            {
                string handShake = ConfigurationManager.AppSettings["HandShake_" + portName];
                switch (handShake)
                {
                    case "NONE":
                        return Handshake.None;
                    case "REQUESTTOSEND":
                        return Handshake.RequestToSend;
                    case "REQUESTTOSENDXONXOFF":
                        return Handshake.RequestToSendXOnXOff;
                    case "XONXOFF":
                        return Handshake.XOnXOff;
                    default:
                        return Handshake.None;
                }
            }
            else
            {
                return Handshake.None;
            }
        }

        /// <summary>
        /// Gets the port controller number of heads.
        /// </summary>
        /// <param name="pName">Name of the p.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static int GetPortControllerNumberOfHeads(string pName)
        {
            return Convert.ToInt32(  ConfigurationManager.AppSettings["ControllerNumberOfHeads_" + pName]); 
        }

        /// <summary>
        /// enumerate over all available serialPorts and add to List
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAvailablePorts()
        {
            List<string> _portNames = new List<string>();
            foreach (string s in SerialPort.GetPortNames())
            {
                _portNames.Add(s);
            }
            return _portNames;
        }

        /// <summary>
        /// Gets the name of the port controller.
        /// </summary>
        /// <param name="pName">Name of the p.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static string GetPortControllerName(string pName)
        {
            return ConfigurationManager.AppSettings["ControllerName_" + pName];
        }

        /// <summary>
        /// Gets the port stop bits.
        /// </summary>
        /// <param name="portName">Name of the port.</param>
        /// <returns></returns>
        public static StopBits GetPortStopBits(string portName)
        {
            if (ConfigurationManager.AppSettings["StopBits_" + portName] != null)
            {
                string strStopBits = ConfigurationManager.AppSettings["StopBits_" + portName];
                switch (strStopBits)
                {
                    case "NONE":
                        return StopBits.None;
                    case "ONE":
                        return StopBits.One;
                    case "ONEPOINTFIVE":
                        return StopBits.OnePointFive;
                    case "TWO":
                        return StopBits.Two;
                    default:                        
                        return StopBits.One;
                }
            }
            else
            {
                return StopBits.One;
            }
        }

        /// <summary>
        /// Gets the port parity.
        /// </summary>
        /// <param name="portName">Name of the port.</param>
        /// <returns></returns>
        public static Parity GetPortParity(string portName)
        {
            if (ConfigurationManager.AppSettings["Parity_" + portName] != null)
            {
                string strParity = ConfigurationManager.AppSettings["Parity_" + portName];
                switch (strParity)
                {
                    case "NONE":
                        return Parity.None;
                    case "MARK":
                        return Parity.Mark;
                    case "EVEN":
                        return Parity.Even;
                    case "ODD":
                        return Parity.Odd;
                    case "SPACE":
                        return Parity.Space;
                    default:
                        return Parity.None;
                }
            }
            else
            {
                return Parity.None;
            }
        }

        /// <summary>
        /// Gets the write timeout.
        /// </summary>
        /// <param name="portName">Name of the port.</param>
        /// <returns></returns>
        public static int GetWriteTimeout(string portName)
        {
            if (ConfigurationManager.AppSettings["WriteTimeout_" + portName] != null)
            {
                string s = ConfigurationManager.AppSettings["WriteTimeout_" + portName];
                int res = 0;
                if (int.TryParse(s, out res))
                {
                    return res;
                }
                else
                {
                    return 500;
                }
            }
            else
            {
                return 500;
            }
        }

        /// <summary>
        /// Gets the read timeout.
        /// </summary>
        /// <param name="portName">Name of the port.</param>
        /// <returns></returns>
        public static int GetReadTimeout(string portName)
        {
            if (ConfigurationManager.AppSettings["ReadTimeout_" + portName] != null)
            {
                string s = ConfigurationManager.AppSettings["ReadTimeout_" + portName];
                int res = 0;
                if (int.TryParse(s, out res))
                {
                    return res;
                }
                else
                {
                    return 500;
                }
            }
            else
            {
                return 500;
            }
        }        
    }
}