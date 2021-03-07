using Nesh.Core.Data;
using Orleans;
using System.Threading.Tasks;

namespace Nesh.Core
{
    public interface INode : IGrainWithIntegerKey
    {
        Task<bool> IsActive();
        Task Active();
        Task Deactive();

        Task Command(Nuid id, int command, NList command_msg);
        Task Custom(Nuid id, int custom, NList custom_msg);

        #region ------ ------ ------ ------ ------ ------ ------Entity------ ------ ------ ------ ------ ------ ------
        Task<bool> Exists(Nuid id);
        Task<string> GetType(Nuid id);
        Task<Nuid> Create(Nuid id, string type, NList args);
        Task<Nuid> Create(string type, Nuid origin, NList args);
        Task Load(Entity entity);
        Task Entry(Nuid id);
        Task Leave(Nuid id);
        Task Destroy(Nuid id);
        Task<NList> GetEntities();
        #endregion

        #region ------ ------ ------ ------ ------ ------ ------Field------ ------ ------ ------ ------ ------ ------
        Task SetField<T>(Nuid id, string field_name, T value);
        Task<T> GetField<T>(Nuid id, string field_name);
        #endregion

        #region ------ ------ ------ ------ ------ ------ ------Table------ ------ ------ ------ ------ ------ ------
        Task<long> FindRow<T>(Nuid id, string table_name, int col, T value);
        Task<T> GetRowCol<T>(Nuid id, string table_name, long row, int col);
        Task SetRowCol<T>(Nuid id, string table_name, long row, int col, T value);
        Task<INList> GetRowValue(Nuid id, string table_name, long row);
        Task<INList> GetRows(Nuid id, string table_name);
        Task<long> AddRow(Nuid id, string table_name, NList value);
        Task SetRow(Nuid id, string table_name, long row, NList value);
        Task DelRow(Nuid id, string table_name, long row);
        Task ClearTable(Nuid id, string table_name);
        #endregion

        #region ------ ------ ------ ------ ------ ------ ------Timer------ ------ ------ ------ ------ ------ ------
        Task AddCountdown(Nuid id, string timer, long over_millseconds, EventCallback handler);
        Task AddHeartbeat(Nuid id, string timer, long gap_millseconds, int count, EventCallback handler);
        Task<bool> HasTimer(Nuid id, string timer);
        Task DelTimer(Nuid id, string timer);
        #endregion

        #region ------ ------ ------ ------ ------ ------ ------Logger------ ------ ------ ------ ------ ------ ------
        Task Info(string message);
        Task Warn(string message);
        Task Error(string message);
        #endregion
    }
}
