using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Configuration;

namespace Utilities
{
    /*
     * modified code taken from https://msdn.microsoft.com/en-us/library/bew39x2a(v=vs.110).aspx
     */


    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket.
        public volatile Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public volatile byte[] buffer = new byte[BufferSize];
        // Received data string.
        public volatile StringBuilder sb = new StringBuilder();
    }

    public class AsyncSocketClient
    {
        // The ip and port number for the remote device.
        public int port = 11000;
        public string ip;
        private string threadname="no";
        public List<List<byte>> ResultList = new List<List<byte>>();
        public bool terminate = false;
        public bool terminateThread = false;
        public bool cleanList = false;
        // ManualResetEvent instances signal completion.
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        private LoggingWithQueue lwq = new LoggingWithQueue();
        public string loglevels = "warning|exception";

        public AsyncSocketClient(string thrdname)
        {
            this.threadname = thrdname;
        }

        /// <summary>
        /// Saves the log.
        /// </summary>
        public void saveLog()
        {
            lwq.SaveLogs();
        }

        /// <summary>
        /// start client - entry point 
        /// </summary>
        public void StartClient()
        {
            
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                IPAddress ipAddress = IPAddress.Parse(ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.
                Socket client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();

                terminate = checkForTermination();
                while (terminate == false)// check if app terminates
                {
                    lwq.WriteLog(loglevels, "Check for received data", "AsyncSocketClient.StartClient", threadname, Logging.LogLevel.Debug);
                    // Receive data from the remote device.
                    Receive(client);
                    receiveDone.WaitOne();
                    lwq.WriteLog(loglevels, "Receive data from the remote device.. done", "AsyncSocketClient.StartClient", threadname, Logging.LogLevel.Debug);
                    terminate = checkForTermination();
                }                

                // Release the socket.
                client.Shutdown(SocketShutdown.Both);
                client.Close();

            }
            catch (Exception e)
            {
                Utilities.Logging.WriteLog("Exception: " + e.ToString(), "AsyncSocketClient.StartClient",threadname, Logging.LogLevel.Exception);
            }
        }

        /// <summary>
        /// check for process termination
        /// </summary>
        /// <returns></returns>
        private bool checkForTermination()
        {
            return terminateThread;
        }



        /// <summary>
        /// connect socket
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                string msg = "Socket connected to {0}" + client.RemoteEndPoint.ToString();
                lwq.WriteLog(loglevels, msg, "AsyncSocketClient.ConnectCallback", threadname, Logging.LogLevel.Debug);

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Utilities.Logging.WriteLog("Exception: "+e.ToString(), "AsyncSocketClient.ConnectCallback", threadname, Logging.LogLevel.Exception);
            }
        }


        /// <summary>
        /// receive data - start to receive
        /// </summary>
        /// <param name="client"></param>
        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;
                string msg = "Socket receive from  {0}" + client.RemoteEndPoint.ToString();
                lwq.WriteLog(loglevels,msg, "AsyncSocketClient.Receive", threadname, Logging.LogLevel.Debug);
                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Utilities.Logging.WriteLog("Exception: " + e.ToString(), "AsyncSocketClient.Receive", threadname, Logging.LogLevel.Exception);
            }
        }

        /// <summary>
        /// process message when received done event happens
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            string msg = "entering ReceiveCallback";
            lwq.WriteLog(msg, "AsyncSocketClient.ReceiveCallback", threadname, Logging.LogLevel.Debug);
            try
            {
                //Utilities.Logging.WriteLog("ReceiveCallback entered", "AsyncSocketClient.ReceiveCallback", threadname, Logging.LogLevel.Info);
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                
                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);
                msg = "ReceiveCallback bytesread = "+bytesRead.ToString();
                lwq.WriteLog(loglevels, msg, "AsyncSocketClient.ReceiveCallback", threadname, Logging.LogLevel.Debug);

                if (bytesRead > 0)
                {
                    byte[] res = state.buffer.ToArray();
                    List<byte> sample = new List<byte>(res);
                    string result = System.Text.Encoding.UTF8.GetString(res).ToString();


                    string doReverse = ConfigurationManager.AppSettings["byteStringReverse"];
                    lwq.WriteLog(loglevels, "reverse=" +doReverse, "AsyncSocketClient.ReceiveCallback", threadname, Logging.LogLevel.Debug);
                    if (doReverse == "true")
                    {
                        msg = "reversing array per configuration";
                        lwq.WriteLog(loglevels, msg, "AsyncSocketClient.ReceiveCallback", threadname, Logging.LogLevel.Debug);
                        // reverse received data order
                        sample.Reverse();
                    }

                    List<List<byte>> results = new List<List<byte>>();

                    if (sample.Contains(64))// event history
                    {// eventhistory start @(64) length 31
                        byte searchval = 64;
                        List<int> keyIndexes = Utilities.ListsAndCollections.GetSampleIndexes(sample, searchval);
                        int keyListLength = keyIndexes.Count;
                                                
                        for (int i = keyListLength - 1; i >= 0; i--)// rolling it off form the back
                        {
                            int startindex = keyIndexes[i];
                            int endindex = startindex + 31;
                            if (endindex > sample.Count - 1)
                            {
                                // too short
                                continue;
                            }
                            else
                            {
                                // good
                                results.Add(new List<byte>(sample.GetRange(startindex, 31)));
                                sample.RemoveRange(startindex, 31); // no problem to remove the set
                                                                    // since we are indexing from the end
                            }
                        }
                    }
                    results.Reverse();
                    foreach (List<byte> set in results)
                    {
                        RecordResult(set);
                    }

                    
                    if (sample.Contains(82) || sample.Contains(83))//real time data
                    {// realtime     start R(82) length 66
                        // or S (83)
                        msg = "ReceiveCallback bytes[0] = " + sample[0].ToString() + " bytes[end] = " + sample[sample.Count - 1].ToString() + " length : " + sample.Count +" data: " + Utilities.ListsAndCollections.ConvertByteArrayToString(sample);
                        lwq.WriteLog(msg, "AsyncSocketClient.ReceiveCallback", threadname, Logging.LogLevel.Debug);
                        byte searchval = 82;
                        List<int> keyIndexes = Utilities.ListsAndCollections.GetSampleIndexes(sample, searchval);


                        //int keyListLength = keyIndexes.Count;
                        if (sample.Count >= 68)
                        {
                            //
                            
                            List<byte> s = new List<byte>(sample.GetRange(0, 68));
                            results.Add(s);
                            msg = "added to results : " + Utilities.ListsAndCollections.ConvertByteArrayToString(s);
                            lwq.WriteLog(loglevels, msg, "AsyncSocketClient.ReceiveCallback", threadname, Logging.LogLevel.Data);
                        }
                        else
                        {
                            //
                            msg = "string too short";
                            lwq.WriteLog(loglevels, msg, "AsyncSocketClient.ReceiveCallback", threadname, Logging.LogLevel.Debug);
                        }
                        //for (int i=keyListLength -1;i >=0; i--)// rolling it off form the back
                        //{
                        //    int startindex = keyIndexes[i];
                        //    int endindex = startindex + 66;
                        //    if (endindex > sample.Count-1)
                        //    {
                        //        // too short
                        //        continue;
                        //    }
                        //    else
                        //    {
                        //        // good
                        //        results.Add(new List<byte>(sample.GetRange(startindex, 66)));
                        //        sample.RemoveRange(startindex, 66); // no problem to remove the set
                        //                                            // since we are indexing from the end
                        //    }
                        //}
                    }
                    results.Reverse();
                    foreach (List<byte> set in results)
                    {
                        RecordResult(set);
                    }

                    if (results.Count == 0)
                    {
                        // not suitable
                        string dataReceived = Utilities.ListsAndCollections.ConvertByteArrayToString(sample);
                        lwq.WriteLog(loglevels, "ReceiveCallback bytes did not contain keys. Data:" +dataReceived , "AsyncSocketClient.ReceiveCallback", threadname, Logging.LogLevel.Debug);

                    }

                    terminate = checkForTermination();
                    if (terminate)
                    {
                        msg = "received termination";
                        lwq.WriteLog(loglevels, msg, "AsyncSocketClient.ReceiveCallback", threadname, Logging.LogLevel.Info);
                        receiveDone.Set();
                        return;
                    }
                    //// There might be more data, so store the data received so far.
                    //state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    Utilities.Logging.WriteLog("ReceiveCallback bytes read:0 ", "AsyncSocketClient.ReceiveCallback", threadname, Logging.LogLevel.Debug);
                    // All the data has arrived; put it in response.
                    //if (state.sb.Length > 1)
                    //{
                    //    RecordResult(state.sb.ToString());
                    //}
                    // Signal that all bytes have been received.
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Utilities.Logging.WriteLog("Exception: " + e.ToString(), "AsyncSocketClient.ReceiveCallback", threadname, Logging.LogLevel.Exception);
            }
        }

        

        /// <summary>
        /// record received data
        /// </summary>
        /// <param name="v"></param>
        private void RecordResult(List<byte> v)
        {            
            if (cleanList == true)
            {
                ResultList = new List<List<byte>>();
                cleanList = false;
            }
            ResultList.Add(v);
            lwq.WriteLog(loglevels, "recording Data length : " + v.Count + " to result list with list length:" +ResultList.Count , "AsyncSocketClient.RecordResult", threadname, Logging.LogLevel.Debug);
        }


        /// <summary>
        /// send data
        /// </summary>
        /// <param name="client"></param>
        /// <param name="data"></param>
        private void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);

            string msg = "Send data: " + data;
            lwq.WriteLog(loglevels, msg, "AsyncSocketClient.Send", threadname, Logging.LogLevel.Debug);
        }


        /// <summary>
        /// send done
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);
                string msg = "Send data completed: bytes sent = " + bytesSent.ToString();
                lwq.WriteLog(loglevels, msg, "AsyncSocketClient.SendCallback", threadname, Logging.LogLevel.Debug);
                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Utilities.Logging.WriteLog("Exception: " + e.ToString(), "AsyncSocketClient.SendCallback", threadname, Logging.LogLevel.Exception);
            }
        }
    }
}
