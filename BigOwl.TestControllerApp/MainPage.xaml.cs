using BigOwl.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        AppServiceConnection connection;

        public MainPage()
        {
            this.InitializeComponent();
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
            DoStuff();
        }

        private async void DoStuff()
        {

            //connection.AppServiceName = "BigOwlStatusRelayService";
            //connection.PackageFamilyName = "BigOwl.StatusRelayService-uwp_rtvxam156657y"

            connection = new AppServiceConnection();
            connection.AppServiceName = "BigOwl.ControllerHubService";
            connection.PackageFamilyName = "BigOwl.ControllerHubService-uwp_rtvxam156657y";
            AppServiceConnectionStatus status = await connection.OpenAsync();

            if (status != AppServiceConnectionStatus.Success)
            {
                //deferral.Complete();
                TestButton.Content = "Bad: " + status.ToString() + " -- " + DateTime.Now.ToString();
                return;
            }

            //Send a message with the name "requestedPinValue" and the value "High"
            //These work like loosely typed input parameters to a method
            //requestedPinValue = "High";
            var message = new ValueSet();
            OwlCommand c = new OwlCommand();

            c.Command = OwlCommand.Commands.RandomFull;
            message["command"] = c;
            AppServiceResponse response = await connection.SendMessageAsync(message);

            //If the message was successful, start a timer to send alternating requestedPinValues to blink the LED
            if (response.Status == AppServiceResponseStatus.Success)
            {
                TestButton.Content = "Success " + DateTime.Now.ToString();
            }
        }
    }
}
