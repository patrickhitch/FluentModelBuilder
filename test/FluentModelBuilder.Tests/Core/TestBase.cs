using FluentModelBuilder.InMemory;
using Microsoft.Data.Entity;
using Microsoft.Framework.DependencyInjection;

namespace FluentModelBuilder.Tests.Core
{
    public abstract class TestBase : ClassFixture
    {
        protected TestBase(ModelFixture fixture) : base(fixture)
        {
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFramework().AddDbContext<DbContext>(x =>
            {
                ConfigureOptions(x);
            }).AddInMemoryFluentProvider();
        }

        protected abstract void ConfigureOptions(DbContextOptionsBuilder options);
    }
}