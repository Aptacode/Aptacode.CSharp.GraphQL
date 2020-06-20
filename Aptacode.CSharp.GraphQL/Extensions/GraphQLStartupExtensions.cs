using System.Threading.Tasks;
using Aptacode.CSharp.GraphQL.Models;
using GraphQL;
using GraphQL.Authorization;
using GraphQL.Server;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Aptacode.CSharp.GraphQL.Extensions
{
    public static class GraphQLStartupExtensions
    {
        public static void AddGraphQLWithAuth<TSchema>(this IServiceCollection services) where TSchema : Schema
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAuthorizationEvaluator, AuthorizationEvaluator>();
            services.AddTransient<IValidationRule, AuthorizationValidationRule>();

            services.AddScoped<TSchema>();

            services.AddGraphQL(o =>
                {
#if (DEBUG)
                    o.ExposeExceptions = true;
                    o.EnableMetrics = true;
#endif
                }).AddGraphTypes(ServiceLifetime.Scoped)
                .AddUserContextBuilder((u) => new GraphQLUserContext(){ User = u.User})
                .AddDataLoader();
        }

        public static void UseGraphQLWithAuth<TSchema>(this IApplicationBuilder app) where TSchema : Schema
        {
            var settings = new GraphQLSettings
            {
                BuildUserContext = async ctx =>
                {
                    var userContext = new GraphQLUserContext
                    {
                        User = ctx.User
                    };

                    return await Task.FromResult(userContext).ConfigureAwait(false);
                }
            };

            var rules = app.ApplicationServices.GetServices<IValidationRule>();
            settings.ValidationRules.AddRange(rules);

            app.UseGraphQL<TSchema>();
        }
    }
}