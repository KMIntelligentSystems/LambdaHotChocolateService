using HotChocolate;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HotChocolateService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<GraphQLServiceWorker>();
            services.AddSingleton<Executor>();
            services.AddGraphQLServer()
           .AddQueryType<QueryType>()
           .AddMutationType<Mutation>();
       /*     var provider = services.BuildServiceProvider();

            IReadOnlyQueryRequest request =
    QueryRequestBuilder.New()
        .SetQuery(@"{ message { content messageFrom { id displayName} sentAt } }")
        .SetServices(provider)
        .Create();
            var executor = provider.GetService<IRequestExecutor>();
            var x_ = executor.Execute(@"{ message { content messageFrom { id displayName} sentAt } }");//.ToJson();
            Console.WriteLine($"XXXXXXXX {x_}");*/
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

          /*  var schema = SchemaBuilder.New()
                 .AddMutationType<Mutation>()
                 .Create();
            var executor = schema.MakeExecutable();
            var x = executor.Execute("{message { content, sentAt, messageFrom {id, displayName}}}").ToJson();
            Console.WriteLine($"XXXX {x}");*/
            //   app.UseMiddleware<> userouting
            /* app.UseEndpoints(endPoints =>
             {
                 endPoints.Map("/", async context =>
                 {
                     await context.Response.WriteAsync("Message");
                 });
             });*/
            if (env.IsDevelopment())
            {
                
            }
        }
    }
}
