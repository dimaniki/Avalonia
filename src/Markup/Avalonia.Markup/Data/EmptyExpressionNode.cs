﻿// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Reactive.Linq;

namespace Avalonia.Markup.Data
{
    internal class EmptyExpressionNode : ExpressionNode
    {
        protected override IObservable<object> StartListening(WeakReference reference)
        {
            return Observable.Return(reference.Target);
        }
    }
}