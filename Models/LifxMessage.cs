using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Foxpaws.Wizlif.Models
{
    public enum LifxMessageType : ushort
    {
        None = 0,
        GetService = 2,
        StateService = 3,
        GetHostInfo = 12,
        StateHostInfo = 13,
        GetHostFirmware = 14,
        StateHoseFirmware = 15,
        GetWifiInfo = 16,
        StateWifiInfo = 17,
        GetWifiFirmware = 18,
        StateWifiFirmware = 19,
        GetPower = 20,
        SetPower = 21,
        StatePower = 22,
        GetLabel = 23,
        SetLabel = 24,
        StateLabel = 25,
        GetVersion = 32,
        StateVersion = 33,
        GetInfo = 34,
        StateInfo = 35,
        Acknowledgement = 45,
        GetLocation = 48,
        StateLocation = 50,
        GetGroup = 51,
        StateGroup = 53,
        EchoRequest = 58,
        RechoResponse = 59,
        LightGet = 101,
        LightSetColor = 102,
        LightSetWaveform = 103,
        LightState = 107,
        LightGetPower = 116,
        LightSetPower = 117,
        LightStatePower = 118,
        LightGetInfrared = 120,
        LightStateInfrared = 121,
        LightSetInfrared = 122,
        MultiZoneSetColorZones = 501,
        MultiZoneGetColorZones = 502,
        MultiZoneStateZone = 503,
        MultiZoneStateMultiZone = 506,
        GetMultiZoneEffect = 507,
        SetMultiZoneEffect = 508,
        StateMultiZoneEffect = 509,
        GetDeviceChain = 701,
        StateDeviceChain = 702,
        SetUserPosition = 703,
        GetTileState64 = 707,
        StateTileState64 = 711,
        SetTileState64 = 715,
        GetTileEffect = 718,
        SetTileEffect = 719,
        StateTileEffect = 720
    };

    public class LifxMessage
    {
        public ushort Size { get; set; }
        public ushort Protocol { get; set; }
        public bool Addressable { get; set; }
        public bool Tagged { get; set; }
        public byte Origin { get; set; }
        public uint Source { get; set; }
        public ulong Target { get; set; }
        public ulong Reserved1 { get; set; }
        public bool ResponseRequired { get; set; }
        public bool AckRequired { get; set; }
        public byte Reserved2 { get; set; }
        public byte Sequence { get; set; }
        public ulong Reserved3 { get; set; }
        public ushort Type { get; set; }
        public ushort Reserved4 { get; set; }

        public LifxMessageType MessageType { get; set; } = LifxMessageType.None;

        public virtual void Deserialize(BinaryReader reader)
        {
            Size = reader.ReadUInt16();

            // Proto is only 12 bits
            Protocol = reader.ReadByte();
            var pato = reader.ReadByte();
            Protocol |= (ushort)((pato & 0b1111) << 8);

            // Remaining bits of pato
            Addressable = (byte)(pato & 0b10000) != 0;
            Tagged = (byte)(pato & 0b100000) != 0;
            Origin = (byte)((byte)(pato & 0b11000000) >> 6);

            Source = reader.ReadUInt32();

            // Target is 8 bytes, not an encoded uint64
            Target = (ulong)(reader.ReadByte()) << 56;
            Target |= (ulong)(reader.ReadByte()) << 48;
            Target |= (ulong)(reader.ReadByte()) << 40;
            Target |= (ulong)(reader.ReadByte()) << 32;
            Target |= (ulong)(reader.ReadByte()) << 24;
            Target |= (ulong)(reader.ReadByte()) << 16;
            Target |= (ulong)(reader.ReadByte()) << 8;
            Target |= (ulong)(reader.ReadByte());

            // 6 'bytes', not 6 encoded bytes
            Reserved1 = (ulong)(reader.ReadByte()) << 40;
            Reserved1 |= (ulong)(reader.ReadByte()) << 32;
            Reserved1 |= (ulong)(reader.ReadByte()) << 24;
            Reserved1 |= (ulong)(reader.ReadByte()) << 16;
            Reserved1 |= (ulong)(reader.ReadByte()) << 8;
            Reserved1 |= (ulong)(reader.ReadByte());

            byte rar = reader.ReadByte();
            ResponseRequired = (rar & 0b1) != 0;
            AckRequired = (rar & 0b10) != 0;
            Reserved2 = (byte)((rar & 0b11111100) >> 2);

            Sequence = reader.ReadByte();
            Reserved3 = reader.ReadUInt64();
            Type = reader.ReadUInt16();
            Reserved4 = reader.ReadUInt16();
        }

        public static LifxMessage Read(byte[] bytes)
        {
            // All LIFX will be at least 36 bytes
            if (bytes.Length < 36)
                throw new InvalidDataException("Supplied byte stream is not long enough for a LIFX packet");

            using var ms = new MemoryStream(bytes);
            using var reader = new BinaryReader(ms);

            var size = reader.ReadUInt16();
            if (bytes.Length != size)
                throw new InvalidDataException($"Invalid LIFX packet supplied ({bytes.Length} supplied, {size} expected)");

            // Get the type bits, since we need to figure out what kind of packet we are
            ms.Seek(32, SeekOrigin.Begin);
            ushort type = reader.ReadUInt16();

            var packet = (LifxMessageType)type switch
            {
                LifxMessageType.LightSetColor => new LifxLightSetColorMessage(),
                _ => new LifxMessage()
            };

            ms.Seek(0, SeekOrigin.Begin);
            packet.Deserialize(reader);
            return packet;
        }
    }
}
