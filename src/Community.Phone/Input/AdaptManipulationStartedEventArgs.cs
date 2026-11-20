using System;
using System.Collections.Generic;
using System.Text;

namespace System.Windows.Input;

public class ManipulationEventArgs
{
    internal ManipulationEventArgs(object originalSource, IInputElement manipulationContainer, Point manipulationOrigin)
    {
        OriginalSource = originalSource;
        ManipulationContainer = manipulationContainer;
        ManipulationOrigin = manipulationOrigin;
    }
    public object OriginalSource { get; }
    public IInputElement ManipulationContainer { get; }
    public Point ManipulationOrigin { get; }
}
public class AdaptManipulationStartedEventArgs : ManipulationEventArgs
{
    internal AdaptManipulationStartedEventArgs(object originalSource, IInputElement manipulationContainer, Point manipulationOrigin)
        : base(originalSource, manipulationContainer, manipulationOrigin)
    {
    }

    public static implicit operator AdaptManipulationStartedEventArgs(ManipulationStartedEventArgs args)
    {
        return new AdaptManipulationStartedEventArgs(args.OriginalSource, args.ManipulationContainer, args.ManipulationOrigin);
    }

    public static AdaptManipulationStartedEventArgs Of(MouseButtonEventArgs args, object sender)
    {
        FrameworkElement container = sender as FrameworkElement;
        return new AdaptManipulationStartedEventArgs(args.OriginalSource, container, args.GetPosition(container));
    }
}

public class AdaptManipulationDeltaEventArgs : ManipulationEventArgs
{
    internal AdaptManipulationDeltaEventArgs(object originalSource, IInputElement manipulationContainer, Point manipulationOrigin)
        : base(originalSource, manipulationContainer, manipulationOrigin)
    {
    }

    public static implicit operator AdaptManipulationDeltaEventArgs(ManipulationDeltaEventArgs args)
    {
        return new AdaptManipulationDeltaEventArgs(args.OriginalSource, args.ManipulationContainer, args.ManipulationOrigin);
    }

    public static AdaptManipulationDeltaEventArgs Of(MouseEventArgs args, object sender)
    {
        FrameworkElement container = sender as FrameworkElement;
        return new AdaptManipulationDeltaEventArgs(args.OriginalSource, container, args.GetPosition(container));
    }
}

public class AdaptManipulationCompletedEventArgs : ManipulationEventArgs
{
    internal AdaptManipulationCompletedEventArgs(object originalSource, IInputElement manipulationContainer, Point manipulationOrigin)
        : base(originalSource, manipulationContainer, manipulationOrigin)
    {
    }

    public static implicit operator AdaptManipulationCompletedEventArgs(ManipulationCompletedEventArgs args)
    {
        return new AdaptManipulationCompletedEventArgs(args.OriginalSource, args.ManipulationContainer, args.ManipulationOrigin);
    }

    public static AdaptManipulationCompletedEventArgs Of(MouseButtonEventArgs args, object sender)
    {
        FrameworkElement container = sender as FrameworkElement;
        return new AdaptManipulationCompletedEventArgs(args.OriginalSource, container, args.GetPosition(container));
    }
}