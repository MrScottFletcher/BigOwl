using BigOwl.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
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

namespace BigOwl.TestControllerApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //AppServiceConnection connection;

        private static BigOwl.StatusRelay.RelayClient relayClient = StatusRelay.RelayClient.Instance;


        public MainPage()
        {
            this.InitializeComponent();
            relayClient.OnMessageReceived += RelayClient_OnMessageReceived;
        }

        private async void RelayClient_OnMessageReceived(ValueSet obj)
        {
            StringBuilder sb = new StringBuilder();
            obj.Keys.ToList().ForEach(v => sb.AppendLine(v.ToString()));

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                // Do something on the dispatcher thread
                relayMessageTextBlock.Text = DateTime.Now.ToString() + " --  " + sb.ToString();
            });

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            MediaElement mediaElement = new MediaElement();
            var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            Windows.Media.SpeechSynthesis.SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync("Hello, Owl!");
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            DoStuffOwlClient();
        }

        private async void DoStuffOwlClient()
        {
            if (relayClient == null)
                relayClient = new StatusRelay.RelayClient();

            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.RandomFull;


            if (!relayClient.IsConnected)
            {
                ExceptionDispatchInfo capturedException = null;
                try
                {
                    await relayClient.Open();
                }
                catch (Exception ex)
                {
                    capturedException = ExceptionDispatchInfo.Capture(ex);
                }

                string exMessage = String.Empty;

                if (capturedException != null)
                {
                    //await ExceptionHandler();
                    //capturedException.Throw();
                    exMessage += capturedException.SourceException.ToString();
                }

                if (!relayClient.IsConnected)
                {
                    //deferral.Complete();
                    TestButton.Content = "Bad: Relay Client connection is not open -- " + DateTime.Now.ToString() + " -- Error: " + exMessage;
                    return;
                }
            }
            //if we made it here, do the message
            await relayClient.SendOwlCommandAsync(c);
        }

        private async void WinkButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("WinkButton_Click Start");
            RunEyeTest();
            SetStatusLabel("WinkButton_Click FINISHED");
        }

        private async void WiggleButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("WiggleButton_Click Start");
            await RunWiggleTest();
            SetStatusLabel("WiggleButton_Click FINISHED");
        }

        private async void HeadLeftButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("HeadLeftButton_Click Start");
            await RunHeadLeftTest();
            SetStatusLabel("HeadLeftButton_Click FINISHED");
        }


        private async void HeadRightButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("HeadRightButton_Click Start");
            await RunHeadRightTest();
            SetStatusLabel("HeadRightButton_Click FINISHED");
        }

        private async void WingFlapButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("WingFlapButton_Click Start");
            await RunWingFlapTest();
            SetStatusLabel("WingFlapButton_Click FINISHED");
        }

        private async void CalibrateAllButton_Click(object sender, RoutedEventArgs e)
        {
            SetStatusLabel("CalibrateAllButton_Click Start");
            await RunCalibrationTest();
            SetStatusLabel("CalibrateAllButton_Click FINISHED");
        }

        private async Task SetStatusLabel(string msg)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                // Do something on the dispatcher thread
                statusLabel.Text = DateTime.Now.ToString() + " -- " + msg;
                await BlinkStatusLight();
            });
        }


        private async Task RunCalibrationTest()
        {
            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.Recalibrate;
            await SendCommand(c);
        }


        private async Task RunEyeTest()
        {
            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.Wink;
            await SendCommand(c);
        }

        private async Task RunWiggleTest()
        {
            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.SmallWiggle;
            await SendCommand(c);
        }

        private async Task RunHeadLeftTest()
        {
            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.HeadLeft;
            await SendCommand(c);
        }

        private async Task RunHeadRightTest()
        {
            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.HeadRight;
            await SendCommand(c);
        }

        private async Task RunWingFlapTest()
        {
            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.Flap;
            await SendCommand(c);
        }

        private async Task BlinkStatusLight()
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
            throw new NotImplementedException("Individual part calibration is not supported.");
            CalibratePart("Head");
        }

        private void CalibrateWingsButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException("Individual part calibration is not supported.");
            CalibratePart("Wings");
        }

        private void CalibrateLEFTEyeButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException("Individual part calibration is not supported.");
            CalibratePart("Left eye");
        }

        private void CalibrateRIGHTEyeButton_Copy1_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException("Individual part calibration is not supported.");
            CalibratePart("Right eye");
        }

        private void CalibratePart(string partName)
        {
            throw new NotImplementedException("Individual part calibration is not supported.");
            //var ignored2 = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //    OwlControllerBase part = _owl.PartsList.Find(p => p.Name.ToLower() == partName.ToLower());
            //    if (part != null)
            //        part.Recalibrate();
            //    else
            //    {
            //        SetStatusLabel("Error calibrating " + partName + " - part not found.");
            //    }
            //});
        }

        private async Task SendCommand(OwlCommand c)
        {

            if (!relayClient.IsConnected)
            {
                //deferral.Complete();
                TestButton.Content = "Bad: Relay Client connection is not open -- " + DateTime.Now.ToString();
                return;
            }

            await relayClient.SendOwlCommandAsync(c);
        }


        //private async void DoStuffManualConnection()
        //{
        //    connection = new AppServiceConnection();
        //    connection.AppServiceName = "BigOwl.ControllerHubService";
        //    connection.PackageFamilyName = "BigOwlControllerHubService-uwp_rtvxam156657y";
        //    AppServiceConnectionStatus status = await connection.OpenAsync();

        //    if (status != AppServiceConnectionStatus.Success)
        //    {
        //        //deferral.Complete();
        //        TestButton.Content = "Bad: " + status.ToString() + " -- " + DateTime.Now.ToString();
        //        return;
        //    }

        //    OwlCommand c = new OwlCommand();
        //    c.Command = OwlCommand.Commands.RandomFull;

        //    var message = new ValueSet();
        //    message.Add("command", BigOwl.StatusRelay.RelayClient.Serialize<OwlCommand>(c));

        //    AppServiceResponse response = await connection.SendMessageAsync(message);

        //    //If the message was successful, start a timer to send alternating requestedPinValues to blink the LED
        //    if (response.Status == AppServiceResponseStatus.Success)
        //    {
        //        TestButton.Content = "Success " + DateTime.Now.ToString();
        //    }
        //}


    }
}
