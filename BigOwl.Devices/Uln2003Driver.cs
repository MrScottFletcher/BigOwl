using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace BigOwl.Devices
{
    public class Uln2003Driver : IDisposable
    {
        private readonly GpioPin[] _gpioPins = new GpioPin[4];

        public int StepDelayMs { get; set; }

        private readonly GpioPinValue[][] _waveDriveSequence =
        {
            new[] {GpioPinValue.High, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low},
            new[] {GpioPinValue.Low, GpioPinValue.High, GpioPinValue.Low, GpioPinValue.Low},
            new[] {GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.High, GpioPinValue.Low},
            new[] {GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.High}
        };

        private readonly GpioPinValue[][] _fullStepSequence =
        {
            new[] {GpioPinValue.High, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.High},
            new[] {GpioPinValue.High, GpioPinValue.High, GpioPinValue.Low, GpioPinValue.Low},
            new[] {GpioPinValue.Low, GpioPinValue.High, GpioPinValue.High, GpioPinValue.Low},
            new[] {GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.High, GpioPinValue.High }
        };

        private readonly GpioPinValue[][] _biPolarStepSequence =
        {
            new[] {GpioPinValue.High, GpioPinValue.Low, GpioPinValue.High, GpioPinValue.Low},
            new[] {GpioPinValue.Low, GpioPinValue.High, GpioPinValue.High, GpioPinValue.Low},
            new[] {GpioPinValue.Low, GpioPinValue.High, GpioPinValue.Low, GpioPinValue.High},
            new[] {GpioPinValue.High, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.High},
        };


        private readonly GpioPinValue[][] _haveStepSequence =
        {
            new[] {GpioPinValue.High, GpioPinValue.High, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.High},
            new[] {GpioPinValue.Low, GpioPinValue.High, GpioPinValue.High, GpioPinValue.High, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low},
            new[] {GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.High, GpioPinValue.High, GpioPinValue.High, GpioPinValue.Low, GpioPinValue.Low},
            new[] {GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.High, GpioPinValue.High, GpioPinValue.High }
        };

        public Uln2003Driver(int blueWireToGpio, int pinkWireToGpio, int yellowWireToGpio, int orangeWireToGpio)
        {
            var gpio = GpioController.GetDefault();

            _gpioPins[0] = gpio.OpenPin(blueWireToGpio);
            _gpioPins[1] = gpio.OpenPin(pinkWireToGpio);
            _gpioPins[2] = gpio.OpenPin(yellowWireToGpio);
            _gpioPins[3] = gpio.OpenPin(orangeWireToGpio);

            foreach (var gpioPin in _gpioPins)
            {
                gpioPin.Write(GpioPinValue.Low);
                gpioPin.SetDriveMode(GpioPinDriveMode.Output);
            }

            StepDelayMs = 5;
        }

        public void SetStepDelay(int milliseconds)
        {
            StepDelayMs = milliseconds;
        }

        public async Task TurnAsync(int degree, TurnDirection direction, DrivingMethod drivingMethod = DrivingMethod.FullStep)
        {
            var steps = 0;
            GpioPinValue[][] methodSequence;
            switch (drivingMethod)
            {
                case DrivingMethod.WaveDrive:
                    methodSequence = _waveDriveSequence;
                    steps = (int)Math.Ceiling(degree / 0.1767478397486253);
                    break;
                case DrivingMethod.FullStep:
                    methodSequence = _fullStepSequence;
                    steps = (int)Math.Ceiling(degree / 0.1767478397486253);
                    break;
                case DrivingMethod.HalfStep:
                    methodSequence = _haveStepSequence;
                    steps = (int)Math.Ceiling(degree / 0.0883739198743126);
                    break;
                case DrivingMethod.BiPolar:
                    methodSequence = _biPolarStepSequence;
                    steps = (int)Math.Ceiling(degree / 0.1767478397486253);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(drivingMethod), drivingMethod, null);
            }
            var counter = 0;
            while (counter < steps)
            {
                for (var j = 0; j < methodSequence[0].Length; j++)
                {
                    //for (var i = 0; i < 4; i++)
                    //{
                    //    GpioPinValue v = methodSequence[direction == TurnDirection.Left ? i : 3 - i][j];
                    //    if (v == GpioPinValue.High)
                    //        _gpioPins[i].Write(v);

                    //    //_gpioPins[i].Write(methodSequence[direction == TurnDirection.Left ? i : 3 - i][j]);
                    //    //await Task.Delay(1);
                    //}
                    //for (var i = 0; i < 4; i++)
                    //{
                    //    GpioPinValue v = methodSequence[direction == TurnDirection.Left ? i : 3 - i][j];
                    //    if (v == GpioPinValue.Low)
                    //        _gpioPins[i].Write(v);
                    //    //await Task.Delay(1);
                    //}

                    //Do it straight
                    for (var i = 0; i < 4; i++)
                    {
                        _gpioPins[i].Write(methodSequence[direction == TurnDirection.Left ? i : 3 - i][j]);
                    }

                    if (StepDelayMs == 0)
                        StepDelayMs = 1; //must be at least one

                    await Task.Delay(StepDelayMs);

                    //if (drivingMethod != DrivingMethod.BiPolar)
                    //{
                    //    if (StepDelayMs == 0)
                    //        StepDelayMs = 1; //must be at least one

                    //    await Task.Delay(StepDelayMs);
                    //}

                    counter++;
                    if (counter == steps)
                        break;
                }
            }

            Stop();
        }

        private void Stop()
        {
            foreach (var gpioPin in _gpioPins)
            {
                gpioPin.Write(GpioPinValue.Low);
            }
        }

        public void Dispose()
        {
            foreach (var gpioPin in _gpioPins)
            {
                gpioPin.Write(GpioPinValue.Low);
                gpioPin.Dispose();
            }
        }
    }

    public enum DrivingMethod
    {
        WaveDrive,
        FullStep,
        HalfStep,
        BiPolar
    }

    public enum TurnDirection
    {
        Left,
        Right
    }
}
