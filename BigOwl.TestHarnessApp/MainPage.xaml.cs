using BigOwl.Devices;
using BigOwl.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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

            _owl = new OwlMasterController.Owl();

            _owl.DeviceError += _component_DeviceError;
            _owl.MoveCompleted += _component_MoveCompleted;

            _owl.DeviceInfoMessage += _owl_DeviceInfoMessage;

            _owl.Initialize();

            SetStatusLight(Colors.Green);

        }

        private void _owl_DeviceInfoMessage(object sender, string message)
        {
            SetStatusLabel("DEVICE MESSAGE: " + message);
        }

        private void _component_MoveCompleted(object sender)
        {
            SetStatusLabel("Move Completed");
        }

        private void _component_DeviceError(object sender, string error)
        {
            SetStatusLabel("Error");
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("TestButton_Click START");
            _owl.Recalibrate();
            SetStatusLabel("TestButton_Finished");
        }

        private void WinkButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("WinkButton_Click Start");
            RunEyeTest();
            SetStatusLabel("WinkButton_Click FINISHED");
        }

        private void WiggleButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("WiggleButton_Click Start");
            RunWiggleTest();
            SetStatusLabel("WiggleButton_Click FINISHED");
        }

        private void HeadLeftButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("HeadLeftButton_Click Start");
            RunHeadLeftTest();
            SetStatusLabel("HeadLeftButton_Click FINISHED");
        }


        private void HeadRightButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("HeadRightButton_Click Start");
            RunHeadRightTest();
            SetStatusLabel("HeadRightButton_Click FINISHED");
        }

        private void WingFlapButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("WingFlapButton_Click Start");
            RunWingFlapTest();
            SetStatusLabel("WingFlapButton_Click FINISHED");
        }

        private void SetStatusLabel(string msg)
        {
            var ignored2 = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Do something on the dispatcher thread
                statusLabel.Text = DateTime.Now.ToString() + " -- " + msg;
                BlinkStatusLight();
            });
        }

        private void RunEyeTest()
        {
            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.Wink;
            _owl.RunCommand(c);
        }

        private void RunWiggleTest()
        {
            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.SmallWiggle;
            _owl.RunCommand(c);
        }

        private void RunHeadLeftTest()
        {
            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.HeadLeft;
            _owl.RunCommand(c);
        }

        private void RunHeadRightTest()
        {
            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.HeadRight;
            _owl.RunCommand(c);
        }

        private void RunWingFlapTest()
        {
            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.Flap;
            _owl.RunCommand(c);
        }


        private void RunThroughSteps()
        {
            BigOwl.Entity.StepperState state = new Entity.StepperState();

            List<int> positions = new List<int>();

            positions.Add(0);
            positions.Add(100);
            positions.Add(50);
            positions.Add(75);
            positions.Add(25);

            _owl.DoPositionList(positions, 1, true);
        }

        private async void Speak(string message)
        {
            BlinkStatusLight();
            return;

            //not enough memory?
            //MediaElement mediaElement = new MediaElement();
            //var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            //Windows.Media.SpeechSynthesis.SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(message);
            //mediaElement.SetSource(stream, stream.ContentType);
            //mediaElement.Play();
        }

        private async void BlinkStatusLight()
        {
            SetStatusLight(Colors.Red);
            await Task.Delay(500);
            SetStatusLight(Colors.Green);
        }

        private async void SetStatusLight(Color c)
        {
            this.statusLight.Fill = new SolidColorBrush(c);
        }

        private void CalibrateHeadButton_Click(object sender, RoutedEventArgs e)
        {
            CalibratePart("Head");
        }

        private void CalibrateWingsButton_Click(object sender, RoutedEventArgs e)
        {
            CalibratePart("Wings");
        }

        private void CalibrateLEFTEyeButton_Click(object sender, RoutedEventArgs e)
        {
            CalibratePart("Left eye");
        }

        private void CalibrateRIGHTEyeButton_Copy1_Click(object sender, RoutedEventArgs e)
        {
            CalibratePart("Right eye");
        }

        private void CalibratePart(string partName)
        {
            var ignored2 = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                OwlControllerBase part = _owl.PartsList.Find(p => p.Name.ToLower() == partName.ToLower());
                if (part != null)
                    part.Recalibrate();
                else
                {
                    SetStatusLabel("Error calibrating " + partName + " - part not found.");
                }
            });
        }


    }
}
