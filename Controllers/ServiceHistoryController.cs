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
    public class ServiceHistoryController : ControllerBase
    {
        private readonly ILogger<ServiceHistoryController> _logger;
        private readonly CosmosDBService _cosmosdbservice;
        
        public ServiceHistoryController(ILogger<ServiceHistoryController> logger, CosmosDBService cdb) // inject CosmosDBService
        {
            _logger = logger;
            _cosmosdbservice = cdb;
        }

        [HttpGet]
        public IEnumerable<ServiceHistoryRecord> Get(string vid)
        {
            _logger.LogDebug("XXXX LOG DEBUG ServiceHistoryController");
            string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogDebug($"XXXX LOG DEBUG ServiceHistoryController: calling vid: {vid}; oid: {owner};");

            return _cosmosdbservice.GetServiceHistory(vid, owner).GetAwaiter().GetResult() as IEnumerable<ServiceHistoryRecord>;

        }


        [HttpPost]
        public async void Post([FromBody] ServiceHistoryRecord shr)
        {
            string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            shr.oid = owner;
            
            shr.ServiceHistoryID = Guid.NewGuid().ToString();
            

            _logger.LogDebug("XXXX LOG DEBUG POST ServiceHistoryController ServiceHistoryRecord ID:" + shr.ServiceHistoryID + " Datum:" + shr.Date + " desc:" + shr.Description + " oid:" + shr.oid);

            await _cosmosdbservice.AddServiceHistory(shr);

        }


        [HttpDelete("{vid}")]
        public async void Delete(string vid)
        {
            _logger.LogDebug("XXXX LOG DEBUG DELETE ServiceHistoryController vid: " + vid);

            string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ServiceHistoryRecord shr = new ServiceHistoryRecord() { ServiceHistoryID = vid, oid = owner };

            //sr.oid = owner;

            _logger.LogDebug("XXXX LOG DEBUG DELETE ServiceHistoryController ServiceHistoryRecord ID:" + shr.ServiceHistoryID +
                "descr:" + shr.Description + " oid: " + shr.oid);

            await _cosmosdbservice.DeleteServiceHistoryEntry(shr); // delete ServiceRecord and ServicePictureRecord

        }



    }
}
