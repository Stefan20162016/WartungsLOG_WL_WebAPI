using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WL_WebAPI.Models
{
    using Newtonsoft.Json;

    public class Vehicle
    {
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "oid")]
        public string oid { get; set; }

        //[JsonProperty(PropertyName = "id")]
        //public string Idforcosmos { get; set; }

        public string Kennzeichen { get; set; }

        public string Description { get; set; }

        public byte[] ImageThumbData { get; set; }

    }
}
