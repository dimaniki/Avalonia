// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Reactive.Testing;
using Avalonia.Data;
using Avalonia.Logging;
using Avalonia.UnitTests;
using Xunit;

namespace Avalonia.Base.UnitTests
{
    public class AvaloniaObjectTests_Binding
    {
        [Fact]
        public void Bind_Sets_Current_Value()
        {
            Class1 target = new Class1();
            Class1 source = new Class1();

            source.SetValue(Class1.FooProperty, "initial");
            target.Bind(Class1.FooProperty, source.GetObservable(Class1.FooProperty));

            Assert.Equal("initial", target.GetValue(Class1.FooProperty));
        }

        [Fact]
        public void Bind_NonGeneric_Sets_Current_Value()
        {
            Class1 target = new Class1();
            Class1 source = new Class1();

            source.SetValue(Class1.FooProperty, "initial");
            target.Bind((AvaloniaProperty)Class1.FooProperty, source.GetObservable(Class1.FooProperty));

            Assert.Equal("initial", target.GetValue(Class1.FooProperty));
        }

        [Fact]
        public void Bind_To_ValueType_Accepts_UnsetValue()
        {
            var target = new Class1();
            var source = new Subject<object>();

            target.Bind(Class1.QuxProperty, source);
            source.OnNext(6.7);
            source.OnNext(AvaloniaProperty.UnsetValue);

            Assert.Equal(5.6, target.GetValue(Class1.QuxProperty));
            Assert.False(target.IsSet(Class1.QuxProperty));
        }

        [Fact]
        public void Bind_Throws_Exception_For_Unregistered_Property()
        {
            Class1 target = new Class1();

            Assert.Throws<ArgumentException>(() =>
            {
                target.Bind(Class2.BarProperty, Observable.Return("foo"));
            });
        }

        [Fact]
        public void Bind_Sets_Subsequent_Value()
        {
            Class1 target = new Class1();
            Class1 source = new Class1();

            source.SetValue(Class1.FooProperty, "initial");
            target.Bind(Class1.FooProperty, source.GetObservable(Class1.FooProperty));
            source.SetValue(Class1.FooProperty, "subsequent");

            Assert.Equal("subsequent", target.GetValue(Class1.FooProperty));
        }

        [Fact]
        public void Bind_Ignores_Invalid_Value_Type()
        {
            Class1 target = new Class1();
            target.Bind((AvaloniaProperty)Class1.FooProperty, Observable.Return((object)123));
            Assert.Equal("foodefault", target.GetValue(Class1.FooProperty));
        }

        [Fact]
        public void Observable_Is_Unsubscribed_When_Subscription_Disposed()
        {
            var scheduler = new TestScheduler();
            var source = scheduler.CreateColdObservable<object>();
            var target = new Class1();

            var subscription = target.Bind(Class1.FooProperty, source);
            Assert.Equal(1, source.Subscriptions.Count);
            Assert.Equal(Subscription.Infinite, source.Subscriptions[0].Unsubscribe);

            subscription.Dispose();
            Assert.Equal(1, source.Subscriptions.Count);
            Assert.Equal(0, source.Subscriptions[0].Unsubscribe);
        }

        [Fact]
        public void Two_Way_Separate_Binding_Works()
        {
            Class1 obj1 = new Class1();
            Class1 obj2 = new Class1();

            obj1.SetValue(Class1.FooProperty, "initial1");
            obj2.SetValue(Class1.FooProperty, "initial2");

            obj1.Bind(Class1.FooProperty, obj2.GetObservable(Class1.FooProperty));
            obj2.Bind(Class1.FooProperty, obj1.GetObservable(Class1.FooProperty));

            Assert.Equal("initial2", obj1.GetValue(Class1.FooProperty));
            Assert.Equal("initial2", obj2.GetValue(Class1.FooProperty));

            obj1.SetValue(Class1.FooProperty, "first");

            Assert.Equal("first", obj1.GetValue(Class1.FooProperty));
            Assert.Equal("first", obj2.GetValue(Class1.FooProperty));

            obj2.SetValue(Class1.FooProperty, "second");

            Assert.Equal("second", obj1.GetValue(Class1.FooProperty));
            Assert.Equal("second", obj2.GetValue(Class1.FooProperty));

            obj1.SetValue(Class1.FooProperty, "third");

            Assert.Equal("third", obj1.GetValue(Class1.FooProperty));
            Assert.Equal("third", obj2.GetValue(Class1.FooProperty));
        }

