using System;
using System.Collections.Generic;
using System.Linq;

using Autofac.Builder;
using Autofac.Core;

namespace App.Boilerplate.Infrastructure
{
    internal class ServicesRegistrations : IRegistrationSource
    {
        public bool IsAdapterForIndividualComponents => false;

        public IEnumerable<IComponentRegistration> RegistrationsFor(
            Service service,
            Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor
            )
        {
            if (registrationAccessor(service).Any())
            {
                yield break;
            }

            if (!(service is IServiceWithType serviceWithType))
            {
                yield break;
            }

            object instance = null;

            if (CMS.Core.Service.IsRegistered(serviceWithType.ServiceType))
            {
                instance = CMS.Core.Service.Resolve(serviceWithType.ServiceType);
            }

            if (instance == null)
            {
                yield break;
            }

            yield return RegistrationBuilder
                .ForDelegate(serviceWithType.ServiceType, (c, p) => instance)
                .CreateRegistration();
        }
    }
}