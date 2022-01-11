using Autofac;
using Autofac.Features.ResolveAnything;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core
{
    public class CompositionRoot
    {
        public static IContainer Create()
        {
            var builder = new ContainerBuilder();

            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            return builder.Build();
        }
    }
}
