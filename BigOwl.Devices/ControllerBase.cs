using BigOwl.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigOwl.Devices
{
    public abstract class ControllerBase
    {
        public string Name { get; set; }
        public string ControllerType { get; set; }
        public DateTime InitializedDateTime { get; set; }
        public string LastError { get; set; }
        public OwlDeviceStateBase.StatusTypes Status { get; set; }

        public bool StayEngagedAfterMove { get; set; }
        public bool ReturnHomeAfterMove { get; set; }
        public int DelaySecondsBeforeReturnHome { get; set; }

        public abstract bool Initialize();
        public abstract bool Enable();
        public abstract bool Disable();
        public abstract bool Shutdown();
        public abstract bool Recalibrate();
        public abstract bool GoHomePosition();

        public abstract OwlDeviceStateBase GetState();
        public abstract bool ApplyState(OwlDeviceStateBase state);
        public abstract bool CancelApplyState();

        public delegate void MoveCompletedHandler(object sender);
        public delegate void DeviceErrorHandler(object sender, string error);

        public event MoveCompletedHandler MoveCompleted;
        public event DeviceErrorHandler DeviceError;

        protected void FireMoveCompleted()
        {
            MoveCompleted?.Invoke(this);
        }

        protected void FireDeviceError(string msg)
        {
            DeviceError?.Invoke(this, msg);
        }

    }
}
