using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class EventHandling
    {
        public event EventHandler customEvent = new EventHandler((e, a) => { });

        protected void OnCustomEvent()
        {
            if (customEvent == null)
                return;
            customEvent(this, new EventArgs());
        }
    }
}
