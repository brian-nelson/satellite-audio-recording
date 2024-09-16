using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using SAR.Libraries.Database.Services;
using ConsoleTraceListener = SAR.Libraries.Common.Trace.ConsoleTraceListener;

namespace SAR.Apps.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            var config = Config.GetInstance();

            var host = new WebHostBuilder()
                .UseKestrel(options => options.AddServerHeader = false)
                .ConfigureServices(services =>
                {
                    services.AddCors();
                    services.AddControllers()
                        .AddNewtonsoftJson(options =>
                            options.SerializerSettings.ContractResolver = new DefaultContractResolver());
                    services.AddMvc();

                    services.AddAutofac(c =>
                    {

                    });

                    services.AddAuthentication(x =>
                    {
                        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.SaveToken = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(config.JwtSecret)),
                            ValidateIssuer = false,
                            ValidateAudience = false
                        };
                        options.Events = new JwtBearerEvents
                        {
                            OnAuthenticationFailed = context =>
                            {
                                return Task.CompletedTask;
                            }
                        };
                    });
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .ConfigureKestrel((context, options) =>
                {
                    // Set properties and call methods on options
                    options.Listen(IPAddress.Any, config.PortNumber, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                        //listenOptions.UseHttps("testCert.pfx", "testPassword");
                    });
                })
                .Build();

            var backupService = host.Services.GetService<BackupService>();
            backupService.Start();

            host.Run();

            backupService.Stop();
        }
    }
}
