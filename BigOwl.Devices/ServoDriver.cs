using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace BigOwl.Devices
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServoDriver
    {
        public static GpioPin pin;
        public static GpioPinValue pinValue;

        private const int Button_PIN = 21;
        private GpioPin pin1;
        private GpioPinValue pinValue1;

        private const int Button_PIN2 = 20;
        private GpioPin pin2;
        private GpioPinValue pinValue2;

        private static IAsyncAction workItemThread;
        public static GpioController gpio;

        public ServoDriver()
        {
            //this.InitializeComponent();

            gpio = GpioController.GetDefault();

            InitGPIO();
        }

        public static async void PWM_R()
        {
            var stopwatch = Stopwatch.StartNew();

            workItemThread = Windows.System.Threading.ThreadPool.RunAsync(
                 (source) =>
                 {
                     // setup, ensure pins initialized
                     ManualResetEvent mre = new ManualResetEvent(false);
                     mre.WaitOne(1500);

                     ulong pulseTicks = ((ulong)(Stopwatch.Frequency) / 1000) * 2;
                     ulong delta;
                     var startTime = stopwatch.ElapsedMilliseconds;
                     while (stopwatch.ElapsedMilliseconds - startTime <= 300)
                     {
                         pin.Write(GpioPinValue.High);
                         ulong starttick = (ulong)(stopwatch.ElapsedTicks);
                         while (true)
                         {
                             delta = (ulong)(stopwatch.ElapsedTicks) - starttick;
                             if (delta > pulseTicks) break;
                         }
                         pin.Write(GpioPinValue.Low);
                         starttick = (ulong)(stopwatch.ElapsedTicks);
                         while (true)
                         {
                             delta = (ulong)(stopwatch.ElapsedTicks) - starttick;
                             if (delta > pulseTicks * 10) break;
                         }
                     }
                 }, WorkItemPriority.High);
        }

        public static async void PWM_L()
        {
            var stopwatch = Stopwatch.StartNew();

            workItemThread = Windows.System.Threading.ThreadPool.RunAsync(
                 (source) =>
                 {
                     // setup, ensure pins initialized
                     ManualResetEvent mre = new ManualResetEvent(false);
                     mre.WaitOne(1500);

                     ulong pulseTicks = ((ulong)(Stopwatch.Frequency) / 1000) * 2;
                     ulong delta;
                     var startTime = stopwatch.ElapsedMilliseconds;
                     while (stopwatch.ElapsedMilliseconds - startTime <= 300)
                     {
                         pin.Write(GpioPinValue.High);
                         ulong starttick = (ulong)(stopwatch.ElapsedTicks);
                         while (true)
                         {
                             delta = starttick - (ulong)(stopwatch.ElapsedTicks);
                             if (delta > pulseTicks) break;
                         }
                         pin.Write(GpioPinValue.Low);
                         starttick = (ulong)(stopwatch.ElapsedTicks);
                         while (true)
                         {
                             delta = (ulong)(stopwatch.ElapsedTicks) - starttick;
                             if (delta > pulseTicks * 10) break;
                         }
                     }
                 }, WorkItemPriority.High);
        }

        private void InitGPIO()
        {
            if (gpio == null)
            {
                pin = null;
                return;
            }

            pin = gpio.OpenPin(18);
            pinValue = GpioPinValue.High;
            pin.Write(pinValue);
            pin.SetDriveMode(GpioPinDriveMode.Output);
        }

        private void buttonPin_ValueChanged_L(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.FallingEdge)
            {
                PWM_L();
            }
        }

        private void buttonPin_ValueChanged_R(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.FallingEdge)
            {
                PWM_R();
            }
        }

        public async void ButtonLeft()
        {
            PWM_L();
        }

        public async void ButtonRight()
        {
            PWM_R();
        }
    }
}
