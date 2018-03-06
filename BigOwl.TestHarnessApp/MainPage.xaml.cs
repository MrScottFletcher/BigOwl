using BigOwl.Devices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BigOwl.TestHarnessApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private StepperControl.StepperController _head;
        private StepperControl.StepperController _wings;
        private PwmControl.PwmController _leftEye;
        private PwmControl.PwmController _rightEye;

        public MainPage()
        {
            this.InitializeComponent();

            _head = new StepperControl.StepperController(19,26,13,6,5,21,true);

            _head.DeviceError += _head_DeviceError;
            _head.MoveCompleted += _head_MoveCompleted;

            _head.Initialize();

        }

        private void _head_MoveCompleted(object sender)
        {
            var ignored = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Do something on the dispatcher thread
                statusLabel.Text = DateTime.Now.ToString() + " --  Move Completed";
            });
        }

        private void _head_DeviceError(object sender, string error)
        {
            var ignored = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Do something on the dispatcher thread
                statusLabel.Text = DateTime.Now.ToString() + " -- Error";
            });
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            var ignored = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Do something on the dispatcher thread
                statusLabel.Text = DateTime.Now.ToString() + " -- TestButton_Click START";
            });

            _head.Recalibrate();

            var ignored2 = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Do something on the dispatcher thread
                statusLabel.Text = DateTime.Now.ToString() + " -- TestButton_Finished";
            });
        }
    }
}
