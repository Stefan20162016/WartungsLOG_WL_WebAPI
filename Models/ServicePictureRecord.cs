using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WL_WebAPI.Models
{
    public class ServicePictureRecord
    {
      
        [JsonProperty(PropertyName = "id")]
        public string ServicePictureID { get; set; }
        
        [JsonProperty(PropertyName = "oid")]
        public string oid { get; set; }

        public string RefServiceID { get; set; } // set to ServiceRecord ID to indicate backward reference

        public string Description { get; set; }

        public byte[] ImageData { get; set; } // big picture


    }
}
