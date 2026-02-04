using System;
using System.Collections.Generic;

namespace Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            // 案例1：.NET 事件 / EventHandler 版观察者
            var counter = new Counter();
            var consoleObserver = new ConsoleObserver();
            counter.ThresholdReached += consoleObserver.OnThresholdReached;
            counter.Increment();
            counter.Increment();

            Console.WriteLine("----");

            // 案例2：自定义委托 + 订阅列表
            var subject = new Subject();
            var obsA = new SimpleObserver("A");
            var obsB = new SimpleObserver("B");
            subject.Subscribe(obsA);
            subject.Subscribe(obsB);
            subject.ChangeState("First State");

            Console.WriteLine("----");

            // 案例3：IObservable<T> / IObserver<T> 简易实现
            var temperatureProvider = new TemperatureProvider();
            var tempObserver = new TemperatureObserver("Screen");
            using (temperatureProvider.Subscribe(tempObserver))
            {
                temperatureProvider.PushTemperature(25.3f);
                temperatureProvider.PushTemperature(26.1f);
            } // 这里自动取消订阅
        }
    }

    #region 案例1：事件 / EventHandler

    public class ThresholdReachedEventArgs : EventArgs
    {
        public int CurrentCount { get; }
        public DateTime TimeReached { get; }

        public ThresholdReachedEventArgs(int currentCount)
        {
            CurrentCount = currentCount;
            TimeReached = DateTime.Now;
        }
    }

    // 被观察者（Subject）
    public class Counter
    {
        private int _count = 0;
        private const int Threshold = 2;

        // 观察者通过事件订阅
        public event EventHandler<ThresholdReachedEventArgs> ThresholdReached;

        public void Increment()
        {
            _count++;
            if (_count >= Threshold)
            {
                ThresholdReached?.Invoke(this, new ThresholdReachedEventArgs(_count));
            }
        }
    }

    // 观察者（Observer）
    public class ConsoleObserver
    {
        public void OnThresholdReached(object sender, ThresholdReachedEventArgs e)
        {
            Console.WriteLine($"[事件] Counter={e.CurrentCount}, Time={e.TimeReached:HH:mm:ss}");
        }
    }

    #endregion

    #region 案例2：自定义委托 + 订阅列表

    public interface ISimpleObserver
    {
        void OnStateChanged(string newState);
    }

    public class Subject
    {
        private readonly List<ISimpleObserver> _observers = new();
        private string _state;

        public void Subscribe(ISimpleObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void Unsubscribe(ISimpleObserver observer)
        {
            _observers.Remove(observer);
        }

        public void ChangeState(string newState)
        {
            _state = newState;
            NotifyObservers();
        }

        private void NotifyObservers()
        {
            foreach (var observer in _observers)
            {
                observer.OnStateChanged(_state);
            }
        }
    }

    public class SimpleObserver : ISimpleObserver
    {
        private readonly string _name;

        public SimpleObserver(string name) => _name = name;

        public void OnStateChanged(string newState)
        {
            Console.WriteLine($"[列表] Observer {_name} 收到状态：{newState}");
        }
    }

    #endregion

    #region 案例3：IObservable<T> / IObserver<T>

    // 被观察者
    public class TemperatureProvider : IObservable<float>
    {
        private readonly List<IObserver<float>> _observers = new();

        public IDisposable Subscribe(IObserver<float> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }

        public void PushTemperature(float value)
        {
            foreach (var obs in _observers)
            {
                obs.OnNext(value);
            }
        }

        private class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<float>> _observers;
            private readonly IObserver<float> _observer;

            public Unsubscriber(List<IObserver<float>> observers, IObserver<float> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }
    }

    // 观察者
    public class TemperatureObserver : IObserver<float>
    {
        private readonly string _name;

        public TemperatureObserver(string name) => _name = name;

        public void OnCompleted()
        {
            Console.WriteLine($"[IObserver] {_name} 完成");
        }

        public void OnError(Exception error)
        {
            Console.WriteLine($"[IObserver] {_name} 出错：{error.Message}");
        }

        public void OnNext(float value)
        {
            Console.WriteLine($"[IObserver] {_name} 收到温度：{value}°C");
        }
    }

    #endregion
}