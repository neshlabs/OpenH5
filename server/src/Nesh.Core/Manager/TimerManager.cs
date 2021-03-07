using Nesh.Core.Data;
using Nesh.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nesh.Core.Manager
{
    public class TimerManager
    {
        class Timer
        {
            public int Serial { get; private set; }
            public Nuid Id { get; private set; }
            public long GapTicks { get; private set; }
            public int RemainBeatCount { get; private set; }
            public int MaxBeatCount { get; private set; }
            public EventCallback Func { get; private set; }
            public string Name { get; private set; }
            public long StartTicks { get; private set; }
            public long StopTicks { get; private set; }

            public static Timer New(int new_serial, Nuid id, string name,
                long gap_ticks, int beat_count, long start_ticks, long stop_ticks, EventCallback func)
            {
                Timer timer = new Timer();
                timer.Id = id;
                timer.Name = name;
                timer.GapTicks = gap_ticks;
                timer.Func = func;
                timer.RemainBeatCount = beat_count;
                timer.MaxBeatCount = beat_count;
                timer.Serial = new_serial;
                timer.StartTicks = start_ticks;
                timer.StopTicks = stop_ticks;

                return timer;
            }

            public void Beat()
            {
                RemainBeatCount--;
            }

            public void Reset()
            {
                StopTicks = StartTicks + (MaxBeatCount - RemainBeatCount + 1) * GapTicks;
            }
        }

        public TimerManager(INode node)
        {
            _Node = node;
            _TimerDic = new Dictionary<int, Timer>();
            _ObjectDic = new Dictionary<Tuple<Nuid, string>, int>();
            _TimerHeap = new SortedDictionary<long, HashSet<int>>();
            _DelTimers = new Queue<int>();
            _AddTimers = new Queue<int>();
        }

        private INode _Node;

        private Dictionary<int, Timer> _TimerDic;

        private Dictionary<Tuple<Nuid, string>, int> _ObjectDic;

        private SortedDictionary<long, HashSet<int>> _TimerHeap;

        private Queue<int> _DelTimers;

        private Queue<int> _AddTimers;

        private int _Serial = 0;

        public void Clear()
        {
            _TimerDic.Clear();
            _ObjectDic.Clear();
            _TimerHeap.Clear();
            _DelTimers.Clear();
            _AddTimers.Clear();
        }

        public bool HasTimer() { return _TimerDic.Count > 0; }

        public bool FindTimer(Nuid id, string timer_name)
        {
            Tuple<Nuid, string> key = new Tuple<Nuid, string>(id, timer_name);

            return _ObjectDic.ContainsKey(key);
        }

        public void AddTimer(Nuid id, string timer_name, long over_millseconds, EventCallback callback)
        {
            AddTimer(id, timer_name, over_millseconds, 1, callback);
        }

        public void AddTimer(Nuid id, string timer_name, long gap_millseconds, int count, EventCallback callback)
        {
            if (gap_millseconds <= 0)
            {
                return;
            }

            if (count == 0)
            {
                return;
            }

            Tuple<Nuid, string> tuple = new Tuple<Nuid, string>(id, timer_name);
            if (_ObjectDic.ContainsKey(tuple))
            {
                return;
            }

            long now_time = TimeUtils.NowMilliseconds;
            long start_time = now_time;
            long stop_time = now_time + gap_millseconds;

            _Serial++;
            Timer timer = Timer.New(_Serial, id, timer_name, gap_millseconds, count, start_time, stop_time, callback);
            _TimerDic.Add(timer.Serial, timer);

            _ObjectDic.Add(tuple, timer.Serial);

            HashSet<int> timers = null;
            if (!_TimerHeap.TryGetValue(stop_time, out timers))
            {
                timers = new HashSet<int>();
                _TimerHeap.Add(stop_time, timers);
            }

            timers.Add(timer.Serial);
        }

        public void DelTimer(Nuid cuid, string timer_name)
        {
            Tuple<Nuid, string> tuple = new Tuple<Nuid, string>(cuid, timer_name);

            int serial = 0;
            if (!_ObjectDic.TryGetValue(tuple, out serial))
            {
                return;
            }

            _DelTimers.Enqueue(serial);
        }

        public async Task Execute()
        {
            while (_DelTimers.Count > 0)
            {
                int serial = _DelTimers.Dequeue();
                Timer timer = null;
                if (!_TimerDic.TryGetValue(serial, out timer))
                {
                    continue;
                }

                Tuple<Nuid, string> group = new Tuple<Nuid, string>(timer.Id, timer.Name);

                if (_ObjectDic.ContainsKey(group))
                {
                    _ObjectDic.Remove(group);
                }

                HashSet<int> timers = null;
                if (_TimerHeap.TryGetValue(timer.StopTicks, out timers))
                {
                    if (timers.Contains(serial))
                    {
                        timers.Remove(serial);
                    }
                }

                _TimerDic.Remove(serial);
            }

            while (_TimerHeap.Count > 0)
            {
                long now_ticks = TimeUtils.NowMilliseconds;

                if (_TimerHeap.ElementAt(0).Key > now_ticks)
                {
                    break;
                }

                foreach (int serial in _TimerHeap.Values.First())
                {
                    Timer timer = null;
                    if (!_TimerDic.TryGetValue(serial, out timer))
                    {
                        continue;
                    }

                    timer.Beat();

                    if (timer.RemainBeatCount == 0)
                    {
                        _DelTimers.Enqueue(timer.Serial);
                    }
                    else if (timer.RemainBeatCount > 0)
                    {
                        _AddTimers.Enqueue(timer.Serial);
                        timer.Reset();
                    }

                    NList args = NList.New();
                    args.Add(timer.Name);
                    args.Add(now_ticks);
                    args.Add(timer.RemainBeatCount);

                    await timer.Func.Invoke(_Node, timer.Id, args);
                }

                _TimerHeap.Remove(_TimerHeap.Keys.First());
            }

            while (_AddTimers.Count > 0)
            {
                int serial = _AddTimers.Dequeue();
                Timer timer = null;
                if (!_TimerDic.TryGetValue(serial, out timer))
                {
                    continue;
                }

                HashSet<int> timers = null;
                if (!_TimerHeap.TryGetValue(timer.StopTicks, out timers))
                {
                    timers = new HashSet<int>();
                    _TimerHeap.Add(timer.StopTicks, timers);
                }

                timers.Add(timer.Serial);
            }
        }
    }
}
