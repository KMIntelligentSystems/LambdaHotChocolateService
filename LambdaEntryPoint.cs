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
using System.Linq;
using HotChocolate;
using HotChocolate.Execution;
using ServiceStack;
using HotChocolate.Types;
using System.Linq.Expressions;
using System.Reflection;

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
        IExecutor executor { get; set; }
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
            SQSClient = new AmazonSQSClient();
            executor = new Executor();

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

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public void HandleGraphQLMessageType(SQSEvent sqsEvent, ILambdaContext context)
        {

            try
            {
                foreach (var record in sqsEvent.Records)
                {
                    Console.WriteLine($"In Graphql server {record.Body}");
                    JObject resp = JObject.Parse(record.Body);
                    var res = resp.SelectToken("payload.query");
                    Console.WriteLine($"RES {res.ToString()}");
                    MessageType mt = new MessageType();
                    mt.Subscription = res.ToString();
                    CreateRedis(mt);
                    // Console.WriteLine($"after {resp}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message }");
            }
        }


        private DocumentNode ParseMessage(string req)
        {
            byte[] msg_ = Encoding.UTF8.GetBytes(req);
            var parser = new Utf8GraphQLParser(
              msg_, ParserOptions.Default);
            DocumentNode document = parser.Parse();
            return document;
        }

        private  void CreateRedis(MessageType state)
        {
            MessageType mt = new MessageType();
           // mt.Subscription = "subscription MessageAdded {message {from {id displayName} content sentAt}}";
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
        public async Task<MessageType> ReceivedGraphQLMutationHandler(string mutation, ILambdaContext context)
        {
            dynamic obj = JsonConvert.DeserializeObject(mutation);
            if(obj != null)
            {
              //  Console.WriteLine($"Received mutation {obj.query}");
              //  Console.WriteLine($"Variaaable {obj.variables}");
            }
           
            try
            {
                /*  IReadOnlyQueryRequest request =
                         QueryRequestBuilder.New()
                         .SetQuery(@"{ message { content messageFrom { id displayName} sentAt } }")
                         .AddVariableValue("content", "test")
                         .AddVariableValue("sentAt", "1992-10-09T00:00:00Z")
                         .AddVariableValue("id", "1")
                         .AddVariableValue("messageFrom", new{id= "1", displayName = "testttt"})
                         .Create();
                  var schema = SchemaBuilder.New()
                      //.AddDocumentFromString(mutation)
                      .AddMutationType<Mutation>()
                      .AddQueryType<QueryType>()
                      .Create();
                  var executor_ = schema.MakeExecutable();
                  // var result = executor_.Execute("{message { content, sentAt, messageFrom {id, displayName}}}").ToJson();
                  var result = executor_.Execute(request).ToJson();*/
                var result = executor.Execute(mutation);
                var sqsRequest = new SendMessageRequest
                {
                    QueueUrl = "https://sqs.us-east-1.amazonaws.com/280449388741/GraphQLDataQueue",
                    MessageBody = result
                };

                await SQSClient.SendMessageAsync(sqsRequest);
            }
           catch(Exception e)
            {
                Console.WriteLine($"ERROR {e.Message}");
            }

            return new MessageType();
        }

         [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public MessageType SentGraphQLMessageHandler(MessageType state, ILambdaContext context)
        {
            var schema = SchemaBuilder.New()
                     .AddQueryType<Query>()
                     .Create();
            var executor_ = schema.MakeExecutable();
            var result = executor_.Execute("{message { content, sentAt, messageFrom {id, displayName}}}").ToJson();
            Console.WriteLine($"ZZZZZZ {result}");


            IReadOnlyDictionary<string, object> dict = new Dictionary<string, object> {
                {"content", "1"},
                {"SentAt", DateTime.Now},
                {"MessageFrom", new MessageFrom{Id = "1", DisplayName = "test"} }
            };

            
         //   var result = executor_.Execute(@"{messageFrom {id, displayName}}");

         //   Console.WriteLine($"Result {result.ToJson()}");

            /* var x_ =executor_.Execute(@"mutation { 
                     messageAdded(
                     message: {
                                 content: String, 
                                 sentAt: String, 
                                 messageFrom: {
                                 id: String, 
                                 displayName: String}
                     })  
             }", dict).ToJson();
             Console.WriteLine($"XXXXXXXX {x_}");*/
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

            Type myType = typeof(Message);
            Type[] types = new Type[3];
            types[0] = typeof(string);
            types[1] = typeof(DateTime);
            types[2] = typeof(MessageFrom);
            ConstructorInfo constructorInfoObj = myType.GetConstructor(types);
            Console.WriteLine($"Constructor {constructorInfoObj}");
         //   var constructor = typeof(Message).GetConstructor(new Type[] { typeof(MessageFrom) });
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
     //   Func<TArg, T> creator = CreateCreator<TArg, T>();

        Func<TArg, T> CreateCreator<TArg, T>()
        {
            var constructor = typeof(T).GetConstructor(new Type[] { typeof(TArg) });
            var parameter = Expression.Parameter(typeof(TArg), "p");
            var creatorExpression = Expression.Lambda<Func<TArg, T>>(
                Expression.New(constructor, new Expression[] { parameter }), parameter);
            return creatorExpression.Compile();
        }
    }
}
