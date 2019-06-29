using ApplicationCore.Interfaces;
using ApplicationCore.Services;
using Braintree;
using Google.Cloud.Diagnostics.AspNetCore;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Web.Configuration;
using Web.Services;

namespace Web
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<ArcadierSettings>(Configuration.GetSection("Arcadier"));

            string marketplaceUrl = Configuration["Arcadier:MarketplaceUrl"];
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.WithOrigins(marketplaceUrl).AllowAnyMethod().AllowAnyHeader();
            }));


            // Configure Braintree
            string environment = Configuration["Braintree:Environment"];
            string merchantId = Configuration["Braintree:MerchantId"];
            string publicKey = Configuration["Braintree:PublicKey"];
            string privateKey = Configuration["Braintree:PrivateKey"];
            services.AddSingleton<IBraintreeGateway>(s => new BraintreeGateway(environment, merchantId, publicKey, privateKey));

            string projectId = Configuration["GoogleCloud:ProjectId"];
            services.AddGoogleExceptionLogging(options =>
            {
                options.ProjectId = projectId;
                options.ServiceName = Configuration["GoogleCloud:ServiceName"];
                options.Version = Configuration["GoogleCloud:Version"];
            });

            //services.AddGoogleTrace(options =>
            //{
            //    options.ProjectId = projectId;
            //    options.Options = TraceOptions.Create(bufferOptions: BufferOptions.NoBuffer());
            //});

            services.AddScoped<IPaymentRepository>(s => new PaymentRepository(projectId));
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<ArcadierService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Configure logging service.
            loggerFactory.AddGoogle(Configuration["GoogleCloud:ProjectId"]);

            // Configure error reporting service.
            app.UseGoogleExceptionLogging();

            // Configure trace service.
            //app.UseGoogleTrace();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseCors("MyPolicy");

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
