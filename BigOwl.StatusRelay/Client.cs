using BigOwl.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace BigOwl.StatusRelay
{
    public class RelayClient
    {
        const string AppServiceName = "BigOwl.ControllerHubService";
        private AppServiceConnection _connection;
        public event Action<ValueSet> OnMessageReceived;

        //connection = new AppServiceConnection();
        //connection.AppServiceName = "BigOwl.ControllerHubService";
        //connection.PackageFamilyName = "BigOwlControllerHubService-uwp_rtvxam156657y";
        //AppServiceConnectionStatus status = await connection.OpenAsync();

        // Todo: convert to dependency injection
        public static RelayClient Instance { get; } = new RelayClient();
        public bool IsConnected => _connection != null;

        private async Task<AppServiceConnection> CachedConnection()
        {
            if (_connection != null) return _connection;
            _connection = await MakeConnection();
            _connection.RequestReceived += ConnectionOnRequestReceived;
            _connection.ServiceClosed += ConnectionOnServiceClosed;
            return _connection;
        }

        public async Task Open()
        {
            await CachedConnection();
        }

        private async Task<AppServiceConnection> MakeConnection()
        {
            var listing = await AppServiceCatalog.FindAppServiceProvidersAsync(AppServiceName);

            if (listing.Count == 0)
            {
                throw new Exception("Unable to find app service '" + AppServiceName + "'");
            }
            var packageName = listing[0].PackageFamilyName;

            var connection = new AppServiceConnection
            {
                AppServiceName = AppServiceName,
                PackageFamilyName = packageName
            };

            var status = await connection.OpenAsync();

            if (status != AppServiceConnectionStatus.Success)
            {
                throw new Exception("Could not connect to MessageRelay, status: " + status);
            }

            return connection;
        }

        private void ConnectionOnServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            DisposeConnection();
        }

        private void DisposeConnection()
        {
            if (_connection == null) return;

            _connection.RequestReceived -= ConnectionOnRequestReceived;
            _connection.ServiceClosed -= ConnectionOnServiceClosed;
            _connection.Dispose();
            _connection = null;
        }

        private void ConnectionOnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var appServiceDeferral = args.GetDeferral();
            try
            {
                ValueSet valueSet = args.Request.Message;
                OnMessageReceived?.Invoke(valueSet);
            }
            finally
            {
                appServiceDeferral.Complete();
            }
        }

        public void CloseConnection()
        {
            DisposeConnection();
        }

        private async Task SendMessageAsync(KeyValuePair<string, object> keyValuePair)
        {
            var connection = await CachedConnection();
            var result = await connection.SendMessageAsync(new ValueSet { keyValuePair });
            if (result.Status == AppServiceResponseStatus.Success)
            {
                return;
            }
            throw new Exception("Error sending " + result.Status);
        }

        public async Task SendOwlCommand(OwlCommand command)
        {
            var message = new ValueSet();
            // Serialized classes are OK
            string value = Serialize<OwlCommand>(command);

            await SendMessageAsync(new KeyValuePair<string, object>("command", value));
        }

        public async Task SendMessageAsync(string key, string value)
        {
            await SendMessageAsync(new KeyValuePair<string, object>(key, value));
        }

        public static string Serialize<T>(T value)
        {
            var dcs = new DataContractSerializer(typeof(T));
            var stream = new MemoryStream();
            //if we wanted to pass binary instead...
            //XmlDictionaryWriter binaryDictionaryWriter = XmlDictionaryWriter.CreateBinaryWriter(stream);
            //dcs.WriteObject(binaryDictionaryWriter, record1);
            //binaryDictionaryWriter.Flush();

            dcs.WriteObject(stream, value);
            stream.Position = 0;
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
            
            return Encoding.UTF8.GetString(buffer);
        }

        public static OwlCommand DeSerializeOwlCommand(string payload)
        {
            using (Stream stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(payload);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(typeof(OwlCommand));
                return deserializer.ReadObject(stream) as OwlCommand;
            }
        }
    }

}
