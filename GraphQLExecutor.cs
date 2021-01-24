using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Language;
using HotChocolate.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace HotChocolateService
{
    public class GraphQLExecutor<TSource> where TSource: class
    {
        TSource Source;
        string Query;
        DocumentNode Document;
        public GraphQLExecutor(TSource source, string query, DocumentNode document)
        {
            Source = source;
            Query = query;
            Document = document;
        }
        public string StartProcessing(MessageHandler<TSource> handler)
        {
            handler.CreateResolvers(Source);
            var schema = handler.GetSchema();

            var field = schema.QueryType.Fields.FirstOrDefault<ObjectField>();

            var executor = schema.MakeExecutable();
            string res = executor.Execute(Query).ToJson();

            var node = Document.Definitions[0] as OperationDefinitionNode;
            var fieldNode = node.SelectionSet.Selections[0] as FieldNode;
            if (fieldNode != null && res.Contains(fieldNode.Name.Value))
            {
                var result = JsonConvert.SerializeObject(new { DefinitionNode = node.Name.Value, Value = res });
            }
            return FormatResponse(res);
        }

        private string FormatResponse(string item)
        {
            dynamic o = JsonConvert.DeserializeObject(item);
            var x = o.data;
            var obj = new { payload = new { data = x }, type = "data", id = "1" };
            var resp = JsonConvert.SerializeObject(obj);
            return resp;
        }
    }
}
