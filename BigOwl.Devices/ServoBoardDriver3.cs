//using AdafruitClassLibrary;
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
//        public readonly Pca9685 _pca9685;
//        private static ushort servoMinPulseLength = 110;
//        private static ushort servoMaxPulseLength = 450;
//        private static ushort servoMaxPulseLength_UPPER = 600;

//        public ServoBoardDriver()
//        {
//            _pca9685 = new Pca9685();
//        }


//        public void Initialize()
//        {
//            //The PCA9685 Class has a single constructor. 
//            //The constructor takes an optional argument of an I2C address. 
//            //I2C addresses for the chip are in the range 0x40 to 0x7F.

//            //DateTime start0 = DateTime.Now;
//            //Task t0 = InitI2C();

//            //while ((t0.Status == TaskStatus.WaitingForActivation ||
//            //    t0.Status == TaskStatus.WaitingForChildrenToComplete ||
//            //    t0.Status == TaskStatus.WaitingToRun) &&
//            //    (DateTime.Now - start0).TotalMinutes < 2 && !t0.IsCompleted)

//            //{
//            //    System.Threading.Thread.Sleep(1000);
//            //}


//            //Task.Run(async () => { await _pca9685.InitI2CAsync(I2CBase.I2CSpeed.I2C_400kHz); }).Wait();
//            //Task.Run(async () => { await _pca9685.InitPCA9685Async(I2CBase.I2CSpeed.I2C_400kHz); }).Wait();

//            //InitI2C();

//            //Task.Run(async () => { await FindDevicesAsync(); }).Wait();

//            Task.Run(async () => { await _pca9685.InitI2CAsync(I2CBase.I2CSpeed.I2C_400kHz); }).Wait();
//            Task.Run(async () => { await _pca9685.InitPCA9685Async(I2CBase.I2CSpeed.I2C_400kHz); }).Wait();


//            Task.Delay(10000).Wait();

//            //_pca9685.Reset();
//            _pca9685.SetPWMFrequency(50);
//            _pca9685.SetAllPWM(0, servoMinPulseLength);


//            ////if (!t0.IsCompleted)
//            //if (false)
//            //{
//            //    throw new ApplicationException("ServoBoardDriver FindDevicesAsync() failed to complete.");
//            //}
//            //else
//            //{

//            //    Task t1 = _pca9685.InitI2CAsync(I2CBase.I2CSpeed.I2C_400kHz);

//            //    ////how do we know that it is ready?!!??!?!
//            //    Task.Delay(10000).Wait();

//            //    DateTime start1 = DateTime.Now;

//            //    while ((t1.Status == TaskStatus.WaitingForActivation ||
//            //        t1.Status == TaskStatus.WaitingForChildrenToComplete ||
//            //        t1.Status == TaskStatus.WaitingToRun) &&
//            //        (DateTime.Now - start1).TotalMinutes < 2 && !t1.IsCompleted)

//            //    {
//            //        System.Threading.Thread.Sleep(1000);
//            //    }

//            //    if (!t1.IsCompleted)
//            //    {
//            //        throw new ApplicationException("ServoBoardDriver _pca9685.InitI2CAsync failed to complete.");
//            //    }
//            //    else
//            //    {
//            //        Task t2 = _pca9685.InitPCA9685Async();


//            //        DateTime start2 = DateTime.Now;
//            //        while ((t2.Status == TaskStatus.WaitingForActivation ||
//            //            t2.Status == TaskStatus.WaitingForChildrenToComplete ||
//            //            t2.Status == TaskStatus.WaitingToRun) &&
//            //            (DateTime.Now - start2).TotalMinutes < 2 && !t2.IsCompleted)

//            //        {
//            //            System.Threading.Thread.Sleep(1000);
//            //        }

//            //        if (!t2.IsCompleted)
//            //        {
//            //            throw new ApplicationException("ServoBoardDriver _pca9685.InitPCA9685Async failed to complete.");
//            //        }
//            //        else
//            //        {
//            //            _pca9685.SetPWMFrequency(50);
//            //            _pca9685.SetAllPWM(0, servoMinPulseLength);
//            //            return;
//            //        }
//            //    }
//            //}

//        }

//        private async void InitI2C()
//        {
//            try
//            {
//                var settings = new I2cConnectionSettings(64);
//                settings.BusSpeed = I2cBusSpeed.FastMode;                       /* 400KHz bus speed */

