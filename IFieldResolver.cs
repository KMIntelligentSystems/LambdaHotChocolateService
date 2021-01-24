using HotChocolate.Resolvers;
using System;

namespace HotChocolateService
{
    public interface IFieldResolver
    {
        /// <summary>
        /// Returns an object, null, or a <see cref="Task{TResult}"/> for the specified field. If a task is returned, then this task will be awaited to obtain the actual object.
        /// </summary>
        /// 
      //  object Resolve(ResolveFieldContext context
        string name { get; set; }
        IObservable<IFieldResolver> fieldStream { get; set; }
       
          object source { get; set; }
        // Func<object, object> func { get; set; }

        //  GraphQL.Resolvers.IFieldResolver func { get; set; }
        object Resolve();
        object Resolve(CustomContext context);
        object Resolve<T>(CustomContext context) where T : class;
    } 

    /// <inheritdoc cref="IFieldResolver"/>
    public interface IFieldResolver<out T> : IFieldResolver
    {
        /// <summary>
        /// Returns an object or null for the specified field. If <see cref="T"/> is a <see cref="Task{TResult}"/>, then this task will be awaited to obtain the actual object.
        /// </summary>
         // T Source { get; set; }
          T Resolve(CustomContext context);
        
        new T Resolve();
    }

}
