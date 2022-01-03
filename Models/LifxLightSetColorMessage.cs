using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foxpaws.Wizlif.Models
{
    class LifxLightSetColorMessage : LifxMessage
    {
        public byte Reserved6 { get; set; }
        public ushort Hue { get; set; }
        public ushort Saturation { get; set; }
        public ushort Brightness { get; set; }
        public ushort Kelvin { get; set; }
        public uint Duration { get; set; }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Reserved6 = reader.ReadByte();
            Hue = reader.ReadUInt16();
            Saturation = reader.ReadUInt16();
            Brightness = reader.ReadUInt16();
            Kelvin = reader.ReadUInt16();
            Duration = reader.ReadUInt32();
        }
    }
}
