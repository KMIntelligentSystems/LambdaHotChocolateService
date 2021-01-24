using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace HotChocolateService
{
    public interface IObservableList<T> : IDisposable
    {
        public IObservable<T> Connect();
    }

    public sealed class TreeSource<T> : IObservableList<T> where T : class
    {
        private SerialDisposable _innerSubscription;
        private readonly ISubject<T> _changes = new Subject<T>();
        private readonly object _locker = new object();
        private Dictionary<string, T> fields = new Dictionary<string, T>();
        private T Value;
        private IDisposable _subscription;
        // public ITree<Node<ExecutionNode, string>> ExecutionTree { get; set; }
        public IObservable<T> Connect()
        {
            // _innerSubscription = new SerialDisposable();
            var subscription = new SingleAssignmentDisposable();
            _subscription = subscription;
            
            var observable = Observable.Create<T>(observer =>
            {
                lock (_locker)
                {
                    if (Value != null)
                    {

                        // var name = kvp.Key;  
                        var node = Value;
                        observer.OnNext(node);

                    }


                    var source = _changes.Finally(observer.OnCompleted);
                    // subscription.Disposable = source.SubscribeSafe(observer);
                    return source.SubscribeSafe(observer);
                }
            });
            return observable;
        }

        public void Disconnect()
        {
            //  _subscription.Dispose();
            _changes.OnCompleted();
        }

   /*     public void Add(T src, string val, Type type, string name)
        {
            var x = src as Node<ExecutionNode<string, string, string>, string>;

            x.AddValue(x, val, name);
            //  x.AddValue(x,val);
          
            Value = x as T;


        }

        public void AddWithFields(T src, IObservable<IFieldResolver> fieldStream, CustomContext context)//object
        {
            var node = src as Node<ExecutionNode<string, string, string>, string>;
            if (fieldStream != null)
            {
                if (node.Children?.Any() == true)
                {
                    foreach (Node<ExecutionNode<string, string, string>, string> child in node.Children)
                    {
                        var fieldResolver = from c in fieldStream where c.name == child.Key select c;
                        //context.Source = fieldResolver..Source;
                        fieldResolver.Subscribe(val =>
                        {
                            context.Source = val.source;
                            dynamic res = val.Resolve(context);
                            string Name = res.Name as string;
                            string Value = JsonConvert.SerializeObject(res.Value);
                            Type type = res.Type as Type;
                            child.AddValue(child, Value, Name);

                        });
                    }

                }
            }
           
            Value = node as T;
        }

        void Check(IObservable<IFieldResolver> fieldStream, Node<ExecutionNode<string, string, string>, string> node, CustomContext context)
        {
            fieldStream.Subscribe(val => Console.WriteLine(val.name));
        }

        private Node<ExecutionNode<string, string, string>, string> Check(Node<ExecutionNode<string, string, string>, string> x, Node<ExecutionNode<string, string, string>, string> y)
        {
            return x;
        }*/
        public void AddValue(T _value)
        {
            Value = _value;
        }

        public void Dispose()
        {
            lock (_locker)
            {
                _changes.OnCompleted();
            }
        }
        

    }
}
