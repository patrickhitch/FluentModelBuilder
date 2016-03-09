﻿using System.Linq;
using FluentModelBuilder.Configuration;
using FluentModelBuilder.Extensions;
using FluentModelBuilder.Tests.Core;
using FluentModelBuilder.TestTarget;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FluentModelBuilder.Tests
{
    public class BuildingModelForSpecificContextFixture
    {
        public IModel ModelOne;
        public IModel ModelTwo;

        public BuildingModelForSpecificContextFixture()
        {
            var services =
                new ServiceCollection();
            services.AddEntityFramework()
                    .AddInMemoryDatabase()
                    .AddDbContext<ContextOne>(x => x.UseInMemoryDatabase())
                    .AddDbContext<ContextTwo>(x => x.UseInMemoryDatabase());
            services.ConfigureEntityFramework(x => x.Add(From.AssemblyOf<EntityBase>(new TestConfiguration()).Context<ContextTwo>()));
            var provider = services.BuildServiceProvider();
            ModelOne = provider.GetService<ContextOne>().Model;
            ModelTwo = provider.GetService<ContextTwo>().Model;
        }
    }

    internal class ContextOne : DbContext
    {

    }

    internal class ContextTwo : DbContext
    {

    }

    public class BuildingModelForSpecificContext : IClassFixture<BuildingModelForSpecificContextFixture>
    {
        private readonly BuildingModelForSpecificContextFixture _fixture;

        public BuildingModelForSpecificContext(BuildingModelForSpecificContextFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void MapsEntitiesOnOneContext()
        {
            Assert.Equal(2, _fixture.ModelTwo.GetEntityTypes().Count());
        }

        [Fact]
        public void DoesNotMapEntitiesOnOtherContext()
        {
            Assert.Equal(0, _fixture.ModelOne.GetEntityTypes().Count());
        }
    }
}