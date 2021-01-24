using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HotChocolateService
{
    public class MessageHandler<TSource>
             where TSource : class
    {
        IObservable<IFieldResolver> FieldStream;
        TSource Source;
        IObservable<TSource> MessageTree;
        IObservable<ResolverContext> ResolverStreamer;
        TreeSource<ResolverContext> ResolverStream;
        IConnectableObservable<IFieldResolver> multiCastStream { get; set; }
        public ISchema Schema { get; set; }
        string SchemaCode { get; }
        public MessageHandler(string schemaCode, IObservable<IFieldResolver> fieldStream)
        {
            SchemaCode = schemaCode;
            FieldStream = fieldStream;
        }

        public void CreateResolvers(TSource src)
        {
            Source = src;
            TreeSource<TSource> Tree = new TreeSource<TSource>();
            Tree.AddValue(src);

            MessageTree = Tree.Connect();
            var r = new ResolverContext(new ComplexType());
            r.Value = src;
          
            r.Result = r.Name = src.GetType().Name;
            FieldStream = MessageTree.TransformToObservableField(r.GetResolver()).Merge(FieldStream);

            ResolverStream = new TreeSource<ResolverContext>();
            ResolverStream.AddValue(r);
            ResolverStreamer = ResolverStream.Connect();

            multiCastStream = FieldStream.Publish();

            var primary = multiCastStream;
            var secondary = multiCastStream;

            var selectedComplex = FieldStream.Select(val =>
            {
                IFieldResolver complex = null;
                if (val is ObservableField<ComplexType>)
                {
                    complex = val;
                }
                return complex;
            });

            primary.Select(val =>
            {
                IFieldResolver scalar = null;
                if (val is ObservableField<ScalarType>)
                {
                    scalar = val;
                }
                return scalar;
            }).Subscribe(val => CheckVal(val, selectedComplex));

            multiCastStream.Connect(); 
            
          //  ResolverStreamer.Subscribe(val => Console.WriteLine(val.Name + val.Result));

           
        }

        public ISchema GetSchema()
        {
            return CreateSchema(ResolverStreamer, SchemaCode);
        }

        private void CheckVal(IFieldResolver r, IObservable<IFieldResolver> selectedComplex)
        {
            //  TSource? Source = null;
            if (r is ObservableField<ComplexType>)
            {
                var t = r as ObservableField<ComplexType>;
            }

            if (r is ObservableField<ScalarType>)
            {
                var t = r as ObservableField<ScalarType>;

                if (Source != null)
                {
                    Type type = Source.GetType();
                    var property = type.GetProperties()//
                        .Where(x => x.Name.ToLower() == r.name.ToLower()).FirstOrDefault();
                    if (property != null)
                    {
                        var val = property.GetValue(Source);
                        try
                        {
                            Type tt = val.GetType();
                            ObservableField<ComplexType> field = CheckForComplexObject(selectedComplex, tt.Name);
                            if (field != null)
                            {
                                ComplexType ct = new ComplexType() { name = r.name };
                                ResolverContext ctx = new ResolverContext(ct);
                                ctx.Name = r.name;
                                ctx.Value = val;
                                ctx.Result = val.GetType().Name;
                                var stream = new TreeSource<ResolverContext>();
                                stream.AddValue(ctx);
                                var streamer = stream.Connect();
                                ResolverStreamer = ResolverStreamer.Merge(streamer);

                                Type ty = val.GetType();
                                var en = ty.GetProperties().GetEnumerator();
                                while (en.MoveNext())
                                {
                                    var p = en.Current as PropertyInfo;
                                    var resolverCtx = CheckForScalar(p.Name);
                                    var v = p.GetValue(val);
                                    resolverCtx.Result = v;
                                    var st = new TreeSource<ResolverContext>();
                                    st.AddValue(resolverCtx);
                                    var sStreamer = st.Connect();
                                    ResolverStreamer = ResolverStreamer.Merge(sStreamer);
                                }
                            }
                            else
                            {
                                ScalarType st = new ScalarType() { name = r.name };
                                ResolverContext ctx = new ResolverContext(st);
                                ctx.Name = r.name;
                                ctx.Result = val;
                                var stream = new TreeSource<ResolverContext>();
                                stream.AddValue(ctx);
                                var streamer = stream.Connect();
                                ResolverStreamer = ResolverStreamer.Merge(streamer);
                            }

                        }
                        catch (Exception e) { }
                    }
                    else
                    {

                        ObservableField<ComplexType> field = CheckForComplexObject(selectedComplex, t.parent);
                        if (field != null)
                        {
                            ComplexType ct = new ComplexType() { name = r.name };
                            ResolverContext ctx = new ResolverContext(ct);
                            ctx.Name = r.name;
                            var stream = new TreeSource<ResolverContext>();
                            stream.AddValue(ctx);
                            var streamer = stream.Connect();
                            ResolverStreamer = ResolverStreamer.Merge(streamer);
                        }
                        else
                        {
                            ScalarType st = new ScalarType() { name = r.name };
                            ResolverContext ctx = new ResolverContext(st);
                            ctx.Name = r.name;
                            var stream = new TreeSource<ResolverContext>();
                            stream.AddValue(ctx);
                            var streamer = stream.Connect();
                            ResolverStreamer = ResolverStreamer.Merge(streamer);
                        }
                    }
                }

            }
            else if (r is ObservableField<TSource>)
            {
                Source = r.source as TSource;
            }
        }

        private ObservableField<ComplexType> CheckForComplexObject(IObservable<IFieldResolver> secondary, string type)
        {
            ObservableField<ComplexType> field = null;

            secondary.Subscribe(val =>
            {
                if (val != null && val.name == type)
                {


                    field = val as ObservableField<ComplexType>;
                }
            });

            return field;
        }

        private ResolverContext CheckForScalar(string name)
        {
            ResolverContext ctx = null;
            var s = from c in ResolverStreamer.Where(val => val.Name.ToLower() == name.ToLower()) select c;
            s.Subscribe(val => ctx = val);

            return ctx;
        }

        private ISchema CreateSchema(IObservable<ResolverContext> resolverStream, string SchemaCode)
        {
            FieldMiddleware middleware = Create<CustomClassMiddleware>(resolverStream);

            return HotChocolate.Schema.Create(
                SchemaCode,
                configuration =>
                {
                 //   configuration.RegisterAuthorizeDirectiveType();
                    configuration.Use(middleware);
                });
        }

        public FieldMiddleware Create<TMiddleware>(IObservable<ResolverContext> resolverStream)
        where TMiddleware : class
        {
            return next => context =>
            {
                Func<Func<ResolverContext, ValueTask>, TMiddleware> activateMiddleware = next =>
                {
                    Type middlewareType = typeof(TMiddleware);
                    var instance = Activator.CreateInstance(middlewareType, next) as TMiddleware;
                    return instance;
                };

                TMiddleware middleware = activateMiddleware.Invoke(next =>
                {
                    context.Result = next.Result;
                    return default;
                });


                Func<TMiddleware, object, ValueTask> ClassQueryFunc = async (middleware, context) =>
                {
                    CustomClassMiddleware c = middleware as CustomClassMiddleware;
                    ResolverContext ctx = context as ResolverContext;
                    await c.InvokeAsync(ctx);
                };

                ResolverContext ctx = null;
                var r = from c in resolverStream.Where(res => res.Name.ToLower() == context.Field.Name.Value.ToLower()) select c;
                r.Subscribe(res => ctx = res);
                ClassQueryFunc.Invoke(middleware, ctx);
                return next(context);
            };
        }

      //  public delegate ValueTask<object> FieldResolverDelegate(ResolverContext context);
        private  Func<ResolverContext, ValueTask> CreateResolverMiddleware(
        Func<ResolverContext, ValueTask<object>> fieldResolver)
        {
            return async ctx =>
            {
                if (fieldResolver is { })
                {
                    ctx.Result = await fieldResolver(ctx).ConfigureAwait(false); //await
                }
            };
        }

        private Func<Func<ResolverContext, ValueTask>, Func<ResolverContext, ValueTask>> CreateFieldMiddleware()
        {
            return next => context =>
            {
                Func<Func<ResolverContext, ValueTask>, CustomClassMiddleware> compileMiddleware = next =>
                {
                    var instance = new CustomClassMiddleware(next);
                    return instance;
                };

                CustomClassMiddleware middleware = compileMiddleware.Invoke(next =>
                {
                    context.Result = next.Result;
                    return default;
                });


                Func<CustomClassMiddleware, ResolverContext, ValueTask<object>> ClassQueryFunc = async (middleware, context) =>
                {
                    CustomClassMiddleware c = middleware as CustomClassMiddleware;
                    await c.InvokeAsync(context);
                    return new ValueTask<object>(context.Result);
                };
                ClassQueryFunc.Invoke(middleware, context);

                return next(context);
            };
        }
    }
}
