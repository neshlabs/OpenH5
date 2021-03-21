using Orleans;
using System.Threading.Tasks;

namespace Nesh.Core.Auth
{
    public interface IUser : IGrainWithGuidKey
    {
        Task<long> GetRole(int realm);
    }
}
