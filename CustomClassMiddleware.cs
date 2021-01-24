using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HotChocolateService
{
    public delegate ValueTask CustomDelegate(ResolverContext context);
    public class CustomClassMiddleware
    {

        //  public delegate CustomContext CustomMiddleware(CustomDelegate next);
        private readonly Func<ResolverContext, ValueTask> _next;

        public CustomClassMiddleware(Func<ResolverContext, ValueTask> next)//, string some
        {
            _next = next;
            // Some = some;
        }

        //  public string Some { get; }

        public async ValueTask InvokeAsync(ResolverContext context)
        {
            // context.Result = context.GetFieldValue(context.Field.Name.Value.ToLower());     //(context.FieldNode.Name.Value.ToLower());      
            await _next(context);
        }
    }

}
