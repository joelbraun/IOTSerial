﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using IotWeb.Common.Util;
using IotWeb.Common.Http;
using IotWeb.Server;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using Windows.Devices.Enumeration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WebHost_UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Simple 'echo' web socket server
        /// </summary>
        class WebSocketHandler : IWebSocketRequestHandler
        {

            public bool WillAcceptRequest(string uri, string protocol)
            {
                return (uri.Length == 0) && (protocol == "echo");
            }

            public void Connected(WebSocket socket)
            {
                socket.DataReceived += OnDataReceived;
            }

            void OnDataReceived(WebSocket socket, string frame)
            {
                socket.Send(frame);
            }
        }

        // The web server instance
        private BaseHttpServer m_server;

        private SerialDevice serialPort = null;
        DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;
        CancellationTokenSource ReadCancellationTokenSource;

        public async void SerialStuff()
        {
            var dis = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector());
            Debug.WriteLine(dis.First().Name);

            Debug.WriteLine(dis.First().Id.ToString());
            serialPort = await SerialDevice.FromIdAsync(dis.First().Id);
            if (serialPort != null)
            {
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.BaudRate = 9600;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;

                ReadCancellationTokenSource = new CancellationTokenSource();

                Listen();
            }
            else
            {
                Debug.WriteLine("Serial system not connected.");
            }
        }

        public async void Listen()
        {
            if (serialPort != null)
            {
                dataReaderObject = new DataReader(serialPort.InputStream);
                
                    while (true)
                {
                    await ReadAsync(ReadCancellationTokenSource.Token);
                }
            }
        }

        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Debug.WriteLine("Entering ReadAsync");
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            // Create a task object to wait for data on the serialPort.InputStream
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            // Launch the task and wait
            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
                Debug.WriteLine(dataReaderObject.ReadString(bytesRead));
                Debug.WriteLine("\n");
            }
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;


            // Run the web server
            m_server = new HttpServer(8000);
            m_server.AddHttpRequestHandler(
                "/",
                new HttpResourceHandler(
                    Utilities.GetContainingAssembly(typeof(Program)),
                    "Resources.Site",
                    "index.html"
                    )
                );
            m_server.AddWebSocketRequestHandler(
                "/sockets/",
                new WebSocketHandler()
                );
            m_server.Start();
            SerialStuff();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
