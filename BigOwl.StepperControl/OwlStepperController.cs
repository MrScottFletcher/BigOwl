using BigOwl.Devices;
using BigOwl.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigOwl.StepperControl
{
    public class OwlStepperController : OwlControllerBase
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

        public bool MovesCancelled { get; private set; }

        public OwlStepperController() : base()
        {
            //do anything PRIOR to having pins set
            State = new StepperState();
            State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.Unknown;

        }

        public OwlStepperController(int directionPin, int stepPin, int stepModePinOne, int stepModePinTwo, int sleepPin, int enablePin, bool bTwoStepMode) : this()
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
                State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.Sleeping;
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
                State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.Awake;
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
                State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.Sleeping;
            }
            catch (Exception exAny)
            {
                LastError = exAny.ToString();
                FireDeviceError(exAny.Message);
            }

            return bOK;
        }

        public override void DoPositionList(List<int> positions, int msPauseBetween, bool returnHomeAfter)
        {
            positions.ForEach(p =>
            {
                if (!MovesCancelled)
                {
                    GotoPosition(p);
                    System.Threading.Thread.Sleep(msPauseBetween);
                }
            });

            MovesCancelled = false;

            if (returnHomeAfter)
                ReturnHome();
        }

        private void ReturnHome()
        {
            GotoPosition(HomePosition.GetValueOrDefault());
        }

        public override void GotoPosition(int p)
        {
            if (VerifyInitialized())
            {
                MovesCancelled = false;
                EasyStepperDriver.Direction dir = EasyStepperDriver.Direction.Forward;

                if (p > MaxPosition.Value)
                    p = MaxPosition.Value;

                if (p < MinPosition.Value)
                    p = MinPosition.Value;

                int positionDiff = p - CurrentPosition.Value;
                int stepsDiff = Convert.ToInt32(Convert.ToDecimal(positionDiff) * StepToPositionFactor.Value);
                if (stepsDiff < 0)
                {
                    dir = EasyStepperDriver.Direction.Backward;
                    stepsDiff = stepsDiff * -1;
                }

                if (stepsDiff != 0)
                {
                    State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.ExecutingMove;

                    this.Driver.WakeUp();
                    this.Driver.EnableOutputs();
                    this.Driver.StepMode = EasyStepperDriver.Mode.Full;

                    this.Driver.StepDirection = dir;
                    Task.Run(async () => { await this.Driver.Turn(Convert.ToUInt32(stepsDiff), 1); }).Wait();

                    CurrentPosition = CurrentPosition.Value + positionDiff;

                    //Task t = this.Driver.Turn(Convert.ToUInt32(stepsDiff), 1);
                    //t.Wait();

                    this.Driver.DisableOutputs();
                    this.Driver.Sleep();
                    State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.Sleeping;
                }

                FireMoveCompleted();
            }
            else
            {
                State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.InError;
            }

        }

        private bool VerifyInitialized()
        {
            bool bOK = true;
            if (!StepToPositionFactor.HasValue)
            {
                this.FireDeviceError("StepToPositionFactor is not initialized");
                bOK = false;
            }
            if (!MaxPosition.HasValue)
            {
                this.FireDeviceError("MaxPosition is not initialized");
                bOK = false;
            }
            if (!MinPosition.HasValue)
            {
                this.FireDeviceError("MinPosition is not initialized");
                bOK = false;
            }
            if (!CurrentPosition.HasValue)
            {
                this.FireDeviceError("CurrentPosition is not initialized");
                bOK = false;
            }

            return bOK;
        }

        public override void Recalibrate()
        {
            State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.Calibrating;

            //Do the limit test here.
            GotoPosition(0);

            GoHomePosition();
        }

        public override bool Shutdown()
        {
            bool bOK = false;
            try
            {
                bOK = Disable();
                State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.ShutDown;
            }
            catch (Exception exAny)
            {
                LastError = exAny.ToString();
                FireDeviceError(exAny.Message);
            }

            return bOK;
        }

        public override void GoHomePosition()
        {
            State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.GoingHome;
            GotoPosition(HomePosition.Value);
            FireMoveCompleted();
        }

        public override OwlDeviceStateBase GetState()
        {
            return State as OwlDeviceStateBase;
        }

        //public override void EnqueueState(OwlDeviceStateBase state)
        //{
        //    bool bOK = false;

        //    //add the state to the list

        //}

        public override void ApplyState(OwlDeviceStateBase state)
        {
            bool bOK = false;

            State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.ExecutingMove;


            FireMoveCompleted();
        }

        public override bool CancelApplyState()
        {
            bool bOK = false;
            MovesCancelled = true;
            State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.GoingHome;
            FireMoveCompleted();
            return bOK;
        }
        
    }
}
