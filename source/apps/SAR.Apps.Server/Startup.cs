using System.IO;
using System.Text;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SAR.Apps.Server.Modules;
using SAR.Libraries.Common.Interfaces;
using SAR.Modules.Script.Importer.Modules;
using SAR.Modules.Script.Modules;
using SAR.Modules.Server.Modules;

namespace SAR.Apps.Server
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();
            this.Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add services to the collection. Don't build or return
            // any IServiceProvider or the ConfigureContainer method
            // won't get called.
            services.AddOptions();
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you. If you
        // need a reference to the container, you need to use the
        // "Without ConfigureContainer" mechanism shown later.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            var config = Config.GetInstance();

            builder.RegisterModule(
                new SarWebServerModule(config));

            builder.RegisterModule(
                new ScriptModule());

            builder.RegisterModule(
                new ServerModule());

            builder.RegisterModule(
                new ImporterModule());
        }

        public IConfigurationRoot Configuration { get; private set; }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ISarLogger logger)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/html";

                    StringBuilder sb = new StringBuilder();

                    sb.Append("<html lang=\"en\"><body>\r\n");
                    sb.Append("ERROR!<br><br>\r\n");

                    var exceptionHandlerPathFeature =
                        context.Features.Get<IExceptionHandlerPathFeature>();

                    var error = exceptionHandlerPathFeature?.Error;
                    if (error != null)
                    {
                        sb.Append($"Error Type:{error.GetType()}<br>");

                        try
                        {
                            logger.Error(error.Message);
                        }
                        catch
                        {
                            //eat all errors
                        }

                        sb.Append(error.Message);
                    }
                    else
                    {
                        logger.Error("Unknown Error");
                    }

                    sb.Append("</body></html>\r\n");
                    await context.Response.WriteAsync(sb.ToString()); // IE padding
                });
            });
            
            DefaultFilesOptions defaultFileOptions = new DefaultFilesOptions();
            defaultFileOptions.DefaultFileNames.Clear();
            defaultFileOptions.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(defaultFileOptions);

            StaticFileOptions staticFileOptions = new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
                RequestPath = ""
            };

            app.UseStaticFiles(staticFileOptions);

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors(x =>
            {
                x.AllowAnyHeader();
                x.AllowAnyOrigin();
                x.AllowAnyMethod();
            });

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.Run(async (context) =>
            {
                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync(Path.Combine(env.WebRootPath, "index.html"));
            });
        }
    }
}
