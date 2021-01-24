
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace HotChocolateService
{
        public static class ObservableExtensions
        {
            public static ChannelReader<T> AsChannelReader<T>(this IObservable<T> observable, int? maxBufferSize = null)
            {
                // This sample shows adapting an observable to a ChannelReader without 
                // back pressure, if the connection is slower than the producer, memory will
                // start to increase.

                // If the channel is bounded, TryWrite will return false and effectively
                // drop items.

                // The other alternative is to use a bounded channel, and when the limit is reached
                // block on WaitToWriteAsync. This will block a thread pool thread and isn't recommended and isn't shown here.
                var channel = maxBufferSize != null ? Channel.CreateBounded<T>(maxBufferSize.Value) : Channel.CreateUnbounded<T>();

                var disposable = observable.Subscribe(
                                    value => channel.Writer.TryWrite(value),
                                    error => channel.Writer.TryComplete(error),
                                    () => channel.Writer.TryComplete());

                // Complete the subscription on the reader completing
                channel.Reader.Completion.ContinueWith(task => disposable.Dispose());

                return channel.Reader;
            }

        public static Type GetGraphTypeFromType(this Type type, bool isNullable = false)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
                if (isNullable == false)
                {
                    throw new ArgumentOutOfRangeException(nameof(isNullable),
                        $"Explicitly nullable type: Nullable<{type.Name}> cannot be coerced to a non nullable GraphQL type. \n");
                }
            }

            Type graphType = null;
           
            /*  if (type.IsArray)
              {
                  var clrElementType = type.GetElementType();
                  var elementType = GetGraphTypeFromType(clrElementType, clrElementType.IsNullable()); // isNullable from elementType, not from parent array
                  graphType = typeof(ListGraphType<>).MakeGenericType(elementType);
              }
              else if (IsAnIEnumerable(type))
              {
                  var clrElementType = GetEnumerableElementType(type);
                  var elementType = GetGraphTypeFromType(clrElementType, clrElementType.IsNullable()); // isNullable from elementType, not from parent container
                  graphType = typeof(ListGraphType<>).MakeGenericType(elementType);
              }
              else
              {
                  graphType = GraphTypeTypeRegistry.Get(type);
              }

              if (graphType == null)
              {
                  if (type.IsEnum)
                  {
                      graphType = typeof(EnumerationGraphType<>).MakeGenericType(type);
                  }
                  else
                      throw new ArgumentOutOfRangeException(nameof(type), $"The type: {type.Name} cannot be coerced effectively to a GraphQL type");
              }

              if (!isNullable)
              {
                  graphType = typeof(NonNullGraphType<>).MakeGenericType(graphType);
              }*/

            return graphType;
        }


   /*       public static IObservable<TDestination> TransformMany<TDestination, TSource>( this IObservable<TSource> source,
                Func<TSource, IEnumerable<TDestination>> manyselector)
           {
               if (source == null)
               {
                   throw new ArgumentNullException(nameof(source));
               }

               if (manyselector == null)
               {
                   throw new ArgumentNullException(nameof(manyselector));
               }

               return new TransformMany<TSource, TDestination>(source, manyselector).Run();
           }

           public static IObservable<Node<TObject, TKey>> Transform<TObject, TKey>(this IObservable<TObject> source,
               Func<TObject, TKey> transformFactory,
               bool transformOnRefresh = false) where TObject : class
           {
               if (source == null)
               {
                   throw new ArgumentNullException(nameof(source));
               }

               if (transformFactory == null)
               {
                   throw new ArgumentNullException(nameof(transformFactory));
               }

               return new Transform<TObject, TKey>(source, transformFactory).Run();
           }


        public static IObservable<ObservableField<TObject, TProp>> TransformToField<TObject, TProp>(this IObservable<TObject> source,
          string key,
          Type type,
          Expression<Func<TObject, TProp>> expression) where TObject : class
        {
            return new TransformToField<TObject,object,TProp>(source, key,type, expression).Run();
        }

        public static IObservable<ObservableField<TObject, TProp>> TransformToField<TObject,TProp>(this IObservable<TObject> source,
          string key,Type type, IFieldResolver func ) where TObject : class
        {
            return new TransformToField<TObject, object, TProp>(source, key,type, func).Run();
        }*/

        public static IObservable<ObservableField<TObject>> TransformToObservableField<TObject>(this IObservable<TObject> source,
        Func<IType, Func<string, object>> func) where TObject : class
        {
            return new TransformToObservableField<TObject>(source, func).Run();
        }


       /* public static IObservable<Node<ExecutionNode<string, string, string>, string>> TransformToTree<TObject>(this IObservable<TObject> source,
          Func<TObject, TObject> transformFactory) where TObject : class
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (transformFactory == null)
            {
                throw new ArgumentNullException(nameof(transformFactory));
            }

            return new TransformToTree<TObject>(source, transformFactory).Run();
        }

        public static IObservable<ExecutionNode<TName, TVar, TVal>> TransformToNode<TObject, TName, TVar, TVal>(this IObservable<TObject> source, 
            Func<TObject, TName> pivotOnName, Func<TObject, TVal> pivotOnVal, Func<TObject, TVar> pivotOnVar) where TObject : class
        {

            return new TransformToNode<TObject, TName, TVar, TVal>(source, pivotOnName,  pivotOnVal, pivotOnVar).Run();
        }*/
    }
}

