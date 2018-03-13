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
        private OwlMasterController.Owl _owl;

        public MainPage()
        {
            this.InitializeComponent();

            _owl.DeviceError += _component_DeviceError;
            _owl.MoveCompleted += _component_MoveCompleted;

            _owl.Initialize();

        }

        private void _component_MoveCompleted(object sender)
        {
            var ignored = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Do something on the dispatcher thread
                statusLabel.Text = DateTime.Now.ToString() + " --  Move Completed";
            });
        }

        private void _component_DeviceError(object sender, string error)
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

            _owl.Recalibrate();

            var ignored2 = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Do something on the dispatcher thread
                statusLabel.Text = DateTime.Now.ToString() + " -- TestButton_Finished";
            });
        }

        private void DoThingsButton_Click(object sender, RoutedEventArgs e)
        {
            var ignored = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Do something on the dispatcher thread
                statusLabel.Text = DateTime.Now.ToString() + " -- DoThingsButton_Click START";
            });

            BigOwl.Entity.StepperState state = new Entity.StepperState();

            List<int> positions = new List<int>();

            positions.Add(0);
            positions.Add(100);
            positions.Add(50);
            positions.Add(75);
            positions.Add(25);

            _owl.DoPositionList(positions, 1, true);

            var ignored2 = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Do something on the dispatcher thread
                statusLabel.Text = DateTime.Now.ToString() + " -- DoThingsButton_Click FINISHED";
            });
        }
    }
}
