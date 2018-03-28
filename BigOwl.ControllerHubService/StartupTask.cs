using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using BigOwl.Entity;
using System.Threading.Tasks;
using Windows.Storage;
using System.Threading;
using System.IO;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace BigOwl.ControllerHubService
{
    public sealed class StartupTask : IBackgroundTask
    {
        //https://docs.microsoft.com/en-us/windows/uwp/launch-resume/run-in-the-background-indefinetly

        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);

        private static BigOwl.StatusRelay.RelayClient relayClient = StatusRelay.RelayClient.Instance;

        BackgroundTaskDeferral deferral = null;

        private static object _lockObj = new object();
        private static OwlCommandQueue _commandQueue = OwlCommandQueue.Instance;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            string name = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            System.Diagnostics.Debug.WriteLine("FamilyName: " + name);

            deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;
            System.Diagnostics.Debug.WriteLine(Windows.ApplicationModel.Package.Current.Id.FamilyName);

            _commandQueue.CommandAdded += _commandQueue_CommandAdded;
            _commandQueue.QueueChanged += _commandQueue_QueueChanged;
            _commandQueue.QueueEmpty += _commandQueue_QueueEmpty;

            relayClient.OnMessageReceived += RelayClient_OnMessageReceived;
            Write("DEBUG", "Startup");
        }

        private async void _commandQueue_QueueEmpty(object sender)
        {
            await Write("DEBUG", "_commandQueue_QueueEmpty");
        }

        private async void _commandQueue_QueueChanged(object sender)
        {
            await Write("DEBUG", "_commandQueue_QueueChanged");
        }

        private async void _commandQueue_CommandAdded(object sender, OwlCommand command)
        {
            await Write("DEBUG", "_commandQueue_CommandAdded");
        }

        /// <summary>
        /// Writes logs to the LocalFolder.  On Windows IoT on a Pi this would be like:
        /// '\User Folders\LocalAppData\UwpMessageRelay.MessageRelay-uwp_1.0.0.0_arm__n7wdzm614gaee\LocalState\MessageRelayLogs'
        /// On an x86 Windows machine it would be something like:
        /// C:\Users\[user]\AppData\Local\Packages\UwpMessageRelay.MessageRelay-uwp_n7wdzm614gaee\LocalState\MessageRelayLogs
        /// </summary>
        private async Task Write(string level, string message)
        {
            var logFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ControllerHubServiceLogs",
                CreationCollisionOption.OpenIfExists);
            var messageRelayLogsPath = Path.Combine(logFolder.Path, "ControllerHubService.txt");
            var contents = $"{DateTime.Now} - {level} - {message}{Environment.NewLine}";
            await Semaphore.WaitAsync();
            try
            {
                File.AppendAllText(messageRelayLogsPath, contents);
            }
            finally
            {
                Semaphore.Release(1);
            }
        }

        private OwlCommandQueue CommandQueue()
        {
            if (_commandQueue == null)
            {
                if (_lockObj == null)
                {
                    _lockObj = new object();
                }
                lock (_lockObj)
                {
                    _commandQueue = OwlCommandQueue.Instance;
                }
            }

            return _commandQueue;
        }


        private void RelayClient_OnMessageReceived(ValueSet message)
        {

            Write("DEBUG", "RelayClient_OnMessageReceived");
            if (message.ContainsKey("command"))
            {
                string payload = (string)message["command"];

                OwlCommand c = BigOwl.StatusRelay.RelayClient.DeSerializeOwlCommand(payload);
                //OwlCommand c = (OwlCommand)message["command"];
                if (c != null)
                {
                    CommandQueue().Add(c);
                    string msg = "Command: " + c.Command.ToString();
                    Write("DEBUG", msg);
                    System.Diagnostics.Debug.WriteLine("Command: " + msg);
                    relayClient.SendAck(c.Id.ToString());
                }
                else
                {
                    Write("DEBUG", "command was null");
                    System.Diagnostics.Debug.WriteLine("Command was null.");
                    relayClient.SendNack(c.Id.ToString());
                }
            }
            else if (message.ContainsKey("ack"))
            {
                Write("DEBUG", "message was ack");
            }
            else if (message.ContainsKey("nack"))
            {
                Write("DEBUG", "message was nack");
            }
            else
            {
                //was something else.
            }

        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            Write("DEBUG", "TaskInstance_Canceled");
            //relayClient.SendTerminating();
            //relayClient.CloseConnection();

            //if (deferral != null)
            //{
            //    deferral.Complete();
            //    deferral = null;
            //}
        }

        public void Shutdown()
        {
            //Tell everyone to relax and disable their controls
            //TODO:
            Write("DEBUG", "Shutdown");

            //relayClient.SendTerminating();
            //relayClient.CloseConnection();

            //When everyone is done, release our soul to the heavens
            deferral.Complete();
        }
    }
}
