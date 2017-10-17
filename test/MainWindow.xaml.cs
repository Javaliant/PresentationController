using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using WindowsInput;
using WindowsInput.Native;

namespace RemoteControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        #region variables
        private bool online;
        private KeyboardSimulator keyboardSimulator = new KeyboardSimulator(new InputSimulator());
        private TcpListener server;
        private string offlineImage = @"C:\Work\Personal\Projects\Test\RemoteControl\RemoteControl\Images\offline.png";
        private string onlineImage = @"C:\Work\Personal\Projects\Test\RemoteControl\RemoteControl\Images\online.png";
        private string activeImage = @"C:\Work\Personal\Projects\Test\RemoteControl\RemoteControl\Images\active.png";
        #endregion

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            ImagePath = offlineImage;
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            stateLabel.Content = online ? "Offline" : $"Online - Host: {RetrieveHostIP()}";
            button.Content = online ? "Start" : "Stop";
            ImagePath = online ? offlineImage : onlineImage;

            if (!online)
            {
                StartListening();
            }
            else
            {
                server.Stop();
            }

            online = !online;
        }

        #region properties
        private string _imagePath;
        public string ImagePath
        {
            get
            {
                return _imagePath;
            }
            private set
            {
                if (_imagePath != value)
                {
                    _imagePath = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        private string RetrieveHostIP()
        {
            return string.Join(", ", NetworkInterface.GetAllNetworkInterfaces()
                .Where(i => i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                .SelectMany(i => i.GetIPProperties().UnicastAddresses)
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.Address.ToString())
                .ToList());
        }

        private void StartListening(int port = 9001)
        {
            server = new TcpListener(IPAddress.Any, port);
            try
            {
                server.Start();
                while (online)
                {
                    TcpClient client = server.AcceptTcpClient();
                    bool active = true;
                    ImagePath = activeImage;
                    messageBox.AppendText("Client connected\n");
                    while (active)
                    {
                        byte[] inputBuffer = new byte[1024];
                        client.GetStream().Read(inputBuffer, 0, inputBuffer.Length);
                        string message = RetrieveMessage(inputBuffer);
                        switch (message)
                        {
                            case "die":
                                client.Close();
                                online = active = false;
                                ImagePath = offlineImage;
                                break;
                            case "":
                            case "stop":
                                client.Close();
                                active = false;
                                ImagePath = onlineImage;
                                messageBox.AppendText("Client disconnected\n");
                                break;
                            case "left":
                                keyboardSimulator.KeyPress(VirtualKeyCode.LEFT);
                                break;
                            case "right":
                                keyboardSimulator.KeyPress(VirtualKeyCode.RIGHT);
                                break;
                            default:
                                messageBox.AppendText($"client sent: {message}\n");
                                break;
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                messageBox.AppendText($"{ex.ToString()}\n");
                ImagePath = offlineImage;
            }
        }

        private string RetrieveMessage(byte[] buffer)
        {
            StringBuilder message = new StringBuilder();
            foreach (byte b in buffer)
            {
                if (b.Equals(0))
                {
                    break;
                }
                message.Append(Convert.ToChar(b));
            }
            return message.ToString();
        }
    }
}
