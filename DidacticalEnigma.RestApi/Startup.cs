using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.RestApi.InternalServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace DidacticalEnigma.RestApi
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
            services
                .AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "DidacticalEnigma.RestApi", Version = "v1"});
                c.EnableAnnotations();
            });

            var rawConfig = Configuration.GetSection(ServiceConfiguration.ConfigurationName);
            services.Configure<ServiceConfiguration>(rawConfig);

            var config = rawConfig.Get<ServiceConfiguration>() ?? new ServiceConfiguration()
            {
                DataDirectory = Directory.Exists("/home/milleniumbug/dokumenty/PROJEKTY/DidacticalEnigma-Data/")
                    ? "/home/milleniumbug/dokumenty/PROJEKTY/DidacticalEnigma-Data/"
                    : "Z:\\DidacticalEnigma-Data"
            };

            var kernel = ServiceConfiguration.Configure(config.DataDirectory);

            services.AddSingleton<DataSourceDispatcher>(new DataSourceDispatcher(kernel.Get<IEnumerable<IDataSource>>()));
            services.AddSingleton<IStash<ParsedText>>(new Stash<ParsedText>(TimeSpan.FromMinutes(5)));
            services.AddSingleton(_ => kernel.Get<ISentenceParser>());
            services.AddSingleton(_ => kernel.Get<IRadicalSearcher>());
            services.AddSingleton(_ => kernel.Get<IKanjiRadicalLookup>());
            services.AddSingleton<RichFormattingRenderer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DidacticalEnigma.RestApi V1");
                c.RoutePrefix = "";
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
