# WartungsLOG_WL_WebAPI
Web API for WartungsLOG Xamarin APP with access to CosmosDB


![architecture](https://github.com/Stefan20162016/WartungsLOG_WL_WebAPI/blob/master/Documentation/architecture%20drawing.png)

NOTES:

- parts from https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2/tree/master/1.%20Desktop%20app%20calls%20Web%20API

- remember to use ILogger logger (especially in CosmosDBService)

- in Startup.cs static Property to get CosmosDB key from Azure App Services portal Settings->Configuration


 workflow: 

 - ServiceControllers GET/POST/DELETE for each Model-Record in \Controllers\

 - CosmosDBService.cs in \Services\ (note JsonProperty attribute in Models for "id" and "oid" == partitionkey)
 
 - ResizeImageService.cs in \Services\ using SkiaSharp to resize&convert to jpeg with 75% quality

 1. Add Vehicle (Vehicle.cs)
 2. Add Service History Entry to Vehicle (ServiceHistoryRecord.cs)
 3. Add Service Entry to History
	- which adds a picture for thumbnail list as ServiceRecord.cs 
	- full-size picture					      as ServicePictureRecord.cs


 
