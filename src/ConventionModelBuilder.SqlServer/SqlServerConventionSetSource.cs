﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConventionModelBuilder.Options;
using ConventionModelBuilder.Sources;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.SqlServer;

namespace ConventionModelBuilder.SqlServer
{
    public class SqlServerConventionSetSource : DefaultConventionSetSource
    {
        private static readonly IConventionSetBuilder ConventionSetBuilder = new SqlServerConventionSetBuilder();
        public SqlServerConventionSetSource(bool useCoreConventions = true) : base(useCoreConventions)
        {
        }

        public override ConventionSet CreateConventionSet(ConventionModelBuilderOptions options)
        {
            var baseConventions = base.CreateConventionSet(options);
            return ConventionSetBuilder.AddConventions(baseConventions);
        }
    }
}