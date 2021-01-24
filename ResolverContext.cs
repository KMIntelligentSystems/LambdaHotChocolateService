using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotChocolateService
{
    
    public class ResolverContext
    {
        IType FieldType;
        public object Value { get; set; }
        public object Result { get; set; }
        public string Name { get; set; }
        //must be static - Instance fields cannot be used to initialize other instance fields outside a method. public int i = 5;   public int j = i;  // CS0236
        Func<IType, Func<string, object>> resolver = (type) => {
            {
                var item = type as IType;
                var fldName = item.name;
                return fldName =>
                {
                    return null;
                };
            }
        };

        public ResolverContext(IType type)
        {
            FieldType = type;
        }

        public Func<string, object> SetResolver(object value)
        {
            value = value;
            return resolver(FieldType);
        }

        public Func<IType, Func<string, object>> GetResolver()
        {
            return resolver;
        }
    }
}
