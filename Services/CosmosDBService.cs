using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WL_WebAPI.Models;



namespace WL_WebAPI.Services
{
    public class CosmosDBService 
    {
        private ILogger<CosmosDBService> _logger;

        private Container _container;


        public CosmosDBService(ILogger<CosmosDBService> logger)
        {
            _logger = logger;
            var list = Startup.credentialsProperty; //{ databaseName, containerName, account, key }
            string databaseName = list[0];
            string containerName = list[1];
            string account = list[2];
            string key = list[3];

            _logger.LogDebug($"XXXX: CosmosDBService constructor using: {databaseName} {containerName} {account}");


            Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);

            _container = client.GetContainer(databaseName, containerName);


        }

        //
        // Select Vehicle Section
        //

        public async Task AddVehicle(Vehicle v)
        {
            _logger.LogDebug("XXXX: COSMOSDBSERVICE AddVehicle: " + v.Kennzeichen);

            try
            {
                var result = await _container.CreateItemAsync<Vehicle>(v, new PartitionKey(v.oid));// use oid instead v.ID

                if (result.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    _logger.LogDebug("XXXX cosmos AddVehicle ERROR " + result + "statuscode: " + result.StatusCode);
                    if (result.Diagnostics != null)
                    {
                        _logger.LogDebug("XXXX cosmos AddVehicle ERROR diag:" + result.Diagnostics);
                    }
                }

                else if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    _logger.LogDebug("XXXX cosmos AddVehicle ERROR: CONFLICT (not unique) " + result);
                    if (result.Diagnostics != null)
                    {
                        _logger.LogDebug("XXXX cosmos AddVehicle ERROR diag:" + result.Diagnostics);
                    }
                }

            }
            catch (Microsoft.Azure.Cosmos.CosmosException ex)
            {
                _logger.LogDebug("Microsoft.Azure.Cosmos.CosmosException 1: " + ex);
                _logger.LogDebug("Microsoft.Azure.Cosmos.CosmosException 2 statuscode: " + ex.StatusCode); // "Conflict"

                if (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    _logger.LogDebug("Microsoft.Azure.Cosmos.CosmosException AddVehicle GOT CONFLICT for ID:" + v.ID);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("cosmos exception 1: " + ex);
                _logger.LogDebug("cosmos exception 2: " + ex.Message);
            }

            _logger.LogDebug("XXXX POST vehicle end of AddVehicle cosmos: descr " + v.Description + " oid: " + v.oid);



        }

        public async Task<IEnumerable> GetVehicles(string oid)
        {
            //string querystring = "select * from c where c.ID = 4711";
            string querystring = "select * from c where c.Kennzeichen != null";

            var query = this._container.GetItemQueryIterator<Vehicle>(new QueryDefinition(querystring),
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(oid),
                    MaxItemCount = 8
                }
                );

