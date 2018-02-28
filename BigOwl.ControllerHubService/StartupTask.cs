using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using BigOwl.Entity;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace BigOwl.ControllerHubService
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral deferral = null;
        AppServiceConnection connection;

        public OwlCommandQueue CommandQueue { get; set; }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            CommandQueue = new OwlCommandQueue();

            string name = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            System.Diagnostics.Debug.WriteLine("FamilyName: " + name);

            deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;
            System.Diagnostics.Debug.WriteLine(Windows.ApplicationModel.Package.Current.Id.FamilyName);

            //Check to determine whether this activation was caused by an incoming app service connection
            var appServiceTrigger = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            if (appServiceTrigger != null)
            {
                //Verify that the app service connection is requesting the "StatusRelayService" that this class provides
                if (appServiceTrigger.Name.Equals("BigOwl.ControllerHubService"))
                {
                    //Store the connection and subscribe to the "RequestRecieved" event to be notified when clients send messages
                    connection = appServiceTrigger.AppServiceConnection;
                    connection.RequestReceived += Connection_RequestReceived;
                }
                else
                {
                    deferral.Complete();
                }

            }
        }
        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (deferral != null)
            {
                deferral.Complete();
                deferral = null;
            }
        }

        private void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var messageDeferral = args.GetDeferral();

            //The message is provided as a ValueSet (IDictionary<String,Object)
            //The only message this server understands is with the name "requestedPinValue" and values of "Low" and "High"
            ValueSet message = args.Request.Message;

            if (message.ContainsKey("command"))
            {
                string payload = (string)message["command"];

                OwlCommand c = BigOwl.StatusRelay.RelayClient.DeSerializeOwlCommand(payload);
                //OwlCommand c = (OwlCommand)message["command"];
                if (c != null)
                {
                    System.Diagnostics.Debug.WriteLine("Command: " + c.Command.ToString());
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Command was null.");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("There was no command payload.");
            }

            messageDeferral.Complete();
        }

        public void Shutdown()
        {
            //Tell everyone to relax and disable their controls
            //TODO:
            //When everyone is done, release our soul to the heavens
            deferral.Complete();
        }
    }
}
