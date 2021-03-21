using System;
using System.Threading.Tasks;

namespace Nesh.Core.Auth
{
    public interface IRoleAgent : IAgent
    {
        Task BindSession(Guid user_id, string stream);
    }
}
