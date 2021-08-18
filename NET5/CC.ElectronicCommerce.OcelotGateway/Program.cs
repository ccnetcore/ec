using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.OcelotGateway
{
    public class Program
    {
        //dotnet CC.ElectronicCommerce.OcelotGateway.dll --urls="http://*:6900"
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
              .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
              {
                  configurationBuilder.AddCommandLine(args);
                  configurationBuilder.AddJsonFile("configuration.json", optional: false, reloadOnChange: true);
              })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