        [Fact]
        public void Two_Way_Binding_With_Priority_Works()
        {
            Class1 obj1 = new Class1();
            Class1 obj2 = new Class1();

            obj1.SetValue(Class1.FooProperty, "initial1", BindingPriority.Style);
            obj2.SetValue(Class1.FooProperty, "initial2", BindingPriority.Style);

            obj1.Bind(Class1.FooProperty, obj2.GetObservable(Class1.FooProperty), BindingPriority.Style);
            obj2.Bind(Class1.FooProperty, obj1.GetObservable(Class1.FooProperty), BindingPriority.Style);

            Assert.Equal("initial2", obj1.GetValue(Class1.FooProperty));
            Assert.Equal("initial2", obj2.GetValue(Class1.FooProperty));

            obj1.SetValue(Class1.FooProperty, "first", BindingPriority.Style);

            Assert.Equal("first", obj1.GetValue(Class1.FooProperty));
            Assert.Equal("first", obj2.GetValue(Class1.FooProperty));

            obj2.SetValue(Class1.FooProperty, "second", BindingPriority.Style);

            Assert.Equal("second", obj1.GetValue(Class1.FooProperty));
            Assert.Equal("second", obj2.GetValue(Class1.FooProperty));

            obj1.SetValue(Class1.FooProperty, "third", BindingPriority.Style);

            Assert.Equal("third", obj1.GetValue(Class1.FooProperty));
            Assert.Equal("third", obj2.GetValue(Class1.FooProperty));
        }

        [Fact]
        public void Local_Binding_Overwrites_Local_Value()
        {
            var target = new Class1();
            var binding = new Subject<string>();

            target.Bind(Class1.FooProperty, binding);

            binding.OnNext("first");
            Assert.Equal("first", target.GetValue(Class1.FooProperty));

            target.SetValue(Class1.FooProperty, "second");
            Assert.Equal("second", target.GetValue(Class1.FooProperty));

            binding.OnNext("third");
            Assert.Equal("third", target.GetValue(Class1.FooProperty));
        }

        [Fact]
        public void StyleBinding_Overrides_Default_Value()
        {
            Class1 target = new Class1();

            target.Bind(Class1.FooProperty, Single("stylevalue"), BindingPriority.Style);

            Assert.Equal("stylevalue", target.GetValue(Class1.FooProperty));
        }

        [Fact]
        public void this_Operator_Returns_Value_Property()
        {
            Class1 target = new Class1();

            target.SetValue(Class1.FooProperty, "newvalue");

            Assert.Equal("newvalue", target[Class1.FooProperty]);
        }

        [Fact]
        public void this_Operator_Sets_Value_Property()
        {
            Class1 target = new Class1();

            target[Class1.FooProperty] = "newvalue";

            Assert.Equal("newvalue", target.GetValue(Class1.FooProperty));
        }

        [Fact]
        public void this_Operator_Doesnt_Accept_Observable()
        {
            Class1 target = new Class1();

            Assert.Throws<ArgumentException>(() =>
            {
                target[Class1.FooProperty] = Observable.Return("newvalue");
            });
        }

