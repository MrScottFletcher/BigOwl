using BigOwl.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigOwl.Devices
{
    public abstract class OwlControllerBase
    {
        public string Name { get; set; }
        public string ControllerType { get; set; }
        public DateTime InitializedDateTime { get; set; }
        public string LastError { get; set; }
        public OwlDeviceStateBase.StatusTypes Status { get; set; }
        public OwlDeviceStateBase.StatusReasonTypes StatusReason { get; set; }

        public bool StayEngagedAfterMove { get; set; }
        public bool ReturnHomeAfterMove { get; set; }
        public int DelaySecondsBeforeReturnHome { get; set; }

        public decimal? StepToPositionFactor { get; set; }
        public int? CurrentPosition { get; set; }
        public int? MinPosition { get; set; }
        public int? MaxPosition { get; set; }
        public int? HomePosition { get; set; }

        public int? ForwardLimitSensorPin { get; set; }
        public int? BackwardsLimitSensorPin { get; set; }

        public abstract bool Initialize();
        public abstract bool Enable();
        public abstract bool Disable();
        public abstract bool Shutdown();
        public abstract void Recalibrate();
        public abstract void GoHomePosition();

        public abstract OwlDeviceStateBase GetState();
        public abstract void ApplyState(OwlDeviceStateBase state);
        public abstract bool CancelApplyState();

        public abstract void DoPositionList(List<int> positions, int msDelayBetween, bool returnToHome);
        public abstract void GotoPosition(int position);

        public delegate void ForwardLimitReachedHandler(object sender);
        public delegate void BackwardLimitReachedHandler(object sender);
        public delegate void MoveCompletedHandler(object sender);
        public delegate void DeviceErrorHandler(object sender, string error);

        public event ForwardLimitReachedHandler ForwardLimitReached;
        public event BackwardLimitReachedHandler BackwardLimitReached;
        public event MoveCompletedHandler MoveCompleted;
        public event DeviceErrorHandler DeviceError;

        protected void FireForwardLimitReached()
        {
            ForwardLimitReached?.Invoke(this);
        }

        protected void FireBackwardLimitReached()
        {
            BackwardLimitReached?.Invoke(this);
        }

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
