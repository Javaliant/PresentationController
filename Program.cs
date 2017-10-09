using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using WindowsInput;
using WindowsInput.Native;

namespace PresentationController
{
    class ControllerServer
    {
        private static KeyboardSimulator keyboardSimulator = new KeyboardSimulator(new InputSimulator());

        public static void Main()
        {
            var ipList = NetworkInterface.GetAllNetworkInterfaces()
                .Where(i => i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                .SelectMany(i => i.GetIPProperties().UnicastAddresses)
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.Address.ToString())
                .ToList();

            Console.WriteLine($"IPs to connect to: {string.Join(", ", ipList)}" );
            StartListening();
        }

        private static void StartListening(int port = 9001)
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            TcpListener server = new TcpListener(ip, port);

            try
            {
                server.Start();
                Console.WriteLine("Server online.");
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Client connected.");
                    bool active = true;
                    while (active)
                    {
                        byte[] inputBuffer = new byte[1024];
                        client.GetStream().Read(inputBuffer, 0, inputBuffer.Length);
                        string message = RetrieveMessage(inputBuffer);
                        switch (message)
                        {
                            case "die":
                                client.Close();
                                active = false;
                                Console.WriteLine("Client disconnected.\n");
                                break;
                            case "left":
                                keyboardSimulator.KeyPress(VirtualKeyCode.LEFT);
                                break;
                            case "right":
                                keyboardSimulator.KeyPress(VirtualKeyCode.RIGHT);
                                break;
                            default:
                                Console.WriteLine($"Client sent: {message}");
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.Read();
            }
        }

        private static string RetrieveMessage(byte[] buffer)
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