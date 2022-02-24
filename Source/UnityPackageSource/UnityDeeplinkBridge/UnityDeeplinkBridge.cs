using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace UnityDeeplinkBridging
{
    [InitializeOnLoad]
    public class UnityDeeplinkBridge
    {
        public const string PROTOCOL_BASE = "unitydeeplink://";
        public delegate void HandlerDelegate(string data);

        public static ILogger _logger = UnityEngine.Debug.unityLogger;
        private static Dictionary<string, HandlerDelegate> _handlers = new Dictionary<string, HandlerDelegate>();

        private Thread _Thread;
        IFocusController _focusController;
        private List<string> _received = new List<string>();
        private bool _kill;
        private bool _initialized;

        internal UnityDeeplinkBridge(IFocusController focusController)
        {
            _focusController = focusController;
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            _kill = false;
            
            _focusController.Initialize();

            _Thread = new Thread(ListenerThreadWork);
            _Thread.Start();
        }

        private void OnBeforeAssemblyReload()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            EditorApplication.update -= Update;
            _kill = true;
            _Thread.Abort();
        }

        private void Update()
        {
            if (_received.Count == 0)
            {
                return;
            }

            ProcessReceived(_received[0]);
            _received.RemoveAt(0);
        }

        private void ListenerThreadWork()
        {
            byte[] textBuffer = new Byte[1024];
            byte[] intBuffer = new Byte[4];
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9910);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(IPAddress.Parse("127.0.0.1").AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and
            // listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);
                // Start listening for connections.  
                while (!_kill)
                {
                    //Thread will hang while waiting for a connection  
                    Socket handler = listener.Accept();
                    handler.ReceiveTimeout = 100;
                    ReadFromConnection(handler, intBuffer, textBuffer);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (ThreadAbortException)
            {
                //This is fine
            }
            catch (Exception e)
            {
                _logger.LogError("UnityDeeplinkBridge", "An error occured in UnityDeeplinkBridge. To prevent spam, the service has been killed. Reboot Unity to restart the service.\n Error: " + e);
            }
        }

        private void ReadFromConnection(Socket handler, byte[] intBuffer, byte[] textBuffer)
        {
            string data = "";
            int intBytesRec = handler.Receive(intBuffer);
            if (intBytesRec != 4)
            {
                _logger.LogError("UnityDeeplinkBridge", "Invalid protocol, less than 4 bytes of data receieved from socket.");
                return;
            }

            int bytesToRead = BitConverter.ToInt32(intBuffer, 0);
            // An incoming connection needs to be processed.  
            while (bytesToRead > 0 && !_kill)
            {
                int bytesRec = handler.Receive(textBuffer);

                bytesToRead -= bytesRec;
                data += Encoding.ASCII.GetString(textBuffer, 0, bytesRec);

                if (handler.Available == 0)
                {
                    break;
                }
            }

            if (bytesToRead > 0)
            {
                _logger.LogWarning("UnityDeeplinkBridge", "Something went wrong while receiving data, not all bytes were read. This could be due to an incorrect header, or the socket terminated early.");
            }

            _received.Add(data);
            _focusController.FocusThis();
        }

        private void ProcessReceived(string received)
        {
            string protocol = received.Split('/')[0];
            if (!_handlers.TryGetValue(protocol, out HandlerDelegate handler))
            {
                _logger.LogError("UnityDeeplinkBridge", $"No handler found for protocol: '{protocol}'");
                return;
            }

            handler.Invoke(received.Substring(protocol.Length + 1, received.Length - protocol.Length - 1));
        }

        public static void AddHandler(string protocol, HandlerDelegate handler)
        {
            if (_handlers.TryGetValue(protocol, out HandlerDelegate _))
            {
                _logger.LogError("UnityDeeplinkBridge", $"Handler alread exists for protocol: '{protocol}'");
                return;
            }

            _handlers[protocol] = handler;
        }
    }
}
