using Nesh.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nesh.Core.Manager
{
    public class EventManager<EVENT_ID>
    {
        class Event : IComparable<Event>
        {
            public int Priority { get; set; }
            public EventCallback Callback { get; set; }

            public Event()
            {
                Priority = 1;
            }

            public int CompareTo(Event other)
            {
                if (other == null)
                    return -1;

                return other.Priority - Priority;
            }
        }

        private Dictionary<EVENT_ID, List<Event>> _EventHandlers;

        public EventManager()
        {
            _EventHandlers = new Dictionary<EVENT_ID, List<Event>>();
        }

        public void Clear()
        {
            _EventHandlers.Clear();
        }

        public async Task Callback(EVENT_ID event_id, INode node, Nuid nuid, NList args)
        {
            List<Event> found = null;
            if (!_EventHandlers.TryGetValue(event_id, out found))
            {
                return;
            }

            foreach (Event event_handler in found)
            {
                await event_handler.Callback.Invoke(node, nuid, args);
            }
        }

        public void Register(EVENT_ID event_id, EventCallback event_handler, int priority = 1)
        {
            Event e = new Event() { Callback = event_handler, Priority = priority };

            List<Event> found = null;
            if (!_EventHandlers.TryGetValue(event_id, out found))
            {
                found = new List<Event>();
                _EventHandlers.Add(event_id, found);
            }

            if (found.Contains(e))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Cant Add Event Because {0} Has Exist in Event {1}",
                    event_handler.Method.Name, event_id.ToString());

                throw new Exception(sb.ToString());
            }
            else
            {
                found.Add(e);
                found.Sort();
            }
        }
    }
}
