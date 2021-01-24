using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace HotChocolateService
{
    public class FieldResolver_<TSource> where TSource : class
    {

        public IObservable<IFieldResolver> fieldStream { get; set; }

        public FieldResolver_()
        {

        }

      

        public IObservable<IFieldResolver> SetResolver(TSource source, Func<IType, Func<string, object>> property)
        {
            TreeSource<TSource> Tree = new TreeSource<TSource>();
            IObservable<TSource> MessageTree;

             Tree.AddValue(source);
             MessageTree = Tree.Connect();
             if (fieldStream == null)
             {
                 fieldStream = MessageTree.TransformToObservableField(property);
             }
             else
             {
                 fieldStream = fieldStream.Merge(MessageTree.TransformToObservableField(property));
             }

             return fieldStream;
        }

    }
}