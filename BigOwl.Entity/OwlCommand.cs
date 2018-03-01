using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace BigOwl.Entity
{
    public class OwlCommand
    {
        public enum Commands
        {
            Rest,
            Flap,
            Wink,
            HeadRight,
            HeadLeft,
            SmallWiggle,
            RandomShort,
            RandomLong,
            RandomFull,
            CancelAllAndReset
        }
        public Guid Id { get; set; }
        public string SourceAppId { get; set; }
        public DateTime CreatedDateTime { get; set; }

        public Commands Command { get; set; }
        public bool StayAfterComplete { get; set; }
        public OwlState State { get; set; }
    }
}
