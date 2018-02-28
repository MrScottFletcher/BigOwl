using BigOwl.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text;
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
        //AppServiceConnection connection;

        BigOwl.StatusRelay.RelayClient relayClient;


        public MainPage()
        {
            this.InitializeComponent();
            relayClient = new StatusRelay.RelayClient();
            relayClient.OnMessageReceived += RelayClient_OnMessageReceived;
        }

        private void RelayClient_OnMessageReceived(ValueSet obj)
        {
            StringBuilder sb = new StringBuilder();
            obj.Values.ToList().ForEach(v => sb.AppendLine(v.ToString()));
            relayMessageTextBlock.Text = DateTime.Now.ToString() + " --  " +  sb.ToString();
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
            relayClient = new StatusRelay.RelayClient();

            OwlCommand c = new OwlCommand();
            c.Command = OwlCommand.Commands.RandomFull;


            if (!relayClient.IsConnected)
            {
                await relayClient.Open();
                if (!relayClient.IsConnected)
                {
                    //deferral.Complete();
                    TestButton.Content = "Bad: Relay Client connection is not open -- " + DateTime.Now.ToString();
                    return;
                }
            }
            await relayClient.SendOwlCommand(c);
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
