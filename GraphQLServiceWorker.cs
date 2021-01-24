
using HotChocolate;
using HotChocolate.Language;
//using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Channels;



namespace HotChocolateService
{
    public interface IGraphQLServiceWorker
    {
        void DoWork();
        IObservable<IFieldResolver> ProcessGraphQLType(string schemaCode);
        DocumentNode ParseMessage(string req);
    }

 
    public class GraphQLServiceWorker : IGraphQLServiceWorker
    {
        private int executionCount = 0;
       // private readonly ILogger _logger;
        private Channel<string> _channel;
        private Channel<string> _srcChannel;

        public GraphQLServiceWorker(/*ILogger logger*/ Channel<string> channel, Channel<string> srcChannel)
        {
            _channel = channel;
            _srcChannel = srcChannel;
            //  _logger = logger;
        }

        public GraphQLServiceWorker() { }

        public async IAsyncEnumerator<string> GetAsyncEnumerator(
          CancellationToken cancellationToken = default)
        {
            while (await _channel.Reader.WaitToReadAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                yield return await _channel.Reader.ReadAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }


        public async void DoWork()
        {
           // ExecuteStepFunctionUsingDefaultProfileWithIAMStepFunctionsFullAccessInIAMConsole();

            while (true)
            {
                var en = GetAsyncEnumerator();
                await en.MoveNextAsync();
                string val = "";
                if(en.Current != null && !en.Current.Contains("connection_init"))
                {
                    Console.WriteLine("await en " + en.Current);
                    //   var docNode = ParseMessage(en.Current);
                    val = en.Current;
                    await _srcChannel.Writer.WriteAsync(val);
                   
                   // ProcessGraphQLType();
                }
            }
           
        }

        public IObservable<IFieldResolver> ProcessGraphQLType(string schemaCode)
        {
         //   _logger.LogInformation("here in logger");
          /*  var schemaCode = @"type Query { message: Message }
            type MessageFrom { id: String displayName: String }
            type Message { content: String sentAt: String messageFrom: MessageFrom }";*/
            ISchema schema = CreateSchema(schemaCode);
           // _logger.LogInformation($"Schema:{schema.Description}");
            
            FieldNodeMapper mapper = new FieldNodeMapper();
            mapper.ParseTypesFromSchema(schema);
            IObservable<IFieldResolver> fieldStream = mapper.GetFieldStream();
            Console.WriteLine($"FIELDSTREAM {fieldStream}");
            return fieldStream;
        }

        private ISchema CreateSchema(string SchemaCode)
        {
            ISchema schema = SchemaBuilder.New()
               .AddDocument(sp =>
                   Utf8GraphQLParser.Parse(SchemaCode))
               .Use(next => context =>
               {
                   context.Result = "foo";
                   return default;
               })
               .Create();

            return schema;
        }

        public  DocumentNode ParseMessage(string req)
        {
            byte[] msg_ = Encoding.UTF8.GetBytes(req);
            var parser = new Utf8GraphQLParser(
              msg_, ParserOptions.Default);
            DocumentNode document = parser.Parse();
            return document;
        }
    }
}
