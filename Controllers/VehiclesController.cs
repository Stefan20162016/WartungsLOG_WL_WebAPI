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
using System.Linq;
using System.IO;
using WL_WebAPI.Services;
using Microsoft.Azure.Cosmos;
using System.Diagnostics;

namespace WL_WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [RequiredScope("zugriffFuerBenutzer")]
    public class VehiclesController : ControllerBase
    {
        static readonly List<Vehicle> _vehicles;
        
        private readonly ILogger<VehiclesController> _logger;
        private readonly CosmosDBService _cosmosdbservice;
        private static byte[] global_image_template;

        public VehiclesController(ILogger<VehiclesController> logger, CosmosDBService cdb) // inject CosmostDBService
        {
            _logger = logger;
            _cosmosdbservice = cdb;
        }


        [HttpGet]
        public IEnumerable<Vehicle> Get()
        {
            _logger.LogTrace("XXXX LOG TRACE VehiclesController GET vehicles");
            _logger.LogDebug("XXXX LOG DEBUG VehiclesController GET vehicles");
            _logger.LogInformation("XXXX LOG INFO VehiclesController GET vehicles");
            _logger.LogWarning("XXXX LOG WARNING VehiclesController GET vehicles");
            _logger.LogError("XXXX LOG ERROR VehiclesController GET vehicles");
            _logger.LogCritical("XXXX LOG CRITICAL VehiclesController GET vehicles");
            

            string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            /*
            foreach (Claim x in User.Claims)
            {
                _logger.LogWarning("XXXX: claim: " + x + "; type: " + x.Type);
            }
            _logger.LogWarning("XXXX: user primary: " + User.Identity.Name);
            _logger.LogWarning("XXXX: user.tostring: " + User);
            _logger.LogWarning("XXXX: user.identity: " + User.Identity);
            _logger.LogWarning("XXXX: owner: " + owner);
            _logger.LogWarning("XXXX: claimtype.name1: " + User.FindFirst(ClaimTypes.AuthenticationMethod)?.Value);
            _logger.LogWarning("XXXX: claimtype.name2: " + User.FindFirst(ClaimTypes.Spn)?.Value);
            _logger.LogWarning("XXXX: claimtype.name3: " + User.FindFirst(ClaimTypes.UserData)?.Value);
            _logger.LogWarning("XXXX: name: " + User.FindFirst(ClaimTypes.Name)?.Value);
            _logger.LogWarning("XXXX: nameidentifier: " + User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            _logger.LogWarning("XXXX: email: " + User.FindFirst(ClaimTypes.Email)?.Value);
            _logger.LogWarning("XXXX: givenname: " + User.FindFirst(ClaimTypes.GivenName)?.Value);
            _logger.LogWarning("XXXX: surname: " + User.FindFirst(ClaimTypes.Surname)?.Value);
            _logger.LogWarning("XXXX: surname: " + User.FindFirst(ClaimTypes.Surname)?.Value);
            _logger.LogWarning("XXXX: name: " + User.FindFirst("name")?.Value);
            _logger.LogWarning("XXXX: emails: " + User.FindFirst("emails")?.Value);
    OUTPUT:
claim: iss: https://wartungslog.b2clogin.com/2b6cb998-ab96-4bd7-a361-369db2e9b489/v2.0/; type: iss // B2C APP REGISTRATION GUID
claim: exp: 1638899395; type: exp
claim: nbf: 1638895795; type: nbf
claim: aud: 0d2db8a3-dafd-4c08-9382-df7e42ccd9a6; type: aud // webapi registration
claim: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier: 7dc3b595-52ee-4fda-90da-c9cb2a6030fb; type: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
claim: emails: wartungslog@gmail.com; type: emails
claim: name: Demo WartungsLOG Account; type: name
claim: tfp: B2C_1_signupandin; type: tfp
claim: http://schemas.microsoft.com/identity/claims/scope: zugriffFuerBenutzer; type: http://schemas.microsoft.com/identity/claims/scope
claim: azp: 04809bbd-20ab-43b4-acd8-c7c3df586222; type: azp // XAMARIN CLIENT APP REGISTRATION GUID 
claim: ver: 1.0; type: ver
claim: iat: 1638895795; type: iat
user primary: Demo WartungsLOG Account
user.tostring: System.Security.Claims.ClaimsPrincipal
user.identity: System.Security.Claims.ClaimsIdentity
owner: 7dc3b595-52ee-4fda-90da-c9cb2a6030fb             // USER GUID in B2C directory
claimtype.name1: 
claimtype.name2: 
claimtype.name3: 
name: 
nameidentifier: 7dc3b595-52ee-4fda-90da-c9cb2a6030fb
email: 
givenname: 
surname: 
surname: 
name: Demo WartungsLOG Account
emails: wartungslog@gmail.com
            */

            return _cosmosdbservice.GetVehicles(owner).GetAwaiter().GetResult() as IEnumerable<Vehicle>;

        }


        [HttpPost]
        public async void Post([FromBody] Vehicle vehicle)
        {
            string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            vehicle.oid = owner;
            
            vehicle.ID = Guid.NewGuid().ToString();

            _logger.LogDebug("XXXX LOG DEBUG POST VehiclesController vehicle ID:" + vehicle.ID + " kenn:" + vehicle.Kennzeichen + " desc:" + vehicle.Description + " oid:" + vehicle.oid);

            await _cosmosdbservice.AddVehicle(vehicle);

            return;

        }



            [HttpDelete("{vid}")]
            public async void Delete(string vid)
            {
                _logger.LogDebug("XXXX LOG DEBUG DELETE VehicleController vid: " + vid);

                string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Vehicle veh = new Vehicle() { ID = vid, oid = owner }; /// oh oh REMBER Kennzeichen is now the GU-ID !!


                _logger.LogDebug("XXXX LOG DEBUG DELETE VehicleController Vehicle ID: " + veh.ID + " Kennzeichen:" + veh.Kennzeichen +
                    "descr:" + veh.Description + " oid: " + veh.oid);

                await _cosmosdbservice.DeleteVehicleEntry(veh); // also delete sub entries

            }

        
    }
}
