using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WL_WebAPI.Models
{
    public class ServiceHistoryRecord
    {
        [JsonProperty(PropertyName = "id")]
        public string ServiceHistoryID { get; set; }

        [JsonProperty(PropertyName = "oid")]
        public string oid { get; set; }
        
        public string RefVehicleID { get; set; }

        public string Date { get; set; }

        public string Description { get; set; }

        public int Kilometerstand { get; set; } // service done at Kilomterstand
                                                //public int VehicleID { get; set; }

        public byte[] ImageThumbData { get; set; } // thumbnail for one Service History Entry
    }
}
