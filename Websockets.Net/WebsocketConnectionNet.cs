using System;
using System.Threading.Tasks;
using Websockets.Net;

namespace Websockets.Droid
{
    /// <summary>
    /// A Websocket connection for 'full' .Net Core applications
    /// </summary>
    public class WebsocketConnectionNet : IWebSocketConnection
    {
        public bool IsOpen { get; private set; }

        public event Action OnClosed = delegate { };
        public event Action OnOpened = delegate { };
        public event Action<string> OnError = delegate { };
        public event Action<string> OnMessage = delegate { };
        public event Action<string> OnLog = delegate { };

        static WebsocketConnectionNet()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
        }

        /// <summary>
        /// Factory Initializer
        /// </summary>
        public static void Link()
        {
            WebSocketFactory.Init(() => new WebsocketConnectionNet());
        }

        private WebSocketWrapper _websocket = null;

        public async void Open(string url, string protocol = null)
        {
            try
            {
                if (_websocket != null)
                    EndConnection();

                _websocket = new WebSocketWrapper();
                _websocket.Closed += _websocket_Closed;
                _websocket.Opened += _websocket_Opened;
                _websocket.Error += _websocket_Error;
                _websocket.MessageReceived += _websocket_MessageReceived;

                await _websocket.Connect(url);

            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
        }

        public void Close()
        {
            EndConnection();
        }

        public async void Send(string message)
        {
            await _websocket.SendMessage(message);
        }


        public void Dispose()
        {
            Close();
        }

        //
        void EndConnection()
        {
            if (_websocket != null)
            {
                _websocket.Closed -= _websocket_Closed;
                _websocket.Opened -= _websocket_Opened;
                _websocket.Error -= _websocket_Error;
                _websocket.MessageReceived -= _websocket_MessageReceived;
                _websocket.Dispose();
                _websocket = null;

                IsOpen = false;
                OnClosed();
            }
        }


        void _websocket_Error(Exception obj)
        {
            OnError(obj.Message);
        }

        void _websocket_Opened(WebSocketWrapper arg)
        {

            IsOpen = true;
            OnOpened();
        }

        void _websocket_MessageReceived(string m, WebSocketWrapper arg)
        {

            OnMessage(m);
        }

        void _websocket_Closed(WebSocketWrapper arg)
        {
            EndConnection();
        }
    }
}