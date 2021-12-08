using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WL_WebAPI.Models;
using WL_WebAPI.Services;

namespace WL_WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [RequiredScope("zugriffFuerBenutzer")]

    public class ServicePictureController : ControllerBase
    {
        private readonly ILogger<ServicePictureController> _logger;
        private readonly CosmosDBService _cosmosdbservice;

        public ServicePictureController(ILogger<ServicePictureController> logger, CosmosDBService cdb)
        {
            _logger = logger;
            _cosmosdbservice = cdb;
        }

        [HttpGet]
        public ServicePictureRecord Get(string picID)
        {

            string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogDebug($"XXXX LOG DEBUG servicepicturecontroller: calling picID: {picID}; oid: {owner};");

            return _cosmosdbservice.GetServicePicture(picID, owner).GetAwaiter().GetResult();

        }




    }
}
