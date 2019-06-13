using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;

namespace RestPrint
{
    class Startup
    {
        public IConfiguration Configuration { get; }

        private readonly ILogger<Startup> _logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            _logger = logger;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            var origins = Configuration["Origins"]?.Split(';');

            if (origins != null)
            {
                foreach (var origin in origins)
                {
                    _logger.LogInformation("Allowed origin: {Origin}", origin);
                }

                app.UseCors(policy => policy.WithOrigins(origins).WithHeaders(HeaderNames.ContentType));
            }

            app.UseMvc();
        }
    }
}