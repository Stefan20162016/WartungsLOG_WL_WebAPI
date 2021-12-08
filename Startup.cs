using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Logging;
using System.Diagnostics;
using WL_WebAPI.Services;


namespace WL_WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            // set static properties for CosmosDB access: from Azure AppService portal Settings->Configuration (add codb:account, codb:key, database, container
            CosmosDB_credentials(Configuration.GetSection("codb"));

            IdentityModelEventSource.ShowPII = true; // personal info in logs

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddMicrosoftIdentityWebApi(
                options =>
                {
                 Configuration.Bind("AzureAdB2C", options);
                 options.TokenValidationParameters.NameClaimType = "name";
               //options.TokenValidationParameters.RequireSignedTokens = false;
                 options.MetadataAddress = "https://WartungsLOG.b2clogin.com/WartungsLOG.onmicrosoft.com/B2C_1_signupandin/v2.0/.well-known/openid-configuration";
                },
                options => 
                { 
                    Configuration.Bind("AzureAdB2C", options); // double ??
                }
        );

            services.AddControllers();
            services.AddSingleton<CosmosDBService>();
            //services.AddLogging();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WL_WebAPI", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                // https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-6.0&tabs=visual-studio
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WL_WebAPI v1"));
            }
            else
            {
                //app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(
                    c => {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WL_WebAPI v1");
                        c.RoutePrefix = string.Empty; // access swagger at webapiservicename.azurewebsites.net/
                    });
            }
            
            app.UseHttpsRedirection();

            app.UseRouting();        

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }

        
        public static void CosmosDB_credentials(IConfigurationSection configurationSection)
        {
            string databaseName = configurationSection.GetSection("database").Value;
            string containerName = configurationSection.GetSection("container").Value;
            string account = configurationSection.GetSection("account").Value;
            string key = configurationSection.GetSection("key").Value;

            credentialsProperty = new List<string> { databaseName, containerName, account, key };


            Debug.WriteLine($"XXXX: STARTUP: cosmosdbaccount: {account}"); // howto do logging here?
        }

        public static List<string> credentialsProperty { get; set; }
        
    }
}
