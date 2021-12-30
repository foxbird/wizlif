using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Foxpaws.Wizlif.Models
{
    public class WizResult
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
