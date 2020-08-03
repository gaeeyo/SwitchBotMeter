using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;

namespace SwitbotThermometerApp
{
    class Program
    {

        enum Commands
        {
            GetTemp,
            List,
            Help
        }

        static void Main(string[] args)
        {
            int limit = 1;
            int timeout = 120;
            Commands command = Commands.GetTemp;

            for (var j = 0; j < args.Length; j++)
            {
                switch (args[j])
                {
                    case "--limit":
                        if (j + 1 >= args.Length) throw new Exception("--limit のパラメータが足りません");
                        j++;
                        limit = int.Parse(args[j]);
                        break;
                    case "--timeout":
                        if (j + 1 >= args.Length) throw new Exception("--timeout のパラメータがありません");
                        j++;
                        timeout = int.Parse(args[j]);
                        break;
                    case "--list":
                    case "-l":
                        command = Commands.List;
                        break;
                    case "--help":
                    case "-h":
                        command = Commands.Help;
                        break;
                }
            }

            switch (command)
            {
                case Commands.GetTemp:
                    GetTemp(limit, timeout);
                    break;
                case Commands.List:
                    ListDevices();
                    break;
                case Commands.Help:
                    Help();
                    break;
            }
        }

        private static void Help()
        {
            Console.WriteLine("Usage: SwitchBotMeter [options...]\n"
                + " Options:\n"
                + "     --limit N    Limit results (default: 1, unlimited: 0)\n"
                + "  -t --timeout N  Timeout seconds(default: 120, unlimited: 0)\n"
                + "  -l --list       ListDevices\n"
                + "  -h --help       Show help\n"
                );
        }

        private static void ListDevices()
        {
            // Query for extra properties you want returned
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            DeviceWatcher deviceWatcher =
                        DeviceInformation.CreateWatcher(
                                BluetoothLEDevice.GetDeviceSelector(),
                                //BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
                                requestedProperties,
                                DeviceInformationKind.AssociationEndpoint);

            deviceWatcher.Added += (sender, args) =>
            {
                Console.WriteLine(args.Name + " " + args.Id);
                var e = args.Properties.GetEnumerator();
                while (e.MoveNext())
                {
                    Console.WriteLine("[" + e.Current.Key + "]" + e.Current.Value);
                }
            };
            deviceWatcher.Start();
            Thread.Sleep(5 * 1000);
            deviceWatcher.Stop();
        }

        private static void GetTemp(int limit, int timeout)
        {
            var switchBotMeterThUuid = new Guid("cba20d00-224d-11e6-9fb8-0002a5d5c51b");

            var filter = new BluetoothLEAdvertisementFilter();
            filter.Advertisement = new BluetoothLEAdvertisement();
            filter.Advertisement.ServiceUuids.Add(switchBotMeterThUuid);

            var watcher = new BluetoothLEAdvertisementWatcher(filter);
            var countdownEvent = new CountdownEvent(limit > 0 ? limit : 1);

            watcher.Received += (sender, args) =>
            {
                if (args.Advertisement.ServiceUuids.Contains(switchBotMeterThUuid))
                {

                    foreach (var ds in args.Advertisement.DataSections)
                    {
                        if (ds.DataType == 22 && ds.Data.Length == 8)
                        {
                            var data = ds.Data.ToArray(2, 6);
                            double temp = ((data[3] & 0x0f) / 10.0 + (data[4] & 0x7f)) * ((data[4] & 0x80) != 0 ? 1 : -1);
                            int humidity = data[5] & 0x7f;
                            int battery = data[2] & 0x7f;
                            Console.WriteLine($"{args.BluetoothAddress:X} {temp:f1} {humidity:d} {battery:d}");
                            if (limit > 0) countdownEvent.Signal();
                        }
                    }
                }
            };
            watcher.ScanningMode = BluetoothLEScanningMode.Active;
            watcher.Start();
            if (timeout > 0)
            {
                countdownEvent.Wait(timeout * 1000);
            }
            else
            {
                countdownEvent.Wait();
            }
            watcher.Stop();
        }
    }
}
