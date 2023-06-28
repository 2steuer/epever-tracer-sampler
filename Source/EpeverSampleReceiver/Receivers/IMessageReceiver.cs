using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpeverSampleReceiver.Receivers
{
    internal interface IMessageReceiver
    {
        public void HandleMessage(string payload);
    }
}
