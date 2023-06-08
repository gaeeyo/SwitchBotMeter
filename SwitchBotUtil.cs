using System;

namespace SwitchBotUtil
{
    class SwitchBot
    {
        public const ushort companyId = 0x0969;
    }

    class SBDeviceTypes
    {
        public const byte Bot = 0x48;
        public const byte Meter = 0x54;
        public const byte Humidifier = 0x65;
        public const byte Curtain = 0x63;
        public const byte MotionSensor = 0x73;
        public const byte ContactSensor = 0x64;
        public const byte ColorBulb = 0x75;
        public const byte LEDStripLight = 0x72;
        public const byte SmartLock = 0x6F;
        public const byte PlugMini = 0x67;
        public const byte MeterPlus = 0x69;
        public const byte OutdoorMeter = 0x77;
    }

    class Uuids
    {
        public static readonly Guid meter = new Guid("cba20d00-224d-11e6-9fb8-0002a5d5c51b");
    }
}
