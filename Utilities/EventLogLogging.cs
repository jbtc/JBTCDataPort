using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class EventLogLogging
    {
        private EventLog evtLog;

        public EventLogLogging(EventLog evtJBTCDataPort)
        {
            this.evtLog = evtJBTCDataPort;
        }

        public void WriteEntry(string eventmessage,Logging.LogLevel logLevel = Logging.LogLevel.Info)
        {
            string loglevelname = Logging.LoglevelToString(logLevel);
            string msg = "#### " + loglevelname + " #### " + eventmessage;
            evtLog.WriteEntry(msg);
        }
    }
}
