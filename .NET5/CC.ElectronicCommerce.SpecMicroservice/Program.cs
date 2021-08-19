using Com.Ctrip.Framework.Apollo;
using Com.Ctrip.Framework.Apollo.Core;
using Com.Ctrip.Framework.Apollo.Enums;
using Com.Ctrip.Framework.Apollo.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.SpecMicroservice
{
    //dotnet CC.ElectronicCommerce.SpecMicroservice.dll --urls="http://*:7007"
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                                        .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                                        {
                                            configurationBuilder.AddCommandLine(args);
                                            LogManager.UseConsoleLogging(Com.Ctrip.Framework.Apollo.Logging.LogLevel.Trace);
                                            configurationBuilder
                                                .AddApollo(configurationBuilder.Build().GetSection("apollo"))
                                                .AddDefault()
                                                .AddNamespace("CCECJson", ConfigFileFormat.Json)//�Զ����private NameSpace
                                                .AddNamespace(ConfigConsts.NamespaceApplication);//Apollo��Ĭ��NameSpace������
                                        })
                            .ConfigureLogging(loggingBuilder =>
                            {
                                loggingBuilder.AddFilter("System", Microsoft.Extensions.Logging.LogLevel.Warning);
                                loggingBuilder.AddFilter("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning);
                                loggingBuilder.AddLog4Net();
                            })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
