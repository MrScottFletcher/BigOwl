//using Adafruit.PCA9685;
//using BigOwl.Entity;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Windows.Devices.Enumeration;
//using Windows.Devices.I2c;

//namespace BigOwl.Devices
//{
//    public class ServoBoardDriver
//    {
//        public readonly PCA9685PWMBreakout _pca9685;
//        private static ushort servoMinPulseLength = 110;
//        private static ushort servoMaxPulseLength = 450;
//        private static ushort servoMaxPulseLength_UPPER = 600;

//        public ServoBoardDriver()
//        {
//            _pca9685 = new PCA9685PWMBreakout();
//        }


//        public void Initialize()
//        {
//            if (false)
//            {
//                throw new ApplicationException("ServoBoardDriver FindDevicesAsync() failed to complete.");
//            }
//            else
//            {

//                //Task t1 = _pca9685.Initialize();
//                Task.Run(async () => { await _pca9685.Initialize(); }).Wait();
//                Task.Run(async () => { await _pca9685.SetFrequency(50); }).Wait();

//                _pca9685.SetPwm(0, .50f);
//                _pca9685.SetPwm(1, .50f);

//                ////how do we know that it is ready?!!??!?!

//                //DateTime start1 = DateTime.Now;

//                //while ((t1.Status == TaskStatus.WaitingForActivation ||
//                //    t1.Status == TaskStatus.WaitingForChildrenToComplete ||
//                //    t1.Status == TaskStatus.WaitingToRun) &&
//                //    (DateTime.Now - start1).TotalMinutes < 2 && !t1.IsCompleted)

//                //{
//                //    System.Threading.Thread.Sleep(1000);
//                //}

//                //if (!t1.IsCompleted)
//                //{
//                //    throw new ApplicationException("ServoBoardDriver _pca9685.InitI2CAsync failed to complete.");
//                //}
//                //else
//                //{
//                //    _pca9685.SetFrequency(50).Wait();
//                //    _pca9685.SetPwm(0, servoMinPulseLength);
//                //    return;
//                //}
//            }

//        }


//        public class ServoPort : OwlControllerBase
//        {
//            private readonly PCA9685PWMBreakout _pca9685;

//            public int PortNumber { get; set; }
//            public ServoPort(string name, int port, PCA9685PWMBreakout pca9685)
//            {
//                this.Name = name;
//                this.PortNumber = port;
//                _pca9685 = pca9685;
//            }

//            public byte PortByte
//            {
//                get
//                {
//                    return Convert.ToByte(PortNumber);
//                }
//            }


//            #region DoNothing Methods
//            public override bool Initialize()
//            {
//                //port does nothing
//                return true;
//            }

//            public override bool Enable()
//            {
//                //port does nothing
//                return true;
//            }

//            public override bool Disable()
//            {
//                //port does nothing
//                return true;
//            }

//            public override bool Shutdown()
//            {
//                //port does nothing
//                return true;
//            }
//            public override bool CancelApplyState()
//            {
//                //port does nothing
//                return true;
//            }

//            #endregion

//            public override void Recalibrate()
//            {
//                int delay = 500;
//                try
//                {
//                    _pca9685.SetPwm(0, 0);
//                    Task.Delay(delay);
//                    _pca9685.SetPwm(0, 1);
//                    Task.Delay(delay);
//                    _pca9685.SetPwm(0, .2f);
//                    Task.Delay(delay);
//                    _pca9685.SetPwm(0, .4f);
//                    Task.Delay(delay);
//                    _pca9685.SetPwm(0, .5f);
//                    Task.Delay(delay);
//                    _pca9685.SetPwm(0, 1);
//                    Task.Delay(delay);
//                    _pca9685.SetPwm(0, 0);
//                    Task.Delay(delay);
//                    _pca9685.SetPwm(0, 1);
//                    Task.Delay(delay);
//                    _pca9685.SetPwm(0, 0);

//                    for (float i = 1; i < 100; i++)
//                    {
//                        _pca9685.SetPwm(0, i / 100f);
//                        Task.Delay(1);
//                    }

//                    _pca9685.SetPwm(0, 0);
//                }
//                catch (Exception exAny)
//                {
//                    string msg = string.Format(
//                        "Errror: {0}",
//                        exAny.ToString());
//                    string s = "";
//                }
//            }

//            public override void GoHomePosition()
//            {
//                throw new NotImplementedException();
//            }

//            public override OwlDeviceStateBase GetState()
//            {
//                throw new NotImplementedException();
//            }

//            public override void ApplyState(OwlDeviceStateBase state)
//            {
//                throw new NotImplementedException();
//            }


//            public override void DoPositionList(List<int> positions, int msDelayBetween, bool returnToHome)
//            {
//                throw new NotImplementedException();
//            }

//            public override void GotoPosition(int position)
//            {
//                _pca9685.SetPwm(PortByte, Convert.ToSingle(position) / 100f);
//            }
//        }

//    }
//}
