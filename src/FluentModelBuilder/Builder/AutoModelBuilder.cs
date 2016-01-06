using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FluentModelBuilder.Alterations;
using FluentModelBuilder.Builder.Sources;
using FluentModelBuilder.Configuration;
using FluentModelBuilder.Extensions;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Internal;

namespace FluentModelBuilder.Builder
{
    public class AutoModelBuilder
    {
        private readonly IDictionary<Type, object> _entityTypeBuilderCache = new Dictionary<Type, object>();
        private readonly IList<InlineOverride> _inlineOverrides = new List<InlineOverride>();

        private readonly List<ITypeSource> _typeSources = new List<ITypeSource>();

        private readonly List<Type> _includedTypes = new List<Type>();
        private readonly List<Type> _ignoredTypes = new List<Type>();

        private readonly AutoModelBuilderAlterationCollection _alterations = new AutoModelBuilderAlterationCollection();

        public readonly IEntityAutoConfiguration Configuration;

        public AutoModelBuilder() : this(new DefaultEntityAutoConfiguration())
        {
            
        }
        
        public AutoModelBuilder(IEntityAutoConfiguration configuration)
        {
            Configuration = configuration;
        }

        public AutoModelBuilder Alterations(Action<AutoModelBuilderAlterationCollection> alterationDelegate)
        {
            alterationDelegate(_alterations);
            return this;
        }

        public AutoModelBuilder UseOverridesFromAssemblyOf<T>()
        {
            return UseOverridesFromAssembly(typeof (T).GetTypeInfo().Assembly);
        }

        public AutoModelBuilder UseOverridesFromAssembly(Assembly assembly)
        {
            _alterations.Add(new EntityTypeOverrideAlteration(assembly));
            return this;
        }

        //public AutoModelBuilder UseOverridesFromThisAssembly()
        //{
        //    var assembly = FindTheCallingAssembly();
        //    return UseOverridesFromAssembly(assembly);
        //}

        public AutoModelBuilder AddTypeSource(ITypeSource typeSource)
        {
            _typeSources.Add(typeSource);
            return this;
        }

        public AutoModelBuilder IncludeBase<T>()
        {
            return IncludeBase(typeof(T));
        }

        public AutoModelBuilder IncludeBase(Type type)
        {
            _includedTypes.Add(type);
            return this;
        }

        public AutoModelBuilder IgnoreBase<T>()
        {
            return IgnoreBase(typeof(T));
        }

        public AutoModelBuilder IgnoreBase(Type type)
        {
            _ignoredTypes.Add(type);
            return this;
        }

        public AutoModelBuilder AddEntityAssembly(Assembly assembly)
        {
            return AddTypeSource(new AssemblyTypeSource(assembly));
        }

        //public AutoModelBuilder AddEntitiesFromThisAssembly()
        //{
        //    var assembly = FindTheCallingAssembly();
        //    return AddEntityAssembly(assembly);
        //}

        //private static Assembly FindTheCallingAssembly()
        //{
        //    var trace = new StackTrace();

        //    var thisAssembly = typeof (AutoModelBuilder).GetTypeInfo().Assembly;
        //    Assembly callingAssembly = null;
        //    for (var i = 0; i < trace.FrameCount; i++)
        //    {
        //        var frame = trace.GetFrame(i);
        //        var assembly = frame.GetMethod().DeclaringType.Assembly;
        //        if (assembly == thisAssembly) continue;
        //        callingAssembly = assembly;
        //        break;
        //    }
        //    return callingAssembly;
        //}

        internal void AddOverride(Type type, Action<object> action)
        {
            _inlineOverrides.Add(new InlineOverride(type, action));
        }

        public AutoModelBuilder Override(Type overrideType)
        {
            var overrideMethod = typeof(AutoModelBuilder)
                .GetMethod("OverrideHelper", BindingFlags.NonPublic | BindingFlags.Instance);
            if (overrideMethod == null)
                return this;

            var overrideInterfaces = overrideType.GetInterfaces().Where(x => x.IsEntityTypeOverrideType()).ToList();
            var overrideInstance = Activator.CreateInstance(overrideType);
            
            foreach (var overrideInterface in overrideInterfaces)
            {
                var entityType = overrideInterface.GetGenericArguments().First();
                AddOverride(entityType, instance =>
                {
                    overrideMethod.MakeGenericMethod(entityType)
                        .Invoke(this, new[] { instance, overrideInstance });
                });
                
            }
            return this;
        }

        public AutoModelBuilder Override<T>(Action<EntityTypeBuilder<T>> builderAction) where T : class
        {
            _inlineOverrides.Add(new InlineOverride(typeof(T), x =>
            {
                if (x is EntityTypeBuilder<T>)
                    builderAction((EntityTypeBuilder<T>) x);
            }));
            return this;
        }

        private object EntityTypeBuilder(InternalModelBuilder builder, Type type)
        {
            if (!_entityTypeBuilderCache.ContainsKey(type))
            {
                var internalEntityTypeBuilder = builder.Entity(type, ConfigurationSource.Explicit);
                var entityTypeBuilderType = typeof (EntityTypeBuilder<>).MakeGenericType(type);
                var entityTypeBuilderInstance = Activator.CreateInstance(entityTypeBuilderType, internalEntityTypeBuilder);
                _entityTypeBuilderCache.Add(type, entityTypeBuilderInstance);
            }
            return _entityTypeBuilderCache[type];
        }

        private void OverrideHelper<T>(EntityTypeBuilder<T> builder, IEntityTypeOverride<T> mappingOverride) where T : class
        {
            mappingOverride.Override(builder);
        }

        internal void Apply(InternalModelBuilder builder)
        {
            _alterations.Apply(this);
            AddEntities(builder);
            ApplyOverrides(builder);
        }

        private void AddEntities(InternalModelBuilder builder)
        {
            var types = _typeSources.SelectMany(x => x.GetTypes());
            foreach (var type in types)
            {
                if (!Configuration.ShouldMap(type))
                    continue;
                if (!ShouldMap(type))
                    continue;

                builder.Entity(type, ConfigurationSource.Explicit);
            }
        }

        private void ApplyOverrides(InternalModelBuilder builder)
        {
            foreach (var inlineOverride in _inlineOverrides)
            {
                var entityTypeBuilderInstance = EntityTypeBuilder(builder, inlineOverride.Type);
                inlineOverride.Apply(entityTypeBuilderInstance);
            }
        }

        private bool ShouldMap(Type type)
        {
            if (_includedTypes.Contains(type))
                return true;
            if (_ignoredTypes.Contains(type))
                return false;
            if (type.GetTypeInfo().IsGenericType && _ignoredTypes.Contains(type.GetGenericTypeDefinition()))
                return false;
            if (type.GetTypeInfo().IsAbstract)
                return false;

            if (type == typeof (object))
                return false;

            return true;
        }
    }
}