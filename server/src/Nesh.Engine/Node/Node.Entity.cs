using Nesh.Core;
using Nesh.Core.Data;
using Nesh.Core.Utils;
using Nesh.Engine.Utils;
using System.Threading.Tasks;

namespace Nesh.Engine.Node
{
    public partial class Node
    {
        public async Task<bool> Exists(Nuid id)
        {
            Entity entity = EntityManager.Get(id);

            if (entity != null)
            {
                return true;
            }
            else
            {
                if (id.Origin == Identity)
                {
                    return await CacheExist(id);
                }
                else
                {
                    INode node = GrainFactory.GetGrain<INode>(id.Origin);
                    return await node.Exists(id);
                }
            }
        }

        public async Task<string> GetType(Nuid id)
        {
            Entity entity = EntityManager.Get(id);

            if (entity != null)
            {
                return entity.Type;
            }
            else
            {
                if (id.Origin == Identity)
                {
                    return await GetCacheType(id);
                }
                else
                {
                    INode node = GrainFactory.GetGrain<INode>(id.Origin);
                    return await node.GetType(id);
                }
                
            }
        }

        public async Task<Nuid> Create(Nuid id, string type, NList args)
        {
            if (id.Origin == Identity)
            {
                Entity entity = EntityManager.Create(id, type);
                if (entity == null)
                {
                    return Nuid.Empty;
                }

                BatchCahceList.Add(NList.New().Add((int)CacheOption.SetEntity).Add(id).Add(type));

                await CallbackEntity(id, EntityEvent.OnCreate, args);
            }
            else
            {
                INode node = GrainFactory.GetGrain<INode>(id.Origin);
                return await node.Create(id, type, args);
            }

            return id;
        }

        public async Task<Nuid> Create(string type, Nuid origin, NList args)
        {
            long unique = IdUtils.UGen.CreateId();
            Nuid id = Nuid.New(unique, origin.Origin);
            return await Create(id, type, args);
        }

        public async Task Entry(Nuid id)
        {
            Entity entity = EntityManager.Get(id);
            if (entity != null)
            {
                await CallbackEntity(id, EntityEvent.OnEntry, NList.Empty);
            }
            else
            {
                if (id.Origin == Identity) return;

                INode node = GrainFactory.GetGrain<INode>(id.Origin);

                if (await node.IsActive())
                {
                    await node.Entry(id);
                }
            }
        }

        public async Task Load(Entity entity)
        {
            if (entity == null) return;

            if (EntityManager.Find(entity.Id))
            {
                EntityManager.Remove(entity.Id);
            }

            if (entity.Id.Origin == Identity)
            {
                EntityManager.Add(entity);
                await CallbackEntity(entity.Id, EntityEvent.OnEntry, NList.Empty);
            }
            else
            {
                INode node = GrainFactory.GetGrain<INode>(entity.Id.Origin);

                if (await node.IsActive())
                {
                    await node.Load(entity);
                }
            }
        }

        public async Task Leave(Nuid id)
        {
            Entity entity = EntityManager.Get(id);
            if (entity != null)
            {
                await CallbackEntity(id, EntityEvent.OnLeave, NList.Empty);
            }
            else
            {
                if (id.Origin == Identity) return;

                INode node = GrainFactory.GetGrain<INode>(id.Origin);

                if (await node.IsActive())
                {
                    await node.Leave(id);
                }
            }
        }

        public async Task Destroy(Nuid id)
        {
            Entity entity = EntityManager.Get(id);
            if (entity != null)
            {
                await CallbackEntity(id, EntityEvent.OnDestroy, NList.Empty);

                EntityManager.Remove(id);
            }
            else
            {
                if (id.Origin == Identity) return;

                INode node = GrainFactory.GetGrain<INode>(id.Origin);

                if (await node.IsActive())
                {
                    await node.Destroy(id);
                }
            }
        }
    }
}
