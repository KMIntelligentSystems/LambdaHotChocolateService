using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace HotChocolateService
{
        public class TransformToObservableField<TObject>
        where TObject : class
        {
            private readonly IObservable<TObject> _source;
            private readonly Func<IType, Func<string, object>> _pivotOn;
            private readonly IType type;

            public TransformToObservableField(IObservable<TObject> source, Func<IType, Func<string, object>> func)
            {
                _source = source;
                _pivotOn = func;
            }

            public IObservable<ObservableField<TObject>> Run()
            {
                 return Observable.Create<ObservableField<TObject>>(observer =>
                {
                        var t = _source.Select((t) => new ObservableField<TObject>(t, _pivotOn));
                        return t.SubscribeSafe(observer);

                });
            }
    }

}
