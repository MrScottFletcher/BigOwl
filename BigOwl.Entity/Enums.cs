using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigOwl.Entity
{
    public class Enums
    {
        public enum MessageTypes
        {
            Command,
            Ack,
            Nack,
            Heartbeat,
            Terminating,
        }
    }
}
