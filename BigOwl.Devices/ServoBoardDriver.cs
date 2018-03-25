//using AdafruitClassLibrary;
using Adafruit.PCA9685;
using BigOwl.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace BigOwl.Devices
{
    public class ServoBoardDriver
    {
        public readonly Pca9685 _pca9685;
        private static ushort servoMinPulseLength = 110;
        private static ushort servoMaxPulseLength = 450;
        private static ushort servoMaxPulseLength_UPPER = 600;

        public ServoBoardDriver()
        {
            _pca9685 = new Pca9685();
        }


        public void Initialize()
        {
            //The PCA9685 Class has a single constructor. 
            //The constructor takes an optional argument of an I2C address. 
            //I2C addresses for the chip are in the range 0x40 to 0x7F.

            //default init is I2CBase.I2CSpeed.I2C_100kHz
            Task.Run(async () => { await _pca9685.InitPCA9685Async(); }).Wait();

            Task.Delay(1000).Wait();

            //_pca9685.Reset();
            _pca9685.SetPWMFrequency(50);
            //_pca9685.SetAllPWM(0, servoMinPulseLength);
        }

        public class ServoPort : OwlControllerBase
        {
            private readonly Pca9685 _pca9685;
            public bool InvertDirection { get; set; }
            public int PortNumber { get; set; }
            public ServoPort(string name, int port, Pca9685 pca9685, bool bInverted) : this()
            {
                InvertDirection = bInverted;
                this.Name = name;
                this.PortNumber = port;
                _pca9685 = pca9685;
            }

            #region DoNothing Methods
            public override bool Initialize()
            {
                //port does nothing
                return true;
            }

            public override bool Enable()
            {
                //port does nothing
                return true;
            }

            public override bool Disable()
            {
                //port does nothing
                return true;
            }

            public override bool Shutdown()
            {
                //port does nothing
                return true;
            }
            public override bool CancelApplyState()
            {
                //port does nothing
                return true;
            }

            #endregion

            public ServoPort() : base(Convert.ToDecimal(servoMaxPulseLength) / Convert.ToDecimal(100))
            {
                //nothing here - we use the base constructor with our servo values,
            }

            public override void Recalibrate()
            {
                int delay = 500;
                try
                {

                    for (int i = 0; i <= 100; i++)
                    {
                        GotoPosition(i);
                        Task.Delay(200).Wait();
                    }
                    GoHomePosition();
                }
                catch (Exception exAny)
                {
                    string msg = string.Format(
                        "Errror: {0}",
                        exAny.ToString());
                    string s = "";
                }
            }

            public override void GoHomePosition()
            {
                GotoPosition(0);
            }

            public override OwlDeviceStateBase GetState()
            {
                throw new NotImplementedException();
            }

            public override void ApplyState(OwlDeviceStateBase state)
            {
                throw new NotImplementedException();
            }


            public override void DoPositionList(List<int> positions, int msDelayBetween, bool returnToHome)
            {
                throw new NotImplementedException();
            }

            public override void GotoPosition(int position)
            {
                if (position < 0 || position > 100)
                    throw new ArgumentOutOfRangeException("position", position, "Position must be between 0-100");

                //Might need to invert the range for left eye vs. right eye.
                //double angle = 100d / 180d * (double)position;
                double angle = 1.8d * (double)position;

                var ticks = Convert.ToUInt16(120 + (2.7d * angle));
                if (ticks > 606)
                {
                    FireDeviceError($"ticks {ticks} greater than {602} aka 180 degrees");
                }
                else
                {
                    if (InvertDirection)
                        ticks = Convert.ToUInt16(606 - ticks);

                    _pca9685.SetPin(PortNumber, ticks, false);
                }
            }
        }

    }
}
