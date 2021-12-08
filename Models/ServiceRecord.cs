using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WL_WebAPI.Models
{
    public class ServiceRecord
    {
        [JsonProperty(PropertyName = "id")]
        public string ServiceID { get; set; } // service entry ID
        
        [JsonProperty(PropertyName = "oid")]
        public string oid { get; set; }

        public string RefServiceHistoryID { get; set; } // references ServiceHistoryID this service belongs to

        public string Description { get; set; } // description for service entry
        
        public int HowManyPics { get; set; }
        
        public string DescriptionForPics { get; set; } // description for pictures
        
        public byte[] ImageThumbData { get; set; } // small thumbnail (big pic in ServicePictureRecord.cs)
    }
}
