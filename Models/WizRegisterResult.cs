using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Foxpaws.Wizlif.Models
{
    public class WizRegisterResult : WizResult
    {
        [JsonPropertyName("mac")]
        public string MacAddress { get; set; }
    }
}
