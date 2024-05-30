using Couchbase.EntityFrameworkCore.Diagnostics.Internal;
using Couchbase.EntityFrameworkCore.Infrastructure;
using Couchbase.EntityFrameworkCore.Infrastructure.Internal;
using Couchbase.EntityFrameworkCore.Query.Internal;
using Couchbase.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Couchbase.EntityFrameworkCore.Extensions;

public static class CouchbaseServiceCollectionExtensions
{
    public static IServiceCollection AddCouchbase<TContext>(
        this IServiceCollection serviceCollection,
        ClusterOptions clusterOptions,
        Action<ICouchbaseDbContextOptionsBuilder>? couchbaseOptionsAction = null,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
        => serviceCollection.AddDbContext<TContext>((_, options) =>
        {
            optionsAction?.Invoke(options);
            options.UseCouchbase(clusterOptions, couchbaseOptionsAction);
        });

    public static IServiceCollection AddEntityFrameworkCouchbaseProvider(this IServiceCollection serviceCollection, CouchbaseOptionsExtension optionsExtension)
    {
        if (serviceCollection is null)
            throw new ArgumentNullException(nameof(serviceCollection));
        
        serviceCollection.AddCouchbase(options =>
        {
            options.WithConnectionString(optionsExtension.ClusterOptions.ConnectionString);
            options.WithCredentials(optionsExtension.ClusterOptions.UserName, optionsExtension.ClusterOptions.Password);
        });
        serviceCollection.AddCouchbaseBucket<INamedBucketProvider>("default");
        serviceCollection.AddLogging(); //this should be injectible from the app side
        
        new EntityFrameworkServicesBuilder(serviceCollection)

            //The following registrations are required to pass basic Query tests (i.e. NorthwindWhereQueryRelationalTestBase)
            .TryAdd<LoggingDefinitions, CouchbaseLoggingDefinitions>()
            .TryAdd<IDatabase, CouchbaseDatabaseWrapper>()
            .TryAdd<IDatabaseProvider, DatabaseProvider<CouchbaseOptionsExtension>>()
            .TryAdd<ITypeMappingSource, CouchbaseTypeMappingSource>()
            .TryAdd<IQueryContextFactory, CouchbaseQueryContextFactory>()
            .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, CouchbaseQueryableMethodTranslatingExpressionVisitorFactory>()
            .TryAdd<IShapedQueryCompilingExpressionVisitorFactory, CouchbaseShapedQueryCompilingExpressionVisitorFactory>()
             /*.TryAdd<IRelationalTypeMappingSource, SampleProviderTypeMappingSource>()
             .TryAdd<ISqlGenerationHelper, SampleProviderSqlGenerationHelper>()
             .TryAdd<IRelationalAnnotationProvider, SampleProviderAnnotationProvider>()
             */.TryAdd<IModelValidator, CouchbaseModelValidator>()
            /*.TryAdd<IProviderConventionSetBuilder, SampleProviderConventionSetBuilder>()
            .TryAdd<IModificationCommandBatchFactory, SampleProviderModificationCommandBatchFactory>()
            .TryAdd<IRelationalConnection>(p => p.GetService<ISampleProviderRelationalConnection>()!)
            .TryAdd<IMigrationsSqlGenerator, SampleProviderMigrationsSqlGenerator>()
            .TryAdd<IRelationalDatabaseCreator, SampleProviderDatabaseCreator>()
            .TryAdd<IHistoryRepository, SampleProviderHistoryRepository>()
            .TryAdd<IRelationalQueryStringFactory, SampleProviderQueryStringFactory>()
            .TryAdd<IMethodCallTranslatorProvider, SampleProviderMethodCallTranslatorProvider>()
            .TryAdd<IAggregateMethodCallTranslatorProvider, SampleProviderAggregateMethodCallTranslatorProvider>()
            .TryAdd<IMemberTranslatorProvider, SampleProviderMemberTranslatorProvider>()
            .TryAdd<IQuerySqlGeneratorFactory, SampleProviderQuerySqlGeneratorFactory>()
            .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, SampleProviderQueryableMethodTranslatingExpressionVisitorFactory>()
            .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, SampleProviderSqlTranslatingExpressionVisitorFactory>()
            .TryAdd<IQueryTranslationPostprocessorFactory, SampleProviderQueryTranslationPostprocessorFactory>()
            .TryAdd<IUpdateSqlGenerator, SampleProviderUpdateSqlGenerator>()
            .TryAdd<ISqlExpressionFactory, SampleProviderSqlExpressionFactory>()

            //Added to main branch for Sqlite provider on Nov. 30th 2022.  Implement post EFCore 7.0.1.
            //.TryAdd<IRelationalParameterBasedSqlProcessorFactory, SampleProviderParameterBasedSqlProcessorFactory>()

            .TryAddProviderSpecificServices(serviceCollectionMap => serviceCollectionMap
                    .TryAddScoped<ISampleProviderRelationalConnection, SampleProviderRelationalConnection>()
            )*/
            .TryAddProviderSpecificServices(serviceCollectionMap => serviceCollectionMap
                .TryAddScoped<ICouchbaseClientWrapper, CouchbaseClientWrapper>()
                /*.TryAddScoped<ISqlExpressionFactory, SqlExpressionFactory>()*/)//try to use existing SQL generator
            .TryAddCoreServices();
        
        return serviceCollection;
    }
}