            List<Vehicle> results = new List<Vehicle>();
            
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }
            return results;

        }

        public async Task DeleteVehicleEntry (Vehicle veh)
        {
            _logger.LogDebug("XXXX: COSMOSDBSERVICE DeleteVehicleEntry: Kennzeichen: " + veh.Kennzeichen);

            var result = await _container.DeleteItemAsync<Vehicle>(veh.ID, new PartitionKey(veh.oid));// Kennzeichen used to save the GUID

            if (result.StatusCode == System.Net.HttpStatusCode.NoContent)  // okay status?
            {
                _logger.LogDebug($"XXXX: COSMOSDBSERVICE DeleteVehicleEntry:  OKAY: result.StatusCode: {result.StatusCode}");
            }
            else
            {
                _logger.LogDebug($"XXXX: COSMOSDBSERVICE DeleteVehicleEntry: ERROR: result.StatusCode: {result.StatusCode}");
            }


            // Get all ServiceHistory Entries and delete them (recursive delete sub entries)

            List<ServiceHistoryRecord> servicehistorylist = (List<ServiceHistoryRecord>)GetServiceHistory(veh.ID, veh.oid).GetAwaiter().GetResult(); // veh.Kennzeichen used to store GUID

            _logger.LogDebug($"XXXX: COSMOSDBSERVICE: DeleteVehicleEntry foreach: ");
            foreach (var sh in servicehistorylist)
            {
                _logger.LogDebug($"XXXX: COSMOSDBSERVICE: DeleteVehicleEntry: ServiceHistoryID: {sh.ServiceHistoryID} ref: {sh.RefVehicleID} descr: {sh.Description}");
                await DeleteServiceHistoryEntry(sh); // delete sub-entries 
            }

        }



        //
        // ServiceHistory Section
        //

        public async Task AddServiceHistory(ServiceHistoryRecord shr)
        {
            _logger.LogDebug("XXXX: COSMOSDBSERVICE AddServiceHistory: " + shr.Description);

            try
            {
                var result = await _container.CreateItemAsync<ServiceHistoryRecord>(shr, new PartitionKey(shr.oid));// use oid instead v.ID

                if (result.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    _logger.LogDebug("XXXX cosmos AddServiceHistory ERROR " + result + "statuscode: " + result.StatusCode);
                    if (result.Diagnostics != null)
                    {
                        _logger.LogDebug("XXXX cosmos AddServiceHistory ERROR diag:" + result.Diagnostics);
                    }
                }

                else if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    _logger.LogDebug("XXXX cosmos AddServiceHistory ERROR: CONFLICT (not unique) " + result);
                    if (result.Diagnostics != null)
                    {
                        _logger.LogDebug("XXXX cosmos AddServiceHistory CONFLICT ERROR diag:" + result.Diagnostics);
                    }
                }

            }
            catch (Microsoft.Azure.Cosmos.CosmosException ex)
            {
                _logger.LogDebug("Microsoft.Azure.Cosmos.CosmosException AddServiceHistory 1: " + ex);
                _logger.LogDebug("Microsoft.Azure.Cosmos.CosmosException AddServiceHistory 2 statuscode: " + ex.StatusCode); // "Conflict"

                if (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    _logger.LogDebug("Microsoft.Azure.Cosmos.CosmosException AddServiceHistory GOT CONFLICT for ID:" + shr.ServiceHistoryID);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("cosmos exception 1 AddServiceHistory: " + ex);
                _logger.LogDebug("cosmos exception 2 AddServiceHistory: " + ex.Message);
            }

            _logger.LogDebug("XXXX POST vehicle end of AddServiceHistory cosmos: descr " + shr.Description + " oid: " + shr.oid);
        }

        public async Task<IEnumerable> GetServiceHistory(string vid, string oid)
        {
            
            List<ServiceHistoryRecord> shrList = new List<ServiceHistoryRecord>();

            _logger.LogDebug($"XXXX GetServiceHistoryRecord start in cosmosdbservice");

            try
            {

                QueryDefinition query = new QueryDefinition(
                    "select * from c where c.RefVehicleID = @RefInput ")
                    .WithParameter("@RefInput", vid);


                using (FeedIterator<ServiceHistoryRecord> resultSet = _container.GetItemQueryIterator<ServiceHistoryRecord>(
                    query,
                    requestOptions: new QueryRequestOptions()
                    {
                        PartitionKey = new PartitionKey(oid),
                        MaxItemCount = 1
                    }))
                {



                    while (resultSet.HasMoreResults)
                    {

                        FeedResponse<ServiceHistoryRecord> response = await resultSet.ReadNextAsync();
                        
                        if (response.Count <= 0)
                        {
                            return shrList;
                        }

                        ServiceHistoryRecord shr = response.First();


                        shrList.AddRange(response);
                    }
                        foreach (var v in shrList)
                        {
                            _logger.LogDebug($"XXXX GetServiceHistory line iterate List in cosmosdbservice : ID {v.ServiceHistoryID} ref: {v.RefVehicleID}; date: {v.Date}");
                        }

                        return shrList;
                    
                }
                

            } catch (Exception ex)
            {
                _logger.LogDebug("XXXX: exception in cosmosdbservice getservicehistory: " + ex.Message);
                _logger.LogDebug("XXXX: exception in cosmosdbservice getservicehistory: " + ex);
            }
            
            return shrList;

        }



        public async Task DeleteServiceHistoryEntry(ServiceHistoryRecord shr)
        {
            _logger.LogDebug("XXXX: COSMOSDBSERVICE DeleteServiceHistoryEntry: " + shr.ServiceHistoryID);

            var result = await _container.DeleteItemAsync<ServiceHistoryRecord>(shr.ServiceHistoryID, new PartitionKey(shr.oid));// the serviceHistoryRecord itself


            if (result.StatusCode == System.Net.HttpStatusCode.NoContent)  // okay status?
            {
                _logger.LogDebug($"XXXX: COSMOSDBSERVICE DeleteServiceHistoryEntry:  OKAY: result.StatusCode: {result.StatusCode}");
            }
            else
            {
                _logger.LogDebug($"XXXX: COSMOSDBSERVICE DeleteServiceHistoryEntry: ERROR: result.StatusCode: {result.StatusCode}");
            }


            // Get all Service Entries and delete them

            List<ServiceRecord> servicelist = (List<ServiceRecord>)GetService(shr.ServiceHistoryID, shr.oid).GetAwaiter().GetResult();

            _logger.LogDebug($"XXXX: COSMOSDBSERVICE: DeleteServiceHistoryEntry foreach: ");
            foreach ( var v in servicelist)
            {
                _logger.LogDebug($"XXXX: COSMOSDBSERVICE: DeleteServiceHistoryEntry: {v.ServiceID} ref: {v.RefServiceHistoryID} descr: {v.Description}");
                await DeleteServiceEntry(v); // delete sub-entries i.e. service entry with thumbnail and service picture in "full-size"
            }

            


        }



        //
        // Service
        // 

        public async Task AddServiceEntry(ServiceRecord sr)
        {
            _logger.LogDebug("XXXX: COSMOSDBSERVICE AddServiceEntry: " + sr.Description);

            //
            // save ServiceRecord with thumbnail image
            //

            byte[] fullImageData = sr.ImageThumbData; // this is the full image we got from the mobile app

            // resize image
            byte [] thumbnail = ResizeImageService.GetJpegThumbnail(fullImageData);

            sr.ImageThumbData = thumbnail; // now  save thumbnail size image


            try
            {
                var result = await _container.CreateItemAsync<ServiceRecord>(sr, new PartitionKey(sr.oid));// use oid instead v.ID

                if (result.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    _logger.LogDebug("XXXX cosmos AddService ERROR " + result + "statuscode: " + result.StatusCode);
                    if (result.Diagnostics != null)
                    {
                        _logger.LogDebug("XXXX cosmos AddService ERROR diag:" + result.Diagnostics);
                    }
                }

                else if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    _logger.LogDebug("XXXX cosmos AddService ERROR: CONFLICT (not unique) " + result);
                    if (result.Diagnostics != null)
                    {
                        _logger.LogDebug("XXXX cosmos AddService CONFLICT ERROR diag:" + result.Diagnostics);
                    }
                }

            }
            catch (Microsoft.Azure.Cosmos.CosmosException ex)
            {
                _logger.LogDebug("Microsoft.Azure.Cosmos.CosmosException AddService 1: " + ex);
                _logger.LogDebug("Microsoft.Azure.Cosmos.CosmosException AddService 2 statuscode: " + ex.StatusCode); // "Conflict"

                if (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    _logger.LogDebug("Microsoft.Azure.Cosmos.CosmosException AddService GOT CONFLICT for ID:" + sr.ServiceID);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("cosmos exception 1 AddService: " + ex);
                _logger.LogDebug("cosmos exception 2 AddService: " + ex.Message);
            }

            _logger.LogDebug("XXXX POST AddServiceEntry SMALL PICTURE end of AddServiceEntry cosmos: descr " + sr.Description + " oid: " + sr.oid);

            //
            //  also save the big image as ServicePictureRecord
            //

            ServicePictureRecord spr = new ServicePictureRecord { ServicePictureID = Guid.NewGuid().ToString(),  Description = sr.Description, RefServiceID = sr.ServiceID, ImageData = fullImageData, oid = sr.oid  };


            try
            {
                var result = await _container.CreateItemAsync<ServicePictureRecord>(spr, new PartitionKey(spr.oid));// use oid instead v.ID

                if (result.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    _logger.LogDebug("XXXX cosmos AddService big pic ERROR " + result + "statuscode: " + result.StatusCode);
                    if (result.Diagnostics != null)
                    {
                        _logger.LogDebug("XXXX cosmos AddService  big pic ERROR diag:" + result.Diagnostics);
                    }
                }

                else if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    _logger.LogDebug("XXXX cosmos AddService big pic ERROR: CONFLICT (not unique) " + result);
                    if (result.Diagnostics != null)
                    {
                        _logger.LogDebug("XXXX cosmos AddService big pic CONFLICT ERROR diag:" + result.Diagnostics);
                    }
                }

            }
            catch (Microsoft.Azure.Cosmos.CosmosException ex)
            {
                _logger.LogDebug("Microsoft.Azure.Cosmos.CosmosException AddService big pic 1: " + ex);
                _logger.LogDebug("Microsoft.Azure.Cosmos.CosmosException AddService big pic 2 statuscode: " + ex.StatusCode); // "Conflict"

                if (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    _logger.LogDebug("Microsoft.Azure.Cosmos.CosmosException AddService big pic GOT CONFLICT for ID:" + sr.ServiceID);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("cosmos exception 1 AddService big pic: " + ex);
                _logger.LogDebug("cosmos exception 2 AddService big pic: " + ex.Message);
            }


            _logger.LogDebug("XXXX POST AddServiceEntry BIG PICTURE end of AddServiceEntry cosmos: descr " + sr.Description + " oid: " + sr.oid);


        }



        public async Task<IEnumerable> GetService(string sid, string oid)
        {
            List<ServiceRecord> srList = new List<ServiceRecord>();

            _logger.LogDebug($"XXXX GetService({sid},{oid}) line start in cosmosdbservice");

            try
            {
                QueryDefinition query = new QueryDefinition(
                    "select * from c where c.RefServiceHistoryID = @RefInput ").WithParameter("@RefInput", sid);

                using (FeedIterator<ServiceRecord> resultSet = _container.GetItemQueryIterator<ServiceRecord>(
                    query,
                    requestOptions: new QueryRequestOptions()
                    {
                        PartitionKey = new PartitionKey(oid),
                        MaxItemCount = 8
                    }))
                {
                    if (resultSet.HasMoreResults) 
                    {
                        _logger.LogDebug($"XXXX GetService HasMoreResults in cosmosdbservice");
                    } else
                    {
                        _logger.LogDebug($"XXXX GetService HasMoreResults == FALSE in cosmosdbservice");
                    }

                    
                    while (resultSet.HasMoreResults)
                    {
                        _logger.LogDebug($"XXXX GetService line 0 in cosmosdbservice");

                        FeedResponse<ServiceRecord> response = await resultSet.ReadNextAsync();
                        
                        _logger.LogDebug($"XXXX GetService Count: {response.Count}");

                        if (response.Count <= 0)
                        {
                            return srList;
                        }

                        //ServiceRecord sr = response.First();
                        srList.AddRange(response);
                    }

                    foreach (var v in srList)
                    {
                        _logger.LogDebug($"XXXX GetService iterate List in cosmosdbservice : ID : {v.ServiceID} ref: {v.RefServiceHistoryID}; descr: {v.Description}");
                    }

                    return srList;

                }


            }
            
            catch (Exception ex)
            {
                _logger.LogDebug("XXXX: exception in cosmosdbservice getservice Message: " + ex.Message);
                _logger.LogDebug("XXXX: exception in cosmosdbservice getservice Exception: " + ex);
            }

            return srList;

        }


        public async Task DeleteServiceEntry(ServiceRecord sr)
        {
            _logger.LogDebug("XXXX: COSMOSDBSERVICE DeleteServiceEntry: " + sr.ServiceID);

            var result = await _container.DeleteItemAsync<ServiceRecord>(sr.ServiceID, new PartitionKey(sr.oid));// the serviceRecord itself
            

            if (result.StatusCode == System.Net.HttpStatusCode.NoContent)  // okay status?
            {
                _logger.LogDebug($"XXXX: COSMOSDBSERVICE DeleteServiceEntry:  OKAY: result.StatusCode: {result.StatusCode}");
            }
            else
            {
                _logger.LogDebug($"XXXX: COSMOSDBSERVICE DeleteServiceEntry: ERROR: result.StatusCode: {result.StatusCode}");
            }


            //delete the ServicePictureRecord

            _logger.LogDebug($"XXXX: DELETE SERVICE PICTURE with sr.ServiceID: {sr.ServiceID} oid: {sr.oid}");

            var spr = GetServicePicture(sr.ServiceID, sr.oid).Result;
            _logger.LogDebug($"XXXX: DELETE SERVICE PICTURE: ID: {spr.ServicePictureID}");


            var result2 = await _container.DeleteItemAsync<ServicePictureRecord>(spr.ServicePictureID, new PartitionKey(spr.oid));

            if (result.StatusCode == System.Net.HttpStatusCode.NoContent)  // okay status?
            {
                _logger.LogDebug($"XXXX: COSMOSDBSERVICE DeleteServiceEntry: delete the ServicePictureRecord  OKAY: result.StatusCode: {result.StatusCode}");
            }
            else
            {
                _logger.LogDebug($"XXXX: COSMOSDBSERVICE DeleteServiceEntry: delete the ServicePictureRecord ERROR: result.StatusCode: {result.StatusCode}");
            }



        }

            //
            // Service Picture GetServicePicture(picID, owner)
            //

            public async Task<ServicePictureRecord> GetServicePicture(string picID, string oid)
        {
            ServicePictureRecord sp = new ServicePictureRecord();

            _logger.LogDebug($"XXXX GetServicePicture({picID},{oid}) line start in cosmosdbservice");

            try
            {
                QueryDefinition query = new QueryDefinition(
                    "select * from c where c.RefServiceID = @RefInput ").WithParameter("@RefInput", picID);

                using (FeedIterator<ServicePictureRecord> resultSet = _container.GetItemQueryIterator<ServicePictureRecord>(
                    query,
                    requestOptions: new QueryRequestOptions()
                    {
                        PartitionKey = new PartitionKey(oid),
                        MaxItemCount = 2
                    }))
                {
                    if (resultSet.HasMoreResults)
                    {
                        _logger.LogDebug($"XXXX GetServicePictureHasMoreResults in cosmosdbservice");
                    }
                    else
                    {
                        _logger.LogDebug($"XXXX GetServicePicture HasMoreResults == FALSE in cosmosdbservice");
                    }


                    while (resultSet.HasMoreResults)
                    {
                        _logger.LogDebug($"XXXX GetServicePicture line 0 in cosmosdbservice");

                        FeedResponse<ServicePictureRecord> response = await resultSet.ReadNextAsync();

                        _logger.LogDebug($"XXXX GetServicePicture Count: {response.Count}");

                        if (response.Count <= 0)
                        {
                            return sp;
                        }

                        sp = response.First();
                        if (response.Count > 1) {
                            _logger.LogDebug($"XXXX ERROR ERROR ERROR has Count > 1 GetServicePicture  in cosmosdbservice returns: ID: {sp.ServicePictureID} ref: {sp.RefServiceID}; descr: {sp.Description}");
                        }
                        
                    }
                    _logger.LogDebug($"XXXX GetServicePicture  in cosmosdbservice returns: ID: {sp.ServicePictureID} ref: {sp.RefServiceID}; descr: {sp.Description}");
                    return sp;
                }
            }

            catch (Exception ex)
            {
                _logger.LogDebug("XXXX: exception in cosmosdbservice getservicepicture Message: " + ex.Message);
                _logger.LogDebug("XXXX: exception in cosmosdbservice getservicepicture Exception: " + ex);
            }

            return sp;
        }


    }
}
