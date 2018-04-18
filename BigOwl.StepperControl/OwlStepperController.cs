using BigOwl.Devices;
using BigOwl.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

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
                if (_driver == null)
                {
                    lock (lockObj)
                    {
                        if (_driver == null)
                        {
                            _driver = new EasyStepperDriver(DirectionPin, StepPin, StepModePinOne, StepModePinTwo, SleepPin, EnablePin, isTwoStepMode);
                        }
                    }
                }
                return _driver;
            }
        }

        public bool MovesCancelled { get; private set; }

        public OwlStepperController(decimal stepsToPositionFactor) : base(stepsToPositionFactor)
        {
            //do anything PRIOR to having pins set
            State = new StepperState();
            State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.Unknown;

        }

        public OwlStepperController(string name, int directionPin, int stepPin, int stepModePinOne, int stepModePinTwo, int sleepPin, int enablePin, int? forwardLimitSensorPin, int? backwardsLimitSensorPin, int stepsToPositionFactor, bool bTwoStepMode) : this(stepsToPositionFactor)
        {
            Name = name;
            DirectionPin = directionPin;
            StepPin = stepPin;
            StepModePinOne = stepModePinOne;
            StepModePinTwo = stepModePinTwo;
            SleepPin = sleepPin;
            EnablePin = enablePin;
            isTwoStepMode = bTwoStepMode;
            ForwardLimitPinNumber = forwardLimitSensorPin;
            BackwardsLimitPinNumber = backwardsLimitSensorPin;

            InitSubscribeToLimitPinEvents();
        }

        protected override void InitSubscribeToLimitPinEvents()
        {
            base.InitSubscribeToLimitPinEvents();

            //also subscribe to set the driver stops
            this.ForwardLimitSensorChanged += OwlStepperController_ForwardLimitSensorChanged;
            this.BackwardLimitSensorChanged += OwlStepperController_BackwardLimitSensorChanged;
        }

        private void OwlStepperController_BackwardLimitSensorChanged(object sender)
        {
            this.Driver.BackwardBlocked = this.IsBackwardLimitReached().GetValueOrDefault();
        }

        private void OwlStepperController_ForwardLimitSensorChanged(object sender)
        {
            this.Driver.ForwardBlocked = this.IsForwardLimitReached().GetValueOrDefault();
        }

        public override bool Initialize()
        {
            bool bOK = false;
            try
            {
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

        public override bool Enable()
        {
            bool bOK = false;
            try
            {
                if (!Driver.IsOutputsEnable)
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
            GotoPosition(p, false);
        }

        private void GotoPosition(int p, bool relaxAfterMove)
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
                int stepsDiff = Convert.ToInt32(Convert.ToDecimal(positionDiff) * StepsToPositionFactor);
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

                    if (relaxAfterMove)
                    {
                        this.Driver.DisableOutputs();
                        this.Driver.Sleep();
                        State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.Sleeping;
                    }
                    else
                    {
                        State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.Awake;
                    }
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
            if (StepsToPositionFactor == 0)
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

            if (IsControlEnabled)
            {
                this.Status = OwlDeviceStateBase.StatusTypes.Unavailable;
                State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.Calibrating;
                CurrentPosition = null;

                EasyStepperDriver.Direction directionForCalibration = EasyStepperDriver.Direction.Forward;
                //check the actual Gpio pins, not just the pin numbers.
                if (ForwardLimitSensorGpioPin == null && BackwardsLimitSensorGpioPin == null)
                {
                    //can't calibrate anything.  Disable me.
                    this.Status = OwlDeviceStateBase.StatusTypes.InError;
                    this.StatusReason = OwlDeviceStateBase.StatusReasonTypes.InError;
                    this.FireDeviceError("Cannot calibrate.  No limit sensors found");
                }
                else
                {
                    //do we calibrate forwards or backwards or both?

                    bool isDirectLimitReachedAlready = false;
                    if (ForwardLimitSensorGpioPin != null)
                    {
                        if (IsForwardLimitReached().HasValue)
                        {
                            isDirectLimitReachedAlready = IsForwardLimitReached().Value;
                        }
                        else
                        {
                            //Expected to be able to 
                            this.Status = OwlDeviceStateBase.StatusTypes.InError;
                            this.StatusReason = OwlDeviceStateBase.StatusReasonTypes.InError;
                            this.FireDeviceError("Cannot calibrate.  Expected Forward limit sensor to return a non-null value.");
                        }
                    }
                    else if (BackwardsLimitSensorGpioPin != null)
                    {
                        directionForCalibration = EasyStepperDriver.Direction.Backward;
                        if (IsBackwardLimitReached().HasValue)
                        {
                            isDirectLimitReachedAlready = IsBackwardLimitReached().Value;
                        }
                        else
                        {
                            //Expected to be able to 
                            this.Status = OwlDeviceStateBase.StatusTypes.InError;
                            this.StatusReason = OwlDeviceStateBase.StatusReasonTypes.InError;
                            this.FireDeviceError("Cannot calibrate.  Expected Backwards limit sensor to return a non-null value (and there was no Forward limit sensor).");
                        }
                    }
                }
                if (this.Status != OwlDeviceStateBase.StatusTypes.InError)
                {
                    bool limitHit = false;
                    //Do the walk until we hit
                    limitHit = (directionForCalibration == EasyStepperDriver.Direction.Forward) ? IsForwardLimitReached().Value : IsBackwardLimitReached().Value;
                    int currentStep = 0;
                    int stepToIncrement = 2;

                    this.Driver.WakeUp();
                    this.Driver.EnableOutputs();
                    this.Driver.StepMode = EasyStepperDriver.Mode.Full;

                    while (!limitHit && currentStep < StepsInRangeOfMotion)
                    {
                        //Step forward 2 steps

                        this.Driver.StepDirection = directionForCalibration;
                        Task.Run(async () => { await this.Driver.Turn(Convert.ToUInt32(stepToIncrement), 1); }).Wait();

                        currentStep += stepToIncrement;

                        //Task t = this.Driver.Turn(Convert.ToUInt32(stepsDiff), 1);
                        //t.Wait();

                        limitHit = (directionForCalibration == EasyStepperDriver.Direction.Forward) ? IsForwardLimitReached().Value : IsBackwardLimitReached().Value;
                    }
                    this.Driver.DisableOutputs();
                    this.Driver.Sleep();

                    if (limitHit)
                    {
                        //This sets the already-known scale.  It does not auto-scale yet
                        this.CurrentPosition = (directionForCalibration == EasyStepperDriver.Direction.Forward) ? 100 : 0;
                    }
                }


                //Do the limit test here.
                GotoPosition(0);

                GoHomePosition();
            }
            else
            {
                //not enabled
                this.Status = OwlDeviceStateBase.StatusTypes.Unavailable;
                this.StatusReason = OwlDeviceStateBase.StatusReasonTypes.Unknown;
                this.CurrentPosition = null;
            }
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
            GotoPosition(HomePosition.Value, true);
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
            throw new NotImplementedException();

            bool bOK = false;
            MovesCancelled = true;
            State.StatusReason = OwlDeviceStateBase.StatusReasonTypes.GoingHome;
            FireMoveCompleted();
            return bOK;
        }

    }
}
