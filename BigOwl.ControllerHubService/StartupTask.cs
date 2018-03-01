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
        //AppServiceConnection connection;

        BigOwl.StatusRelay.RelayClient relayClient;

        private static object _lockObj = new object();
        private static OwlCommandQueue _commandQueue;

        public void Run(IBackgroundTaskInstance taskInstance)
        {

            string name = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            System.Diagnostics.Debug.WriteLine("FamilyName: " + name);

            deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;
            System.Diagnostics.Debug.WriteLine(Windows.ApplicationModel.Package.Current.Id.FamilyName);

            relayClient = new StatusRelay.RelayClient();
            relayClient.OnMessageReceived += RelayClient_OnMessageReceived;   
            
            deferral.Complete();
        }

        private OwlCommandQueue CommandQueue()
        {
            if(_commandQueue == null)
            {
                if(_lockObj == null)
                {
                    _lockObj = new object();
                }
                lock (_lockObj)
                {
                    _commandQueue = new OwlCommandQueue();
                }
            }

            return _commandQueue;
        }


        private void RelayClient_OnMessageReceived(ValueSet message)
        {


            if (message.ContainsKey("command"))
            {
                string payload = (string)message["command"];

                OwlCommand c = BigOwl.StatusRelay.RelayClient.DeSerializeOwlCommand(payload);
                //OwlCommand c = (OwlCommand)message["command"];
                if (c != null)
                {
                    CommandQueue().Add(c);

                    System.Diagnostics.Debug.WriteLine("Command: " + c.Command.ToString());
                    relayClient.SendAck(c.Id.ToString());
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Command was null.");
                    relayClient.SendNack(c.Id.ToString());
                }
            }
            else if (message.ContainsKey("ack"))
            {
            }
            else if (message.ContainsKey("nack"))
            {
            }
            else
            {
                //was something else.
            }

        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            relayClient.SendTerminating();
            relayClient.CloseConnection();

            if (deferral != null)
            {
                deferral.Complete();
                deferral = null;
            }
        }

        public void Shutdown()
        {
            //Tell everyone to relax and disable their controls
            //TODO:

            relayClient.SendTerminating();
            relayClient.CloseConnection();

            //When everyone is done, release our soul to the heavens
            deferral.Complete();
        }
    }
}