//                string aqs = I2cDevice.GetDeviceSelector();                     /* Get a selector string that will return all I2C controllers on the system */
//                var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the I2C bus controller devices with our selector string             */
//                var I2CAccel = await I2cDevice.FromIdAsync(dis[0].Id, settings);    /* Create an I2cDevice with our selected bus controller and I2C settings    */
//                if (I2CAccel == null)
//                {
//                    string msg = string.Format(
//                        "Slave address {0} on I2C Controller {1} is currently in use by " +
//                        "another application. Please ensure that no other applications are using I2C.",
//                        settings.SlaveAddress,
//                        dis[0].Id);
//                    return;
//                }
//            }
//            catch (Exception ex)
//            {
//                string msg = string.Format(
//                    "Errror: {0}",
//                    ex.ToString());
//                return;
//            }
//        }



//        #region a bunch of junk
//        //protected async Task InitI2C()
//        //{
//        //    try
//        //    {
//        //        var settings = new I2cConnectionSettings(64);
//        //        settings.BusSpeed = I2cBusSpeed.FastMode;                       /* 400KHz bus speed */

//        //        string aqs = I2cDevice.GetDeviceSelector();                     /* Get a selector string that will return all I2C controllers on the system */
//        //        var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the I2C bus controller devices with our selector string             */
//        //        var I2CAccel = await I2cDevice.FromIdAsync(dis[0].Id, settings);    /* Create an I2cDevice with our selected bus controller and I2C settings    */
//        //        if (I2CAccel == null)
//        //        {
//        //            string msg = string.Format(
//        //                "Slave address {0} on I2C Controller {1} is currently in use by " +
//        //                "another application. Please ensure that no other applications are using I2C.",
//        //                settings.SlaveAddress,
//        //                dis[0].Id);
//        //            return;
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        string msg = string.Format(
//        //            "Errror: {0}",
//        //            ex.ToString());
//        //        return;
//        //    }
//        //}


//        public static async Task<IEnumerable<byte>> FindDevicesAsync()
//        {
//            IList<byte> returnValue = new List<byte>();
//            IList<int> intValues = new List<int>();
//            IList<string> strValues = new List<string>();

//            // *** 
//            // *** Get a selector string that will return all I2C controllers on the system 
//            // *** 
//            string aqs = I2cDevice.GetDeviceSelector();
//            // *** 
//            // *** Find the I2C bus controller device with our selector string 
//            // *** 
//            var dis = await DeviceInformation.FindAllAsync(aqs).AsTask();
//            int count = dis.Count;
//            if (count > 0)
//            {

//                foreach (var d in dis)
//                {
//                    string id = d.Id.ToString();
//                    string name = d.Name;
//                    string isEnabled = d.IsEnabled.ToString();
//                    string t = "";
//                    string kind = d.Kind.ToString();

//                    StringBuilder sbProps = new StringBuilder();

//                    if (d.Properties != null)
//                    {
//                        d.Properties.ToList().ForEach(p =>
//                        {
//                            string val = (p.Value != null) ? p.Value.ToString() : "(null)";
//                            sbProps.AppendLine(p.Key + ":" + val);
//                        });
//                    }
//                    else
//                    {
//                        sbProps.AppendLine("Properties were null");
//                    }
//                    System.Diagnostics.Debug.WriteLine(String.Format("id:{0}, Name:{1}, isEnabled:{2}, kind:{3}", id, name, isEnabled.ToString(), kind));
//                    System.Diagnostics.Debug.WriteLine("Properties: " + sbProps.ToString());
//                }

//                string idString = dis[0].Id.ToString();

//                //64 - 127
//                const int minimumAddress = 64;
//                //const int maximumAddress = 77;
//                const int maximumAddress = 127;
//                for (byte address = minimumAddress; address <= maximumAddress; address++)
//                {
//                    var settings = new I2cConnectionSettings(address);
//                    settings.BusSpeed = I2cBusSpeed.FastMode;
//                    settings.SharingMode = I2cSharingMode.Shared;
//                    // *** 
//                    // *** Create an I2cDevice with our selected bus controller and I2C settings 
//                    // *** 
//                    I2cDevice device = null;

//                    try
//                    {

//                        device = await I2cDevice.FromIdAsync(idString, settings); ;

//                        if (device != null)
//                        {
//                            System.Diagnostics.Debug.WriteLine(String.Format("DETECTED: Address: {0} - id:{1}", address.ToString(), device.DeviceId.ToString()));
//                        }

//                        //device = await I2cDevice.FromIdAsync(idString, settings);
//                        //Task<I2cDevice> t1 = I2cDevice.FromIdAsync(idString, settings).AsTask();

//                        //DateTime start1 = DateTime.Now;

//                        //while ((t1.Status == TaskStatus.WaitingForActivation ||
//                        //     t1.Status == TaskStatus.WaitingForChildrenToComplete ||
//                        //     t1.Status == TaskStatus.WaitingToRun) &&
//                        //     (DateTime.Now - start1).TotalMinutes < 2 && !t1.IsCompleted)
//                        //{
//                        //    System.Threading.Thread.Sleep(1000);
//                        //}

//                        //if (!t1.IsCompleted)
//                        //{
//                        //    //nothing here
//                        //    //throw new ApplicationException("I2cDevice.FromIdAsync failed to complete.");
//                        //}
//                        //else
//                        //{
//                        //    device = t1.Result;
//                        //    if (device != null)
//                        //    {
//                        //        System.Diagnostics.Debug.WriteLine(String.Format("DETECTED: Address: {0} - id:{1}", address.ToString(), device.DeviceId.ToString()));
//                        //    }
//                        //}
//                    }
//                    catch (AggregateException exAgg)
//                    {
//                        string msg = string.Format(
//                            "Agg Errror: {0}",
//                            exAgg.ToString());
//                        return returnValue;
//                    }
//                    catch (Exception exAny)
//                    {
//                        string msg = string.Format(
//                            "Errror: {0}",
//                            exAny.ToString());
//                        return returnValue;
//                    }
//                    if (device != null)
//                    {
//                        try
//                        {
//                            byte[] writeBuffer = new byte[1] { 0 };
//                            device.Write(writeBuffer);
//                            // *** 
//                            // *** If no exception is thrown, there is 
//                            // *** a device at this address. 
//                            // *** 
//                            returnValue.Add(address);
//                            strValues.Add(idString);
//                        }
//                        catch
//                        {
//                            // *** 
//                            // *** If the address is invalid, an exception will be thrown. 
//                            // *** 
//                        }
//                        device.Dispose();
//                    }
//                }
//            }
//            return returnValue;
//        }

//        #endregion

//        public class ServoPort : OwlControllerBase
//        {
//            private readonly Pca9685 _pca9685;

//            public int PortNumber { get; set; }
//            public ServoPort(string name, int port, Pca9685 pca9685)
//            {
//                this.Name = name;
//                this.PortNumber = port;
//                _pca9685 = pca9685;
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

//            public ServoPort()
//            {
//                StepToPositionFactor = Convert.ToDecimal(servoMaxPulseLength) / Convert.ToDecimal(100);
//            }

//            public override void Recalibrate()
//            {
//                int delay = 500;
//                try
//                {
//                    _pca9685.SetPin(0, 0, false);
//                    Task.Delay(delay);
//                    _pca9685.SetPin(0, servoMinPulseLength, false);
//                    Task.Delay(delay);
//                    _pca9685.SetPin(0, 200, false);
//                    Task.Delay(delay);
//                    _pca9685.SetPin(0, 300, false);
//                    Task.Delay(delay);
//                    _pca9685.SetPin(0, 400, false);
//                    Task.Delay(delay);
//                    _pca9685.SetPin(0, servoMaxPulseLength, false);
//                    Task.Delay(delay);
//                    _pca9685.SetPin(0, servoMinPulseLength, false);
//                    Task.Delay(delay);
//                    _pca9685.SetPin(0, servoMaxPulseLength, false);
//                    Task.Delay(delay);
//                    _pca9685.SetPin(0, servoMinPulseLength, false);

//                    for (ushort i = servoMinPulseLength; i < servoMaxPulseLength; i++)
//                    {
//                        _pca9685.SetPin(0, i, false);
//                        Task.Delay(1);
//                    }

//                    _pca9685.SetPin(0, servoMinPulseLength, false);
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
//                if (position < 0 || position > 100)
//                    throw new ArgumentOutOfRangeException("position", position, "Position must be between 0-100");

//                _pca9685.SetPin(0, Convert.ToUInt16(Convert.ToDecimal(position) * StepToPositionFactor.Value), false);
//            }
//        }

//    }
//}
