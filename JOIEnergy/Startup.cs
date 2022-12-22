using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JOIEnergy.Domain;
using JOIEnergy.Enums;
using JOIEnergy.Generator;
using JOIEnergy.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using JOIEnergy.Middleware;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace JOIEnergy
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var readings =
                GenerateMeterElectricityReadings();

            var pricePlans = new List<PricePlan> {
                new PricePlan{
                    EnergySupplier = Enums.Supplier.DrEvilsDarkEnergy,
                    UnitRate = 10m,
                    PeakTimeMultiplier = new List<PeakTimeMultiplier>()
                },
                new PricePlan{
                    EnergySupplier = Enums.Supplier.TheGreenEco,
                    UnitRate = 2m,
                    PeakTimeMultiplier = new List<PeakTimeMultiplier>()
                },
                new PricePlan{
                    EnergySupplier = Enums.Supplier.PowerForEveryone,
                    UnitRate = 1m,
                    PeakTimeMultiplier = new List<PeakTimeMultiplier>()
                }
            };

            services.AddMvc();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IMeterReadingService, MeterReadingService>();
            services.AddTransient<IPricePlanService, PricePlanService>();
            services.AddSingleton((IServiceProvider arg) => readings);
            services.AddSingleton((IServiceProvider arg) => pricePlans);
            services.AddSingleton((IServiceProvider arg) => SmartMeterToPricePlanAccounts);
            // services.AddMvc(options => options.EnableEndpointRouting = false);

            services.AddControllers();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "ToDo API",
                    Description = "An ASP.NET Core Web API for managing ToDo items",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Example Contact",
                        Url = new Uri("https://example.com/contact")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Example License",
                        Url = new Uri("https://example.com/license")
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Console.WriteLine($"Current environment is: {env.EnvironmentName}");
            if (env.EnvironmentName == Environments.Development)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("../swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }

        private Dictionary<string, List<ElectricityReading>> GenerateMeterElectricityReadings()
        {
            var readings = new Dictionary<string, List<ElectricityReading>>();
            var generator = new ElectricityReadingGenerator();
            var smartMeterIds = SmartMeterToPricePlanAccounts.Select(mtpp => mtpp.Key);

            foreach (var smartMeterId in smartMeterIds)
            {
                readings.Add(smartMeterId, generator.Generate(20));
            }
            return readings;
        }

        public Dictionary<String, Supplier> SmartMeterToPricePlanAccounts
        {
            get
            {
                Dictionary<String, Supplier> smartMeterToPricePlanAccounts = new Dictionary<string, Supplier>();
                smartMeterToPricePlanAccounts.Add("smart-meter-0", Supplier.DrEvilsDarkEnergy);
                smartMeterToPricePlanAccounts.Add("smart-meter-1", Supplier.TheGreenEco);
                smartMeterToPricePlanAccounts.Add("smart-meter-2", Supplier.DrEvilsDarkEnergy);
                smartMeterToPricePlanAccounts.Add("smart-meter-3", Supplier.PowerForEveryone);
                smartMeterToPricePlanAccounts.Add("smart-meter-4", Supplier.TheGreenEco);
                return smartMeterToPricePlanAccounts;
            }
        }
    }
}
