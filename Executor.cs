using HotChocolate;
using HotChocolate.Execution;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotChocolateService
{
    public interface IExecutor
    {
        string Execute(string mutation);
    }
   public class Executor : IExecutor
    {
        public string Execute(string mutation)
        {
            var query = @"mutation($input: MessageInput) {
                addMessage(input: $input) {
                    content
                    sentAt
                    messageFrom {id displayName}
   }}";

            IReadOnlyQueryRequest request =
                      QueryRequestBuilder.New()
                      .SetQuery(query)
                      .AddVariableValue("input", new MessageInput { content = "Conent", sentAt = DateTime.Now, messageFrom = new MessageFrom { Id = "1", DisplayName = "test" } })
                      .Create();
            var schema = SchemaBuilder.New()
                .AddMutationType<Mutation>()
                .AddQueryType<Query>()
                .Create();
            var executor_ = schema.MakeExecutable();
            var result = executor_.Execute(request).ToJson();
        
            Console.WriteLine($"result..... {result}");
            Console.WriteLine($"mutation..... {mutation}");
            return result;
        }
    }
}
