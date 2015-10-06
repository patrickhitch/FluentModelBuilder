﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentModelBuilder.Conventions;
using FluentModelBuilder.Options;

namespace FluentModelBuilder.Extensions
{
    public static class DefaultModelBuilderConventionOptionsExtensions
    {
        /// <summary>
        /// Adds assemblies to enumerate for types
        /// </summary>
        /// <param name="options"><see cref="DefaultModelBuilderConventionOptions"/></param>
        /// <param name="assemblies">List of <see cref="Assembly"/> to add</param>
        public static void FromAssemblies(this DefaultModelBuilderConventionOptions options,
            IList<Assembly> assemblies)
        {
            options.Assemblies.Combine(assemblies);
        }
    }
}