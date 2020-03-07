using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Jerry.Common.Imp;
using Jerry.Common.Interface;
using Jerry.Model;
using Jerry.PdfProcessor.Logic.CommandHandle;
using Jerry.PdfProcessor.Logic.CommandHandle.Impl;
using Jerry.PdfProcessor.Logic.PDF;
using Jerry.PdfProcessor.Logic.Queue;
using Jerry.PdfProcessor.Logic.Queue.Helper;
using Jerry.Repository.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using NLog;

namespace Jerry.PdfProcessor
{
    public class DependencyContainer
    {
        static IContainer _container;
        private static readonly string _LockName = "Processor.lockName";

        public DependencyContainer() : this(null)
        {
        }

        public DependencyContainer(IMemoryCache memoryCache)
        {
            var builder = new ContainerBuilder();

            ConfigureBuilder(builder, memoryCache);

            _container = builder.Build();
        }

        public static void ConfigureBuilder(ContainerBuilder builder, IMemoryCache memoryCache)
        {
            builder.Register(c => new RabbitMqHelperFactory());
            builder.Register(c => new NLogManager()).As<ILogManager>().SingleInstance();
            builder.Register(c => new ConfigProvider(c.Resolve<IConfiguration>())).As<IConfigProvider>().SingleInstance();

            builder.Register(c => new PdfGenerator()).As<IPdfGenerator>();
            builder.Register(c => new RabbitMqChannelWriteClient(c.Resolve<ILogManager>(), c.Resolve<IConfigProvider>())).As<IChannelWriteClient>();
            builder.Register(c => new RabbitMqChannelReceivedServer(c.Resolve<ILogManager>(),c.Resolve<IConfigProvider>(),c.Resolve<ICommandHandleFactory>())).As<IChannelReceivedServer>();
            builder.Register(c => new CommandHandleFactory(c.Resolve<ILogManager>())
            {
                {CommandHandleType.SampleCommandHandle.ToString(),new SampleCommandHandle(c.Resolve<ILogManager>()) },
                {CommandHandleType.Pdf1CommandHandle.ToString(),new Pdf1CommandHandle(c.Resolve<ILogManager>(),c.Resolve<IPdfGenerator>()) }
            }).As<ICommandHandleFactory>();
        }

        public static T ResolveNamed<T>(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return _container.ResolveNamed<T>(name);
            }

            return _container.Resolve<T>();
        }

        public static T Resolve<T>(params DependencyParameter[] parameters)
        {
            if (parameters==null ||parameters.Length ==0)
            {
              return _container.Resolve<T>();
            }

            var typedParameters = new List<TypedParameter>(parameters.Length);
            typedParameters.AddRange(parameters.Select(dp => new TypedParameter(dp.Type, dp.Value)));

            return _container.Resolve<T>(typedParameters);
        }

    }

    public class DependencyParameter
    {
        public Type Type { get; set; }
        public object Value { get; set; }
    }
}
