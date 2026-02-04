namespace Receiver;

public class _Event
{
    public void Event_Ob(string[] strs)
    {
        //方式一
        var counter = new Counter();
        var consoleObserver = new ConsoleObserver();
        counter.ThresholdReached += consoleObserver.OnThresholdReached;
        counter.Increment();
        counter.Increment();
        
        Console.WriteLine("----");
        
        //方式二
        Subject<string> subject_Str = new Subject<string>();
        Subject<int> subject_int = new Subject<int>();
        
        var simpleObserver_Str = new SimpleObserver_Str("SimpleObserver");
        var simpleObserver_Int = new SimpleObserver_Int(0000);
        
        subject_Str.Subscribe(simpleObserver_Str);//注册
        subject_int.Subscribe(simpleObserver_Int);
        
        subject_Str.ChangeState("观察者模式");
        subject_int.ChangeState(1111);
        
        Thread.Sleep(1000);
        
        subject_Str.UnSubscribe(simpleObserver_Str);//取消
        subject_int.UnSubscribe(simpleObserver_Int);
        
        Console.WriteLine("----");
        
        //方式三
    }

    #region  方式一
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

    #region 方式二

    public interface ISimpleObserver<T> //使用接口作为参数
    {
        void OnStateChange(T newState);
    }

    public class Subject<T>
    {
        private readonly List<ISimpleObserver<T>> _observers;
        private T _state;

        public void Subscribe(ISimpleObserver<T> observer)//添加订阅
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
                Console.WriteLine("注册");
            }
        }

        public void UnSubscribe(ISimpleObserver<T> observer)//移除订阅
        {
            if (_observers.Contains(observer))
            {
                _observers.Remove(observer);
                Console.WriteLine("取消");
            }
        }

        public void ChangeState(T newState)
        {
            _state = newState;
            NotifyObservers();
        }

        private void NotifyObservers()
        {
            foreach (var observer in _observers)
            {
                observer.OnStateChange(_state);
            }
        }
    }

    public class SimpleObserver_Str : ISimpleObserver<string>
    {
        private readonly string _name;

        public SimpleObserver_Str(string name) => _name = name;
        public void OnStateChange(string newState)
        {
            Console.WriteLine($"[列表] Observer {_name} 收到状态：{newState}");
        }
    }
    
    public class SimpleObserver_Int : ISimpleObserver<int>
    {
        private readonly int _id;

        public SimpleObserver_Int(int id) => _id = id;
        public void OnStateChange(int newState)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}