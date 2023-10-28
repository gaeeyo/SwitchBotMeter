using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using SwitchBotUtil;

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

        private static void PrintTemp(ulong address, double temp, int humidity, int battery, String device)
        {
            Console.WriteLine($"{address:X} {temp:f1} {humidity:d} {battery:d} {device}");
        }

        private static void GetTemp(int limit, int timeout)
        {
            var watcher = new BluetoothLEAdvertisementWatcher();
            var countdownEvent = new CountdownEvent(limit > 0 ? limit : 1);
            var meters = new Dictionary<ulong, byte[]>();

            watcher.Received += (sender, args) =>
            {
                if (args.Advertisement.ServiceUuids.Contains(Uuids.meter))
                {
                    // 初代の温度湿度計
                    foreach (var ds in args.Advertisement.DataSections)
                    {
                        if (ds.DataType == 22 && ds.Data.Length == 8)
                        {
                            var data = ds.Data.ToArray(2, 6);
                            double temp = ((data[3] & 0x0f) / 10.0 + (data[4] & 0x7f)) * ((data[4] & 0x80) != 0 ? 1 : -1);
                            int humidity = data[5] & 0x7f;
                            int battery = data[2] & 0x7f;

                            PrintTemp(args.BluetoothAddress, temp, humidity, battery, "Meter");
                            if (limit > 0) countdownEvent.Signal();
                        }
                    }
                }
                else
                {
                    if (meters.TryGetValue(args.BluetoothAddress, out var serviceData))
                    {
                        var mdl = args.Advertisement.GetManufacturerDataByCompanyId(SwitchBot.companyId);
                        if (mdl.Count == 1)
                        {
                            var md = mdl[0].Data.ToArray();
                            double temp = ((md[9] & 0x7f) + ((md[8] & 0x0f) / 10.0)) * ((md[9] & 0x80) == 0 ? -1 : 1);
                            int humidity = (md[10] & 0x7f);
                            int battery = serviceData[4] & 0x7f;
                            int deviceType = serviceData[2] & 0x7f;
                            PrintTemp(args.BluetoothAddress, temp, humidity, battery, GetDeviceTypeName(deviceType));
                            if (limit > 0) countdownEvent.Signal();
                        }
                    }
                    else
                    {
                        var sdl = args.Advertisement.GetSectionsByType(0x16);
                        foreach (var sd in sdl)
                        {
                            if (sd.Data.GetByte(0) == 0x3d && sd.Data.GetByte(1) == 0xfd)
                            {
                                var deviceType = (sd.Data.GetByte(2) & 0x7f);
                                switch (deviceType)
                                {
                                    case SBDeviceTypes.OutdoorMeter:
                                    case SBDeviceTypes.MeterPlus:
                                        meters[args.BluetoothAddress] = sd.Data.ToArray();
                                        break;
                                    default:
                                        break;
                                }
                            }
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

        static String GetDeviceTypeName(int value)
        {
            switch (value)
            {
                case SBDeviceTypes.Bot:
                    return "Bot";
                case SBDeviceTypes.Meter:
                    return "Meter";
                case SBDeviceTypes.MeterPlus:
                    return "MeterPlus";
                default:
                    return "notImplemented";
            }
        }
    }
}
