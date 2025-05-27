using System;
using Avalonia.Reactive;

namespace Prolizy2.Utilities;

public static class ReactiveExtensions
{
    
    public static void Subscribe<T>(this IObservable<T> observable, Action<T> onNext)
    {
        if (observable == null)
            throw new ArgumentNullException(nameof(observable));
        if (onNext == null)
            throw new ArgumentNullException(nameof(onNext));

        observable.Subscribe(new AnonymousObserver<T>(onNext));
    }
    
}