        [Fact]
        public void this_Operator_Binds_One_Way()
        {
            Class1 target1 = new Class1();
            Class2 target2 = new Class2();
            IndexerDescriptor binding = Class2.BarProperty.Bind().WithMode(BindingMode.OneWay);

            target1.SetValue(Class1.FooProperty, "first");
            target2[binding] = target1[!Class1.FooProperty];
            target1.SetValue(Class1.FooProperty, "second");

            Assert.Equal("second", target2.GetValue(Class2.BarProperty));
        }

        [Fact]
        public void this_Operator_Binds_Two_Way()
        {
            Class1 target1 = new Class1();
            Class1 target2 = new Class1();

            target1.SetValue(Class1.FooProperty, "first");
            target2[!Class1.FooProperty] = target1[!!Class1.FooProperty];
            Assert.Equal("first", target2.GetValue(Class1.FooProperty));
            target1.SetValue(Class1.FooProperty, "second");
            Assert.Equal("second", target2.GetValue(Class1.FooProperty));
            target2.SetValue(Class1.FooProperty, "third");
            Assert.Equal("third", target1.GetValue(Class1.FooProperty));
        }

        [Fact]
        public void this_Operator_Binds_One_Time()
        {
            Class1 target1 = new Class1();
            Class1 target2 = new Class1();

            target1.SetValue(Class1.FooProperty, "first");
            target2[!Class1.FooProperty] = target1[Class1.FooProperty.Bind().WithMode(BindingMode.OneTime)];
            target1.SetValue(Class1.FooProperty, "second");

            Assert.Equal("first", target2.GetValue(Class1.FooProperty));
        }

        [Fact]
        public void BindingError_Does_Not_Cause_Target_Update()
        {
            var target = new Class1();
            var source = new Subject<object>();

            target.Bind(Class1.QuxProperty, source);
            source.OnNext(6.7);
            source.OnNext(new BindingError(new InvalidOperationException("Foo")));

            Assert.Equal(6.7, target.GetValue(Class1.QuxProperty));
        }

        [Fact]
        public void BindingError_With_FallbackValue_Causes_Target_Update()
        {
            var target = new Class1();
            var source = new Subject<object>();

            target.Bind(Class1.QuxProperty, source);
            source.OnNext(6.7);
            source.OnNext(new BindingError(new InvalidOperationException("Foo"), 8.9));

            Assert.Equal(8.9, target.GetValue(Class1.QuxProperty));
        }

        [Fact]
        public void Bind_Logs_BindingError()
        {
            var target = new Class1();
            var source = new Subject<object>();
            var called = false;
            var expectedMessageTemplate = "Error binding to {Target}.{Property}: {Message}";

            LogCallback checkLogMessage = (level, area, src, mt, pv) =>
            {
                if (level == LogEventLevel.Error &&
                    area == LogArea.Binding &&
                    mt == expectedMessageTemplate)
                {
                    called = true;
                }
            };

            using (TestLogSink.Start(checkLogMessage))
            {
                target.Bind(Class1.QuxProperty, source);
                source.OnNext(6.7);
                source.OnNext(new BindingError(new InvalidOperationException("Foo")));

                Assert.Equal(6.7, target.GetValue(Class1.QuxProperty));
                Assert.True(called);
            }
        }

        /// <summary>
        /// Returns an observable that returns a single value but does not complete.
        /// </summary>
        /// <typeparam name="T">The type of the observable.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The observable.</returns>
        private IObservable<T> Single<T>(T value)
        {
            return Observable.Never<T>().StartWith(value);
        }

        private class Class1 : AvaloniaObject
        {
            public static readonly StyledProperty<string> FooProperty =
                AvaloniaProperty.Register<Class1, string>("Foo", "foodefault");

            public static readonly StyledProperty<double> QuxProperty =
                AvaloniaProperty.Register<Class1, double>("Qux", 5.6);
        }

        private class Class2 : Class1
        {
            public static readonly StyledProperty<string> BarProperty =
                AvaloniaProperty.Register<Class2, string>("Bar", "bardefault");
        }
    }
}
