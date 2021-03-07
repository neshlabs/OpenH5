using Nesh.Core;
using Nesh.Core.Data;
using System;
using System.Threading.Tasks;

namespace Nesh.Engine.Node
{
    public partial class Node
    {
        private async Task FixedUpdate(object arg)
        {
            if (TimerManager.HasTimer())
            {
                await TimerManager.Execute();
            }
            else
            {
                if (_TimerObj != null)
                {
                    _TimerObj.Dispose();
                    _TimerObj = null;
                }
            }
        }

        private void CheckTimer()
        {
            if (_TimerObj == null)
            {
                _TimerObj = RegisterTimer(FixedUpdate, null, TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(100));
            }
        }

        public Task AddCountdown(Nuid id, string timer, long over_millseconds, EventCallback handler)
        {
            TimerManager.AddTimer(id, timer, over_millseconds, handler);
            CheckTimer();

            return Task.CompletedTask;
        }

        public Task AddHeartbeat(Nuid id, string timer, long gap_millseconds, int count, EventCallback handler)
        {
            TimerManager.AddTimer(id, timer, gap_millseconds, count, handler);
            CheckTimer();

            return Task.CompletedTask;
        }

        public Task<bool> HasTimer(Nuid id, string timer)
        {
            return Task.FromResult(TimerManager.FindTimer(id, timer));
        }

        public Task DelTimer(Nuid id, string timer)
        {
            TimerManager.DelTimer(id, timer);
            return Task.CompletedTask;
        }
    }
}
