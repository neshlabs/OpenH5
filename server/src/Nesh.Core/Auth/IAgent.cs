using Nesh.Core.Data;
using Orleans;
using Orleans.Streams;
using System.Threading.Tasks;

namespace Nesh.Core.Auth
{
    public interface IAgent : IGrainWithIntegerKey
    {
        Task OnResponse(int message_id, NList message);

        Task SendMessage(NList message);
    }

    public interface IAgentObserver : IAsyncObserver<NList>
    {
    }
}
