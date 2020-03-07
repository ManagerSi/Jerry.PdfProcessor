using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Diagnostics;
using System.Linq;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;

namespace Jerry.PdfProcessor
{
    class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = CreateWebHostBuilder(args);
                var host = builder.Build();

                var isService = !(Debugger.IsAttached || args.Contains("--console"));
                if (isService)
                {
                    host.RunAsService();
                }
                else
                {
                    host.Run();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(AppDomain.CurrentDomain.BaseDirectory)
                .UseEnvironment(environment:Environment.GetEnvironmentVariable("ENVIRONMENT")) //根据值去获取不同的appsettings文件，若为dev，则选择appsettings.dev.json
                .ConfigureAppConfiguration((context, builder) =>
                {
                    //builder.AddXmlFile("abc.xml", false, true);
                    builder.AddJsonFile("appsettings.json", false, true);
                    builder.AddEnvironmentVariables($"{AppDomain.CurrentDomain.FriendlyName}:");
                })
                .UseStartup<Startup>();
    }
}
