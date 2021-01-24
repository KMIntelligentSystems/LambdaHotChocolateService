using System;
using Amazon.Lambda;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

using Amazon.Lambda.Serialization.SystemTextJson;

//using Amazon.ApiGatewayManagementApi;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.Lambda.SQSEvents;

using Amazon;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Redis;
using Newtonsoft.Json;
using HotChocolateService;
using Newtonsoft.Json.Linq;
using System.Threading.Channels;
using ServiceStack.Script;
using System.Collections.Generic;
using HotChocolate.Language;

//[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]




namespace HotChocolateService
{
    /// <summary>
    /// This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the 
    /// actual Lambda function entry point. The Lambda handler field should be set to
    /// 
    /// MyHttpGatewayApi::MyHttpGatewayApi.LambdaEntryPoint::FunctionHandlerAsync
    /// </summary>
    public class LambdaEntryPoint : Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction
    {
        IAmazonSQS SQSClient { get; set; }
        Thread messageThread;
        Channel<string> channel = Channel.CreateUnbounded<string>();
        Channel<string> sourceChannel = Channel.CreateUnbounded<string>();


        protected override void Init(IWebHostBuilder builder)
        {
            builder
                .UseStartup<Startup>();
        }

        /// <summary>
        /// Use this override to customize the services registered with the IHostBuilder. 
        /// 
        /// It is recommended not to call ConfigureWebHostDefaults to configure the IWebHostBuilder inside this method.
        /// Instead customize the IWebHostBuilder in the Init(IWebHostBuilder) overload.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IHostBuilder builder)
        {
           // SQSClient = new AmazonSQSClient();

        }
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public State  ReceivedGraphQLMessageHandler(JObject state, ILambdaContext context)
        {
            Console.WriteLine($"State {state}");
            string o = JsonConvert.SerializeObject(state);
            DocumentNode node = ParseMessage("subscription MessageAdded {message {from {id displayName} content sentAt}}");
            MessageType mt = new MessageType();
            mt.Subscription = "subscription MessageAdded {message {from {id displayName} content sentAt}}";
            CreateRedis(mt);
            //  var graphQLProcessor = new GraphQLServiceWorker(channel, sourceChannel);

            /* messageThread = new Thread(graphQLProcessor.DoWork)
             {
                 IsBackground = true
             };
             messageThread.Start();

            await channel.Writer.WriteAsync(state.ToString());
             var en = GetAsyncEnumerator();
             await en.MoveNextAsync();
             Console.WriteLine($"ASYNC {en.Current}");*/

           

            return new State();
        }

        private DocumentNode ParseMessage(string req)
        {
            byte[] msg_ = Encoding.UTF8.GetBytes(req);
            var parser = new Utf8GraphQLParser(
              msg_, ParserOptions.Default);
            DocumentNode document = parser.Parse();
            return document;
        }

        private async ValueTask CreateRedis(MessageType state)
        {
            MessageType mt = new MessageType();
            mt.Subscription = "subscription MessageAdded {message {from {id displayName} content sentAt}}";
            mt.SchemaCode  = @"type Query { message: Message }
            type MessageFrom { id: String displayName: String }
            type Message { content: String sentAt: String messageFrom: MessageFrom }";

            using (var redis = new RedisClient("messagetyperedis-001.03brtg.0001.use1.cache.amazonaws.com", 6379))
            {
                var redisUsers = redis.As<MessageType>();
                LambdaLogger.Log("redis client created");
                redisUsers.Store(mt);
              
            }
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public MessageType SentGraphQLMessageHandler(MessageType state, ILambdaContext context)
        {
            Console.WriteLine($"State MessageType {state.SchemaCode}");
          //  Message msg = null;
            IList<Message> messages = null;
            using (var manager = new RedisManagerPool("redisserver-001.03brtg.0001.use1.cache.amazonaws.com:6379"))
            {
                using (var redis = manager.GetClient())
                {
                    var x = redis.As<Message>();
                    var m = x.GetAll();
                    messages = m;
                }
            }

            GraphQLServiceWorker worker = new GraphQLServiceWorker();
            IObservable<IFieldResolver> fieldStream = worker.ProcessGraphQLType(state.SchemaCode);
            MessageHandler<Message> handler = new MessageHandler<Message>(state.SchemaCode, fieldStream);
            DocumentNode document = worker.ParseMessage(state.Subscription);
            var en = messages.GetEnumerator();
            while (en.MoveNext())
            {
                var msg = en.Current;
                GraphQLExecutor<Message> executor = new GraphQLExecutor<Message>(msg, "{message {content messageFrom {id displayName} sentAt }}", document);
                state.result = executor.StartProcessing(handler);
                Console.WriteLine($"Move Next {state.result}");
                return state;
            }
            return null;
            
        }

    }
}
