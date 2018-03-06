using BigOwl.Devices;
using BigOwl.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigOwl.StepperControl
{
    public class StepperController : ControllerBase
    {

        public StepperState State { get; set; }
        public EasyStepperDriver _driver { get; set; }

        public int DirectionPin { get; set; }
        public int StepPin { get; set; }
        public int StepModePinOne { get; set; }
        public int StepModePinTwo { get; set; }
        public int SleepPin { get; set; }
        public int EnablePin { get; set; }
        public bool isTwoStepMode { get; set; }

        object lockObj = new object();
        
        public EasyStepperDriver Driver
        {
            get
            {
                if(_driver == null)
                {
                    lock (lockObj)
                    {
                        if(_driver == null)
                        {
                            _driver = new EasyStepperDriver(DirectionPin, StepPin, StepModePinOne, StepModePinTwo, SleepPin, EnablePin, isTwoStepMode);
                        }
                    }
                }
                return _driver;
            }
        }

        public StepperController()
        {
            //do anything PRIOR to having pins set
            State = new StepperState();
            State.Status = OwlDeviceStateBase.StatusTypes.Unknown;

        }

        public StepperController(int directionPin, int stepPin, int stepModePinOne, int stepModePinTwo, int sleepPin, int enablePin, bool bTwoStepMode) : base()
        {
            DirectionPin = directionPin;
            StepPin = stepPin;
            StepModePinOne = stepModePinOne;
            StepModePinTwo = stepModePinTwo;
            SleepPin = sleepPin;
            EnablePin = enablePin;
            isTwoStepMode = bTwoStepMode;
        }

        public override bool Initialize()
        {
            bool bOK = false;
            try
            {
                bOK = Driver.DisableOutputs();
                State.Status = OwlDeviceStateBase.StatusTypes.Sleeping;
            }
            catch(Exception exAny)
            {
                LastError = exAny.ToString();
                FireDeviceError(exAny.Message);
            }

            return bOK;
        }

        public override bool Enable()
        {
            bool bOK = false;
            try
            {
                if(!Driver.IsOutputsEnable)
                    bOK = Driver.EnableOutputs();
                if (Driver.IsDriverSleep)
                    bOK = Driver.WakeUp();
                State.Status = OwlDeviceStateBase.StatusTypes.Awake;
            }
            catch (Exception exAny)
            {
                LastError = exAny.ToString();
                FireDeviceError(exAny.Message);
            }

            return bOK;
        }

        public override bool Disable()
        {
            bool bOK = false;
            try
            {
                if (!Driver.IsDriverSleep)
                    bOK = Driver.Sleep();
                if (Driver.IsOutputsEnable)
                    bOK = Driver.DisableOutputs();
                State.Status = OwlDeviceStateBase.StatusTypes.Sleeping;
            }
            catch (Exception exAny)
            {
                LastError = exAny.ToString();
                FireDeviceError(exAny.Message);
            }

            return bOK;
        }

        public override bool Shutdown()
        {
            bool bOK = false;
            try
            {
                bOK = Disable();
                State.Status = OwlDeviceStateBase.StatusTypes.ShutDown;
            }
            catch (Exception exAny)
            {
                LastError = exAny.ToString();
                FireDeviceError(exAny.Message);
            }

            return bOK;

        }

        public override bool Recalibrate()
        {
            bool bOK = false;

            State.Status = OwlDeviceStateBase.StatusTypes.Calibrating;

            FireMoveCompleted();

            return bOK;
        }

        public override bool GoHomePosition()
        {
            bool bOK = false;
            State.Status = OwlDeviceStateBase.StatusTypes.GoingHome;
            FireMoveCompleted();
            return bOK;
        }

        public override OwlDeviceStateBase GetState()
        {
            return State as OwlDeviceStateBase;
        }

        public override bool ApplyState(OwlDeviceStateBase state)
        {
            bool bOK = false;
            State.Status = OwlDeviceStateBase.StatusTypes.ExecutingMove;

            FireMoveCompleted();

            return bOK;
        }

        public override bool CancelApplyState()
        {
            bool bOK = false;
            State.Status = OwlDeviceStateBase.StatusTypes.GoingHome;
            FireMoveCompleted();
            return bOK;
        }
        
    }
}
