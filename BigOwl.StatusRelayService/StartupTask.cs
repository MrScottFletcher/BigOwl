using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace BigOwl.StatusRelayService
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral deferral = null;
        AppServiceConnection connection;

        public void Run(IBackgroundTaskInstance taskInstance)
        {

            deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;
            System.Diagnostics.Debug.WriteLine(Windows.ApplicationModel.Package.Current.Id.FamilyName);

            // 
            // TODO: Insert code to perform background work
            //
            // If you start any asynchronous methods here, prevent the task
            // from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            //

            //Check to determine whether this activation was caused by an incoming app service connection
            var appServiceTrigger = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            if (appServiceTrigger != null)
            {
                //Verify that the app service connection is requesting the "StatusRelayService" that this class provides
                if (appServiceTrigger.Name.Equals("StatusRelayService"))
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
            
            //string requestedPinValue = (string)message["requestedPinValue"];
            //if (message.ContainsKey("requestedPinValue"))
            //{

            //    if (requestedPinValue.Equals("High"))
            //    {
            //        pin.Write(GpioPinValue.High);
            //    }
            //    else if (requestedPinValue.Equals("Low"))
            //    {
            //        pin.Write(GpioPinValue.Low);
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.WriteLine("Reqested pin value is not understood: " + requestedPinValue);
            //        System.Diagnostics.Debug.WriteLine("Valid values are 'High' and 'Low'");
            //    }
            //}
            //else
            //{
            //    System.Diagnostics.Debug.WriteLine("Message not understood");
            //    System.Diagnostics.Debug.WriteLine("Valid command is: requestedPinValue");
            //}

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
