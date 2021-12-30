using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Foxpaws.Wizlif.Models
{
    public class WizRegisterResponseMessage : WizResponseMessage
    {
        [JsonPropertyName("result")]
        public WizRegisterResult Result { get; set; }
    }
}
