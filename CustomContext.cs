using System;
using System.Collections.Generic;
using System.Text;

namespace HotChocolateService
{
    public class CustomContext
    {
        /*  private FieldDelegate _next;
          public IDictionary<string, object> FieldResolvers { get; set; }
          public IObjectField Field { get; set; }
          public FieldNode FieldNode { get; set; }
          public NameString Operation { get; set; }
          public IExecutionStrategy ExecutionStrategy { get; set; }
          public object Source { get; set; }
          public IObservable<IFieldResolver> Resolver { get; set; }
          public CustomContext() { }
          public CustomContext(IExecutionStrategy executionStrategy, FieldDelegate next)
          {
              _next = next;
              ExecutionStrategy = executionStrategy;
              //   TreeObservable = executionStrategy.TreeObservable;
          }
          public object Value;
          public string Type;
          public string Name;
          public void Resolve()
          {
              Resolver.Subscribe(val =>
              {
                  Source = val.source;
                  dynamic res = val.Resolve(this);
                  Name = res.Name as string;
                  Type = res.Type;
                  Value = res.Value;
              });
          }

          public void SetStream(IObservable<ResolverContext> stream)
          {

          }

          public object GetFieldValue(string key)
          {

              dynamic result = FieldResolvers[key];
              object val = null;
              if (result.Type == "complex")
              {
                  val = result.Name;
              }
              else
              {
                  val = result.Value;
              }

              return val;
          }


          public object Result { get; set; }
          public IObservable<IObservable<Node<ExecutionNode<string, string, string>, string>>> TreeObservable { get; set; }

      */
    }
    }