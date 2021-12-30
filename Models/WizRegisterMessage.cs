using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Foxpaws.Wizlif.Models
{
    public class WizRegisterMessage
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = "registration";
        [JsonPropertyName("params")]
        public RegisterParams Params { get; set; } = new RegisterParams();

        public class RegisterParams
        {
            [JsonPropertyName("phoneMac")]
            public string MacAddress { get; set; } = "AAAAAAAAAAAA";
            [JsonPropertyName("register")]
            public bool Register { get; set; } = false;
            [JsonPropertyName("phoneIp")]
            public string IpAddress { get; set; } = "1.2.3.4";
            [JsonPropertyName("id")]
            public string Id { get; set; } = "1";
        }

    }


}
