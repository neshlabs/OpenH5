using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Nesh.Engine.Node
{
    public partial class Node
    {
        public Task Info(string message)
        {
            _Logger.LogInformation(message);
            return Task.CompletedTask;
        }

        public Task Warn(string message)
        {
            _Logger.LogWarning(message);
            return Task.CompletedTask;
        }

        public Task Error(string message)
        {
            _Logger.LogError(message);
            return Task.CompletedTask;
        }
    }
}
