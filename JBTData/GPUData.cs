using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBTData
{
    public class GPURealTimeData 
    {
        private string InputString { get; set; }
        public string Pre;// always R
        public string VinØA;
        public string VinØB;
        public string VinØC;
        public string VinAvg;
        public string Fault;
        public string VoutØA;
        public string VoutØB;
        public string VoutØC;
        public string VoutAvg;
        public string Freq;
        public string AoutØA;
        public string AoutØB;
        public string AoutØC;
        public string AoutAvg;
        public string kVA;
        public string AoCont1;
        public string AoCont2;
        public string AinAvg;
        public string AkWh;
        public string Status;
        public string kVAset;
        public string DCVolts;
        public string DCAmp;

        public GPURealTimeData(string input, string doNotTranslateTags)
        {
            try
            {
                //
                InputString = input;
                Pre = InputString.Substring(0, 1);
                VinØA = InputString.Substring(1, 3);
                VinØB = InputString.Substring(4, 3);
                VinØC = InputString.Substring(7, 3);
                VinAvg = InputString.Substring(10, 3);
                Fault = (doNotTranslateTags == "true" ? InputString.Substring(13, 1) : GetFault(InputString.Substring(13, 1)));
                VoutØA = InputString.Substring(14, 3);
                VoutØB = InputString.Substring(17, 3);
                VoutØC = InputString.Substring(20, 3);
                VoutAvg = InputString.Substring(23, 3);
                Freq = InputString.Substring(26, 3);
                AoutØA = InputString.Substring(29, 3);
                AoutØB = InputString.Substring(32, 3);
                AoutØC = InputString.Substring(35, 3);
                AoutAvg = InputString.Substring(38, 3);
                kVA = InputString.Substring(41, 3);
                AoCont1 = InputString.Substring(44, 3);
                AoCont2 = InputString.Substring(47, 3);
                AinAvg = InputString.Substring(50, 3);
                AkWh = InputString.Substring(53, 3);
                Status = (doNotTranslateTags == "true" ? InputString.Substring(56, 1) : GetStatus(InputString.Substring(56, 1)));
                kVAset = (doNotTranslateTags == "true" ? InputString.Substring(57, 1) : GetKVASetting(InputString.Substring(57, 1)));
                DCVolts = InputString.Substring(58, 3);
                DCAmp = InputString.Substring(61, 4);
            }
            catch (Exception esd)
            {
                //
            }
        }

        /// <summary>
        /// get kva setting form parameter
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string GetKVASetting(string input)
        {
            string res = "";
            switch (input)
            {
                case "0":
                    res = "45 kVA w/ single output";
                    break;
                case "1":
                    res = "60 kVA w/ single output";
                    break;
                case "2":
                    res = "90 kVA w/ single output";
                    break;
                case "3":
                    res = "120 kVA w/ single output";
                    break;
                case "4":
                    res = "140 kVA w/ single output";
                    break;
                case "5":
                    res = "180 kVA w/ single output";
                    break;
                case "6":
                    res = "200 kVA w/ single output";
                    break;
                case "7":
                    res = "312 kVA w/ single output";
                    break;
                case "8":
                    res = "45 kVA w/ dual output";
                    break;
                case "9":
                    res = "60 kVA w/ dual output";
                    break;
                case ":":
                    res = "90 kVA w/ dual output";
                    break;
                case ";":
                    res = "120 kVA w/ dual output";
                    break;
                case "<":
                    res = "140 kVA w/ dual output";
                    break;
                case "=":
                    res = "180 kVA w/ dual output";
                    break;
                case ">":
                    res = "200 kVA w/ dual output";
                    break;
                case "?":
                    res = "312 kVA w/ dual output";
                    break;
                default:
                    res = "kVA Setting key unknown:" + input;
                    break;
            }
            return res;
        }

        /// <summary>
        /// get status from indicator
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string GetStatus(string input)
        {
            string res = "";
            switch (input)
            {
                case "0":
                    res = "45 kVA w/ single output";
                    break;
                case "1":
                    res = "60 kVA w/ single output";
                    break;
                case "2":
                    res = "90 kVA w/ single output";
                    break;
                case "3":
                    res = "120 kVA w/ single output";
                    break;
                case "4":
                    res = "140 kVA w/ single output";
                    break;
                case "5":
                    res = "180 kVA w/ single output";
                    break;
                case "6":
                    res = "200 kVA w/ single output";
                    break;
                case "7":
                    res = "312 kVA w/ single output";
                    break;
                case "8":
                    res = "45 kVA w/ dual output";
                    break;
                case "9":
                    res = "60 kVA w/ dual output";
                    break;
                case ":":
                    res = "90 kVA w/ dual output";
                    break;
                case ";":
                    res = "120 kVA w/ dual output";
                    break;
                case "<":
                    res = "140 kVA w/ dual output";
                    break;
                case "=":
                    res = "180 kVA w/ dual output";
                    break;
                case ">":
                    res = "200 kVA w/ dual output";
                    break;


                default:
                    res = "status key unknown:" + input;
                    break;
            }
            return res;
        }

        /// <summary>
        /// get fault from indicator
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string GetFault(string input)
        {
            string res = "";
            switch (input)
            {
                case "0":
                    res = "Unit off, out of Bypass, no contacts in use.";
                    break;
                case "1":
                    res = "Unit off, in Bypass, no contacts in use.";
                    break;
                case "2":
                    res = "Unit on, out of Bypass, no contacts in use.";
                    break;
                case "3":
                    res = "Unit on, in Bypass, no contacts in use.";
                    break;
                case "4":
                    res = "Unit off, out of Bypass, number two contact in use.";
                    break;
                case "5":
                    res = "Unit off, in Bypass, number two contact in use.";
                    break;
                case "6":
                    res = "Unit on, out of Bypass, number two contact in use.";
                    break;
                case "7":
                    res = "Unit on, in Bypass, number two contact in use.";
                    break;
                case "8":
                    res = "Unit off, out of Bypass, number one contact in use.";
                    break;
                case "9":
                    res = "Unit off, in Bypass, number one contact in use.";
                    break;
                case ":":
                    res = "Unit on, out of Bypass, number one contact in use.";
                    break;
                case ";":
                    res = "Unit on, in Bypass, number one contact in use.";
                    break;
                case "<":
                    res = "Unit off, out of Bypass, both contacts in use.";
                    break;
                case "=":
                    res = "Unit off, in Bypass, both contacts in use.";
                    break;
                case ">":
                    res = "Unit on, out of Bypass, both contacts in use.";
                    break;
                case "?":
                    res = "Unit on, in Bypass, both contacts in use.";
                    break;


                default:
                    res = "fault key unknown:" + input;
                    break;
            }
            return res;
        }
    }
    public class GPUEventHistory 
    {
        private string InputString { get; set; }
        public string Pre;// always @
        public string Header;
        public string Fault;
        public string CkWh;
        public string Month;
        public string Day;
        public string Hour;
        public string Min;
        public string VoMax;
        public string AoMax;
        public string VoLast;
        public string AoLast;
        public string DCA;
        
        /// <summary>
        /// translate the data 
        /// </summary>
        /// <param name="input"></param>
        public GPUEventHistory(string input , string doNotTranslateTags)
        {
            //
            InputString = input;
            Pre = InputString.Substring(0, 1);
            Header = (doNotTranslateTags == "true" ? InputString.Substring(1, 1) : GetHeader(InputString.Substring(1, 1)));
            Fault = (doNotTranslateTags == "true" ? InputString.Substring(2, 1) : GetFault(InputString.Substring(2, 1)));
            CkWh = InputString.Substring(3, 4);
            Month = InputString.Substring(7, 2);
            Day = InputString.Substring(9, 2);
            Hour = InputString.Substring(11, 2);
            Min = InputString.Substring(13, 2);
            VoMax = InputString.Substring(15, 3);
            AoMax = InputString.Substring(18, 3);
            VoLast = InputString.Substring(21, 3);
            AoLast = InputString.Substring(23, 3);
            DCA = InputString.Substring(26, 4);
        }

        /// <summary>
        /// get fault from indicator
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string GetFault(string input)
        {
            string res = "";
            switch (input)
            {
                case "0":
                    res = "No fault.";
                    break;

                case "1":
                    res = "Input volts too high.";
                    break;

                case "2":
                    res = "Input volts too low.";
                    break;

                case "3":
                    res = "400Hz Output voltage less than 100V for 5 sec.";
                    break;

                case "4":
                    res = "400Hz Output overvoltage.";
                    break;

                case "5":
                    res = "400Hz Output overload.";
                    break;

                case "6":
                    res = "No E&F present.";
                    break;

                case "7":
                    res = "Neutral fault.";
                    break;

                case "8":
                    res = "DC Bus fault.";
                    break;

                case "9":
                    res = "400Hz Contactor fault.";
                    break;

                case ":":
                    res = "Over-temperature fault.";
                    break;

                case ";":
                    res = "400Hz Frequency fault.";
                    break;

                case "<":
                    res = "IGBT fault.";
                    break;

                case "=":
                    res = "No output voltage detected fault.";
                    break;

                case ">":
                    res = "28VDC Output Fault (28VDC units).";
                    break;

                case "?":
                    res = "Unit Overload Fault (28VDC units).";
                    break;

                default:
                    res = "fault key unknown:" + input;
                    break;
            }
            return res;
        }

        /// <summary>
        /// extract header data
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        private string GetHeader(string header)
        {
            string res = "";
            switch (header)
            {
                case "0":
                    res = "	Start with no contactors used.";
                    break;
                case "1":
                    res = "	Start with number one contactor used.";
                    break;
                case "2":
                    res = "	Start with number two contactor used.";
                    break;
                case "3":
                    res = "	Start with both contactors used.";
                    break;
                case "4":
                    res = "	Fault with no contactors used.";
                    break;
                case "5":
                    res = "	Fault with number one contactor used.";
                    break;
                case "6":
                    res = "	Fault with number two contactor used.";
                    break;
                case "7":
                    res = "	Fault with both contactors used.";
                    break;
                case "8":
                    res = "	Stop with no contactors used.";
                    break;
                case "9":
                    res = "	Stop with number one contactor used.";
                    break;
                case ":":
                    res = "	Stop with number two contactor used.";
                    break;
                case ";":
                    res = "	Stop with both contactors used.";
                    break;
                case "<":
                    res = "	Reset with no contactors used.";
                    break;
                case "=":
                    res = "	Reset with number one contactor used.";
                    break;
                case ">":
                    res = "	Reset with number two contactor used.";
                    break;
                case "?":
                    res = "	Reset with both contactors used.";
                    break;
                default:
                    res = "heaader key unknown:" + header;
                    break;
            }
            return res;
        }

    }

    public class GPUData 
    {
        public List<GPUEventHistory> InputQueueGPUEventHistory = new List<GPUEventHistory>();
        public List<GPURealTimeData> InputQueueGPURealTimeData = new List<GPURealTimeData>();
        private string doNotTranslateTags = "";

        public GPUData()
        {
            doNotTranslateTags = ConfigurationManager.AppSettings["doNotTranslateTags"];
        }

        public void ProcessData(string input)
        {
            try
            {
                if (!string.IsNullOrEmpty(input))
                {
                    if (input.Length > 2)
                    {
                        string mType = input.Substring(0, 1);
                        switch (mType)
                        {
                            case "@":// Event History
                                GPUEventHistory iqeh = new GPUEventHistory(input, doNotTranslateTags);
                                InputQueueGPUEventHistory.Add(iqeh);
                                break;
                            case "R":// Real Time Data
                                GPURealTimeData iqrd = new GPURealTimeData(input, doNotTranslateTags);
                                InputQueueGPURealTimeData.Add(iqrd);
                                break;
                            default:
                                // unknown type
                                break;
                        }
                    }
                }
            }
            catch (Exception esd)
            {
                //
            }
        }
        
    }
}
