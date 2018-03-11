using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace WiiMouse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker bg;
        BluetoothDeviceInfo wiidevice;
        private static string Addr = "00:24:44:99:C1:E5";
        private static BluetoothClient localClient = new BluetoothClient();
        private bool autoConnectWiiMote_Flag = false;

        public MainWindow()
        {
            InitializeComponent();

            bg = new BackgroundWorker();
            bg.DoWork += new DoWorkEventHandler(bg_DoWork);
            bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
            BluetoothDeviceInfo[] array = localClient.DiscoverDevices();
            foreach (BluetoothDeviceInfo device in array)
            {
                if (device.DeviceName.Contains("Nintendo") || device.DeviceName.Contains(Addr))
                {
                    wiidevice = device;
                }
            }
        }

        private void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WiiConnectStart();
        }

        private void bg_DoWork(object sender, DoWorkEventArgs e)
        {

           /* BluetoothDeviceInfo[] array = localClient.DiscoverDevices();
            foreach (BluetoothDeviceInfo device in array)
            {
                if (device.DeviceName.Contains("Nintendo") || device.DeviceName.Contains(Addr))
                {
                    wiidevice = device;
                }
            }
            e.Result = wiidevice;
            */
        }

        private void connectWiiMote_Click(object sender, RoutedEventArgs e)
        {
            if (!bg.IsBusy)
            {
                bg.RunWorkerAsync();
            }
        }

        private void autoConnectWiiMote_Click(object sender, RoutedEventArgs e)
        {
            if (!autoConnectWiiMote_Flag)
            {
                autoConnectWiiMote_Flag = true;
                autoConnectWiiMote.Background = Brushes.Red;
                do
                {
                    if (!wiidevice.Connected)
                    {
                        bg.RunWorkerAsync();

                        System.Threading.Thread.Sleep(2000);
                    }
                }
                while (autoConnectWiiMote_Flag);
            }
        
        }
    

        private void WiiConnectStart()
        {
            BluetoothSecurity.RemoveDevice(wiidevice.DeviceAddress);

            wiidevice.SetServiceState(BluetoothService.HumanInterfaceDevice, true);
            wiidevice.Refresh();
            System.Threading.Thread.Sleep(500);
            localClient.BeginConnect(wiidevice.DeviceAddress, BluetoothService.HumanInterfaceDevice, new AsyncCallback(Connect), wiidevice);
        }

        private void Connect(IAsyncResult result)
        {
            if (result.IsCompleted)
            {
                using (WiiMoteController wiiController = new WiiMoteController())
                {
                    wiiController.InitiateConnect();
                }
            }
            else
            {
                MessageBox.Show("not connected");
            }
        }


    }
}
