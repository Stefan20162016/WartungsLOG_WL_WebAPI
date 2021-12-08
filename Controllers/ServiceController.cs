using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WL_WebAPI.Services;
using WL_WebAPI.Models;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace WL_WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [RequiredScope("zugriffFuerBenutzer")]

    public class ServiceController : ControllerBase
    {
        private readonly ILogger<ServiceController> _logger;
        private readonly CosmosDBService _cosmosdbservice;

        public ServiceController(ILogger<ServiceController> logger, CosmosDBService cdb)
        {
            _logger = logger;
            _cosmosdbservice = cdb;
        }

        [HttpGet]
        public IEnumerable<ServiceRecord> Get(string sid)
        {

            string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogDebug($"XXXX LOG DEBUG ServiceController: calling shid: {sid}; oid: {owner};");

            return _cosmosdbservice.GetService(sid, owner).GetAwaiter().GetResult() as IEnumerable<ServiceRecord>;

        }


        [HttpPost]
        public async void Post([FromBody] ServiceRecord sr)
        {
            string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            sr.oid = owner;

            sr.ServiceID = Guid.NewGuid().ToString();

            _logger.LogDebug("XXXX LOG DEBUG POST ServiceController ServiceRecord ID:" + sr.ServiceID +
                " Datum:" + sr.Description + " oid: " + sr.oid);

            await _cosmosdbservice.AddServiceEntry(sr);

        }

        [HttpDelete("{sid}")]
        public async void Delete( string sid)
        {
            _logger.LogDebug("XXXX LOG DEBUG DELETE ServiceController sid: " + sid);

            string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ServiceRecord sr = new ServiceRecord() { ServiceID = sid, oid = owner };


            _logger.LogDebug("XXXX LOG DEBUG DELETE ServiceController ServiceRecord ID:" + sr.ServiceID +
                "descr:" + sr.Description + " oid: " + sr.oid);

            await _cosmosdbservice.DeleteServiceEntry(sr); // delete ServiceRecord and ServicePictureRecord

        }



    }
}
