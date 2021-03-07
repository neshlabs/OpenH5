using Nesh.Core;
using Nesh.Core.Data;
using Nesh.Core.Utils;
using System.Threading.Tasks;

namespace Nesh.Engine.Node
{
    public partial class Node
    {
        public async Task SetField<T>(Nuid id, string field_name, T field_value)
        {
            if (field_value == null) return;

            Entity entity = EntityManager.Get(id);

            if (entity != null)
            {
                Field field = entity.GetField(field_name);
                if (field == null)
                {
                    return;
                }

                NList result;
                if (!field.TrySet(field_value, out result))
                {
                    return;
                }

                BatchCahceList.Add(NList.New().Add((int)CacheOption.SetField).Add(id).Add(field_name).Add(ProtoUtils.Serialize(field_value)));

                await CallbackField(id, field_name, FieldEvent.Change, result);
            }
            else
            {
                if (id.Origin == Identity)
                {
                    T old_value = await GetCacheField<T>(id, field_name);
                    if (await SetCacheField(id, field_name, field_value))
                    {
                        NList result = NList.New();
                        result.Add(old_value);
                        result.Add(field_value);
                        await CallbackField(id, field_name, FieldEvent.Change, result);
                    }
                }
                else
                {
                    INode node = GrainFactory.GetGrain<INode>(id.Origin);
                    await node.SetField(id, field_name, field_value);
                }
            }
        }

        public async Task<T> GetField<T>(Nuid id, string field_name)
        {
            Entity entity = EntityManager.Get(id);

            if (entity != null)
            {
                Field field = entity.GetField(field_name);
                if (field == null)
                {
                    return default(T);
                }

                return field.Get<T>();
            }
            else
            {
                if (id.Origin == Identity)
                {
                    return await GetCacheField<T>(id, field_name);
                }
                else
                {
                    INode node = GrainFactory.GetGrain<INode>(id.Origin);

                    return await node.GetField<T>(id, field_name);
                }
            }
        }
    }
}
