﻿using System.Reflection;
using FluentModelBuilder.Core.Contributors.Impl;
using FluentModelBuilder.Core.Criteria;

namespace FluentModelBuilder.Core.Contributors.Extensions
{
    public static class DiscoveryOverrideContributorExtensions
    {
        /// <summary>
        /// Adds an assembly containing the specified type to look entities from
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="contributor"></param>
        /// <returns></returns>
        public static OverrideDiscoveryContributor AssemblyContaining<T>(this OverrideDiscoveryContributor contributor)
        {
            return contributor.AddAssembly(typeof(T).GetTypeInfo().Assembly);
        }

        /// <summary>
        /// Specifies a criterion to match on the base type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="contributor"></param>
        /// <returns></returns>
        public static OverrideDiscoveryContributor BaseType<T>(this OverrideDiscoveryContributor contributor)
        {
            return
                contributor.NotAbstract()
                    .WithCriterion<BaseTypeCriterion>(c => c.AddType(typeof(T).GetTypeInfo()));
        }
    }
}
