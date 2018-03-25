using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace BigOwl.Devices
{
    /// <summary>
    /// A class to manage the EasyDriver v4.4. An Open Source Hardware Stepper Motor Drive Project.
    /// </summary>
    /// <remarks>
    /// Thanks to Gus (GHI Electronics team). You may have some additional information about this class on http://webge.github.io/EasyStepperDriver/
    /// </remarks>
    public class EasyStepperDriver : IDisposable
    {
        #region Properties
        private GpioPin _SleepPin;
        private GpioPin _EnablePin;
        private GpioPin _DirectionPin;
        private GpioPin _StepPin;
        private GpioPin _StepModePinOne;
        private GpioPin _StepModePinTwo;
        private int _StepDelay = 2;
        private UInt32 _Steps = 0;
        private Mode _StepMode = Mode.Full;
        private Direction _StepDirection = Direction.Forward;

        /// <summary>
        /// Directions are Forward or Backward
        /// </summary>
        public enum Direction : byte
        {
            Forward,
            Backward
        }
        /// <summary>
        /// Modes are Full, Half, Quarter, OneEighth
        /// </summary> 
        public enum Mode : byte
        {
            Full,
            Half,
            Quarter,
            OneEighth
        }
        /// <summary>
        /// Get Steps
        /// </summary>
        public UInt32 Steps
        {
            get
            {
                return _Steps;
            }
        }

        /// <summary>
        /// Get or Set direction
        /// </summary>      
        public Direction StepDirection
        {
            get
            {
                return _StepDirection;
            }
            set
            {
                _StepDirection = value;
            }
        }

        /// <summary>
        /// Get or Set Step Mode
        /// </summary>
        public Mode StepMode
        {
            get
            {
                return _StepMode;
            }
            set
            {
                _StepMode = value;
            }
        }

        /// <summary>
        /// Get or set time between two step
        /// </summary>
        public int StepDelay
        {
            get
            {
                return _StepDelay;
            }
            set
            {
                _StepDelay = value;
            }

        }

        /// <summary>
        /// Get if Sleep or not
        /// </summary>
        public bool IsDriverSleep
        {
            get
            {
                return ((GpioPinValue)_SleepPin.Read()) == GpioPinValue.High;
            }
        }

        /// <summary>
        /// Get if Enable or Disable
        /// </summary>
        public bool IsOutputsEnable
        {
            get
            {
                return _EnablePin.Read() == GpioPinValue.High;
            }
        }

        public bool ForwardBlocked { get; set; }
        public bool BackwardBlocked { get; set; }

        #endregion properties

        #region Constructors

        /// <summary>
        /// Creates an instance of the driver, that only lets you move, choose direction, sleep, select step mode and enable / disable card
        /// </summary>
        /// <param name="DirectionPin">(DIR) This needs to be a 0V to 5V (or 0V to 3.3V if you've set your Easy Driver up that way) digital signal.
        /// The level if this signal (high/low) is sampled on each rising edge of STEP to determine which direction to take the step (or microstep).</param>
        /// <param name="StepPin">(STEP) This needs to be a 0V to 5V (or 0V to 3.3V if you've set your Easy Driver that way) digital signal. 
        /// Each rising edge of this signal will cause one step (or microstep) to be taken.</param>
        /// <param name="StepModePinOne">(MS1) These digital inputs control the microstepping mode. Possible settings are (MS1/MS2) : full step (0,0), half step (1,0), 1/4 step (0,1), and 1/8 step (1,1 : default)</param>
        /// <param name="StepModePinTwo">(MS2) These digital inputs control the microstepping mode. Possible settings are (MS1/MS2) : full step (0,0), half step (1,0), 1/4 step (0,1), and 1/8 step (1,1 : default)</param>
        /// <param name="SleepPin">(SLP) This normally high input signal will minimize power consumption by disabling 
        /// internal circuitry and the output drivers when pulled low.(disabled by default)</param>
        /// <param name="EnablePin">(EN) This normally low input signal will disable all outputs when pulled high.</param>
        public EasyStepperDriver(int DirectionPin, int StepPin, int StepModePinOne, int StepModePinTwo, int SleepPin, int EnablePin, bool isTwoStepMode)
        {
            var gpio = GpioController.GetDefault();

            _DirectionPin = gpio.OpenPin(DirectionPin);
            _DirectionPin.Write(GpioPinValue.Low); //forward
            _DirectionPin.SetDriveMode(GpioPinDriveMode.Output);

            _StepPin = gpio.OpenPin(StepPin);
            _StepPin.Write(GpioPinValue.Low);
            _StepPin.SetDriveMode(GpioPinDriveMode.Output);

            _StepModePinOne = gpio.OpenPin(StepModePinOne);
            _StepModePinOne.Write(GpioPinValue.High);//  OneEighth step
            _StepModePinOne.SetDriveMode(GpioPinDriveMode.Output);

            _StepModePinTwo = gpio.OpenPin(StepModePinTwo);
            if (isTwoStepMode)
                _StepModePinTwo.Write(GpioPinValue.High);
            else
                _StepModePinTwo.Write(GpioPinValue.Low);

            _StepModePinTwo.SetDriveMode(GpioPinDriveMode.Output);

            _SleepPin = gpio.OpenPin(SleepPin);
            _SleepPin.Write(GpioPinValue.Low); //sleep enable
            _SleepPin.SetDriveMode(GpioPinDriveMode.Output);

            _EnablePin = gpio.OpenPin(EnablePin);
            _EnablePin.Write(GpioPinValue.Low); //outputs enable
            _EnablePin.SetDriveMode(GpioPinDriveMode.Output);

        }

        #endregion

        /// <summary>
        /// Put the stepper driver to sleep mode
        /// </summary>
        /// <returns>Boolean : True if sleep, false otherwise</returns>
        public bool Sleep()
        {
            if (_SleepPin != null)
            {
                _SleepPin.Write(GpioPinValue.High);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Wake up the stepper driver
        /// </summary>
        /// <returns>Boolean : True if WakeUp, false otherwise </returns>
        public bool WakeUp()
        {
            if (_SleepPin != null)
            {
                _SleepPin.Write(GpioPinValue.High);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Enable the stepper driver outputs
        /// </summary>
        /// <returns>Boolean : True if Outputs are enable, false otherwise </returns>
        public bool EnableOutputs()
        {
            if (_EnablePin != null)
            {
                _EnablePin.Write(GpioPinValue.Low);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Disable the stepper driver outputs
        /// </summary>
        /// <returns>Boolean : True if Outputs are disable, false otherwise</returns>
        public bool DisableOutputs()
        {
            if (_EnablePin != null)
            {
                _EnablePin.Write(GpioPinValue.High);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Moves the stepper motor
        /// </summary>
        /// <param name="steps">Indicate the amount of steps that need to be moved</param>
        /// <param name="stepdelay">Duration between steps</param>
        public async Task Turn(UInt32 steps, int stepdelay = 2)
        {
            _Steps = steps; _StepDelay = stepdelay;
            await ChangeStepMode(_StepMode);
            await ChangeDirection(_StepDirection);
            for (Int32 i = 0; i < _Steps; i++)
            {
                _StepPin.Write(GpioPinValue.High);
                await Task.Delay(_StepDelay);
                _StepPin.Write(GpioPinValue.Low);

                if (!ContinueDirection(_StepDirection))
                    break;
            }
        }

        private bool ContinueDirection(Direction stepDirection)
        {
            bool ok = true;
            switch (stepDirection)
            {
                case Direction.Forward:
                    ok = !ForwardBlocked;
                    break;
                case Direction.Backward:
                    ok = !BackwardBlocked;
                    break;
            }
            return ok;
        }

        /// <summary>
        /// Moves the stepper motor
        /// </summary>
        /// <param name="steps">Indicate the amount of steps that need to be moved</param>
        /// <param name="direction">Indicates the direction of rotation</param>
        /// <param name="stepdelay">Duration between steps</param>
        /// <param name="mode">Full, Half, Quarter, or OneEighth step</param>                               
        public async Task Turn(UInt32 steps, Direction direction, int stepdelay = 2, Mode mode = Mode.OneEighth)
        {
            _StepMode = mode; _StepDirection = direction; _StepDelay = stepdelay; _Steps = steps;
            await ChangeStepMode(mode);
            await ChangeDirection(direction);
            for (Int32 i = 0; i < _Steps; i++)
            {
                _StepPin.Write(GpioPinValue.High);
                await Task.Delay(_StepDelay);
                _StepPin.Write(GpioPinValue.Low);
            }
        }

        /// <summary>
        /// Set Direction pin
        /// </summary>
        /// <param name="direction">Indicates the direction of rotation</param>
        private async Task ChangeDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Forward:
                    if (_DirectionPin != null)
                        _DirectionPin.Write(GpioPinValue.High);
                    break;
                case Direction.Backward:
                    if (_DirectionPin != null)
                        _DirectionPin.Write(GpioPinValue.Low);
                    break;
            }
        }
        /// <summary>
        /// Set pins MS1 and MS2 
        /// </summary>
        /// <param name="mode">Full, Half, Quarter, or OneEighth step</param>
        private async Task ChangeStepMode(Mode mode)
        {
            if (_StepModePinOne != null & _StepModePinTwo != null)
            {
                switch (mode)
                {
                    case Mode.Full:
                        _StepModePinOne.Write(GpioPinValue.Low);
                        _StepModePinTwo.Write(GpioPinValue.Low);
                        break;
                    case Mode.Half:
                        _StepModePinOne.Write(GpioPinValue.High);
                        _StepModePinTwo.Write(GpioPinValue.Low);
                        break;
                    case Mode.Quarter:
                        _StepModePinOne.Write(GpioPinValue.Low);
                        _StepModePinTwo.Write(GpioPinValue.High);
                        break;
                    case Mode.OneEighth:
                        _StepModePinOne.Write(GpioPinValue.High);
                        _StepModePinTwo.Write(GpioPinValue.High);
                        break;
                }
            }
        }

        public void Dispose()
        {
            //Sleep
            _SleepPin.Write(GpioPinValue.High);
            _SleepPin.Dispose();

            _EnablePin.Write(GpioPinValue.Low);
            _EnablePin.Dispose();
            _DirectionPin.Write(GpioPinValue.Low);
            _DirectionPin.Dispose();
            _StepPin.Write(GpioPinValue.Low);
            _StepPin.Dispose();
            _StepModePinOne.Write(GpioPinValue.Low);
            _StepModePinOne.Dispose();
            _StepModePinTwo.Write(GpioPinValue.Low);
            _StepModePinTwo.Dispose();
        }
    }
}
