using Orleans;
using System;
using System.Threading.Tasks;

namespace Nesh.Core.Auth
{
    public interface IAccessToken : IGrainWithStringKey
    {
        Task<Guid> GetUserId();
    }
}
