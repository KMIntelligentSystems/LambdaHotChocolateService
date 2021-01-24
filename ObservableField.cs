using HotChocolate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HotChocolateService
{
    public enum TypeKind
    {
        ComplexType = 0,
        Scalar = 1
    }
   

    public class ObservableField<TObject> : IFieldResolver
    {
        Func<IType, Func<string, object>>? resolver;
        
        public IObservable<IFieldResolver> fieldStream { get; set; }

        public string name { get; set; }
        public string parent { get; set; }
        TypeKind kind { get;  }
        public object source { get; set; }//Requied for IFieldResolver
       // public TObject Source { get; set; }
        public IType type;
        public ObservableField(TObject _source, Func<IType, Func<string, object>> _resolver) 
        {
            if(_source is ComplexType)
            {
                var src = _source as ComplexType;
                name = src.name;
                kind = src.kind;
            } else if (_source is ScalarType)
            {
                var src = _source as ScalarType;
                name = src.name;
                kind = src.kind;
                parent = src.parentName;            }
            else
            {
                source = _source;
            }
            resolver = _resolver;
        //    resolverContext = new ResolverContext<TObject,TProp>(source);
        }

         public object Resolve()
        {
            throw new NotImplementedException();
        }

        object IFieldResolver.Resolve(CustomContext context)
        {
            return null;
        }

        public object Resolve<T>(CustomContext context) where T : class
        {
            throw new NotImplementedException();
        }

       
    }


}
