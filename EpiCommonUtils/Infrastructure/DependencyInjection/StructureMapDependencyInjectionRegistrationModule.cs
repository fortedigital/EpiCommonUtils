using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Mvc;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using StructureMap;
using IDependencyResolver = System.Web.Http.Dependencies.IDependencyResolver;

namespace Forte.EpiCommonUtils.Infrastructure.DependencyInjection
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class StructureMapDependencyInjectionRegistrationModule : IConfigurableModule
    {
        public void Initialize(InitializationEngine context)
        {
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            var resolver = new StructureMapDependencyResolver(context.StructureMap());
            
            var disableStructureMapMvcRegistration = ConfigurationManager.AppSettings["epiCommonUtils:disableStructureMapMvcRegistration"];
            var disableStructureMapWebApiRegistration = ConfigurationManager.AppSettings["epiCommonUtils:disableStructureMapWebApiRegistration"];

            if (disableStructureMapMvcRegistration == null || bool.Parse(disableStructureMapMvcRegistration) == false)
            {
                DependencyResolver.SetResolver(resolver);                
            }

            if (disableStructureMapWebApiRegistration == null || bool.Parse(disableStructureMapWebApiRegistration) == false)
            {
                GlobalConfiguration.Configuration.DependencyResolver = resolver;                
            }
        }

        private class StructureMapDependencyResolver : StructureMapDependencyScope, IDependencyResolver, System.Web.Mvc.IDependencyResolver
        {
            public StructureMapDependencyResolver(IContainer container) : base(container)
            {
            }

            public IDependencyScope BeginScope()
            {
                return new StructureMapDependencyScope(this.Container.GetNestedContainer());
            }
        }

        internal class StructureMapDependencyScope : IDependencyScope
        {
            protected readonly IContainer Container;

            public StructureMapDependencyScope(IContainer container)
            {
                this.Container = container;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType.IsInterface || serviceType.IsAbstract)
                    return this.Container.TryGetInstance(serviceType);

                return this.Container.GetInstance(serviceType);
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                return this.Container.GetAllInstances(serviceType).Cast<object>();
            }

            public void Dispose()
            {
                this.Container.Dispose();
            }
        }
    }
}
