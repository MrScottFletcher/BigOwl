using BigOwl.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

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

        public bool IsControlEnabled { get; set; }

        public bool StayEngagedAfterMove { get; set; }
        public bool ReturnHomeAfterMove { get; set; }
        public int DelaySecondsBeforeReturnHome { get; set; }

        public decimal StepsToPositionFactor { get; private set; }
        public decimal StepsInRangeOfMotion { get; private set; }
        public int? CurrentPosition { get; set; }
        public int? MinPosition { get; set; }
        public int? MaxPosition { get; set; }
        public int? HomePosition { get; set; }

        public int? ForwardLimitPinNumber { get; set; }
        public int? BackwardsLimitPinNumber { get; set; }

        protected GpioPin ForwardLimitSensorGpioPin { get; set; }
        protected GpioPin BackwardsLimitSensorGpioPin { get; set; }

        protected GpioController _gpioController;

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

        public delegate void ForwardLimitSensorChangedHandler(object sender);
        public delegate void BackwardLimitSensorChangedHandler(object sender);
        public delegate void MoveCompletedHandler(object sender);
        public delegate void DeviceErrorHandler(object sender, string error);
        public delegate void DeviceInfoMessageHandler(object sender, string message);

        public event ForwardLimitSensorChangedHandler ForwardLimitSensorChanged;
        public event BackwardLimitSensorChangedHandler BackwardLimitSensorChanged;
        public event MoveCompletedHandler MoveCompleted;
        public event DeviceErrorHandler DeviceError;
        public event DeviceInfoMessageHandler DeviceInfoMessage;

        protected bool pinEventsSubscribed = false;

        public OwlControllerBase(decimal stepToPositionFactor)
        {
            StepsToPositionFactor = stepToPositionFactor;
            StepsInRangeOfMotion = StepsToPositionFactor * 100;
        }

        protected virtual void InitSubscribeToLimitPinEvents()
        {
            if (!pinEventsSubscribed)
            {
                _gpioController = GpioController.GetDefault();

                if (ForwardLimitPinNumber.HasValue)
                {
                    ForwardLimitSensorGpioPin = _gpioController.OpenPin(ForwardLimitPinNumber.Value);
                    ForwardLimitSensorGpioPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
                    ForwardLimitSensorGpioPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                }

                if (BackwardsLimitPinNumber.HasValue)
                {
                    BackwardsLimitSensorGpioPin = _gpioController.OpenPin(BackwardsLimitPinNumber.Value);
                    BackwardsLimitSensorGpioPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
                    BackwardsLimitSensorGpioPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                }

                if (ForwardLimitSensorGpioPin != null)
                {
                    ForwardLimitSensorGpioPin.ValueChanged += ForwardLimitSensorGpioPin_ValueChanged;
                }

                if (BackwardsLimitSensorGpioPin != null)
                {
                    BackwardsLimitSensorGpioPin.ValueChanged += BackwardsLimitSensorGpioPin_ValueChanged; ;
                }
                pinEventsSubscribed = true;
            }
        }

        private void BackwardsLimitSensorGpioPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            FireBackwardLimitSensorChanged();
        }

        private void ForwardLimitSensorGpioPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            FireForwardLimitSensorChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns null if no Gpio pin</returns>
        public bool? IsForwardLimitReached()
        {
            return IsLimitReached(ForwardLimitSensorGpioPin);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns null if no Gpio pin</returns>
        public bool? IsBackwardLimitReached()
        {
            return IsLimitReached(BackwardsLimitSensorGpioPin);
        }

        private bool? IsLimitReached(GpioPin limitGpio)
        {
            bool? isLimitReached = null;
            if (limitGpio != null)
            {
                isLimitReached = limitGpio.Read() == GpioPinValue.High;
            }

            return isLimitReached;
        }

        protected void FireForwardLimitSensorChanged()
        {
            ForwardLimitSensorChanged?.Invoke(this);
        }

        protected void FireBackwardLimitSensorChanged()
        {
            BackwardLimitSensorChanged?.Invoke(this);
        }

        protected void FireMoveCompleted()
        {
            MoveCompleted?.Invoke(this);
        }

        protected void FireDeviceError(string msg)
        {
            DeviceError?.Invoke(this, msg);
        }

        protected void FireDeviceInfoMessage(string msg)
        {
            DeviceInfoMessage?.Invoke(this, msg);
        }


    }
}
