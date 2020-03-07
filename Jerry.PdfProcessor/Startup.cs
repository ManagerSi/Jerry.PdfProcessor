using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Jerry.PdfProcessor.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;

namespace Jerry.PdfProcessor
{

    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
            services.AddMemoryCache();
            services.AddHostedService<DataPickupService>();
            services.AddHostedService<PdfGenerateSerice>();


            //add autofac
            ContainerBuilder builder = new ContainerBuilder();
            //将services中的服务填充到Autofac中
            builder.Populate(services);
            
            //配置依赖
            var sp = services.BuildServiceProvider();
            DependencyContainer.ConfigureBuilder(builder, sp.GetService<IMemoryCache>());

            ////创建容器
            //ApplicationContainer = builder.Build();
            //var injectContainer = new AutofacContainer(builder, ApplicationContainer);

            var container = builder.Build();
            return new AutofacServiceProvider(container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //env.ConfigureNLog("NLog.config");
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();
            //loggerFactory.AddNLog();
            //env.ConfigureNLog(@"XmlConfig\nlog.config");
        }
    }
}
