using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Nesh.Core;
using Nesh.Core.Auth;
using Nesh.Core.Data;
using Nesh.Core.Manager;
using Nesh.Core.Utils;
using Orleans;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nesh.Engine.Node
{
    public partial class Node : Grain, INode
    {
        private IAgent Agent { get; set; }

        private long Identity { get; set; }

        private bool Activated { get; set; }

        private ILogger _Logger { get; set; }

        private IDisposable _TimerObj { get; set; }

        private TimerManager TimerManager { get; set; }

        private EntityManager EntityManager { get; set; }

        private EventManager<SyncType> SyncManager { get; set; }

        private List<NList> BatchCahceList { get; set; }

        private IRedisCacheClient _CacheClient { get; set; }

        public Task<bool> IsActive()
        {
            return Task.FromResult(Activated);
        }

        public override async Task OnDeactivateAsync()
        {
            await Persist();
        }

        public async Task Active()
        {
            await PullPersistFields("player");
        }

        public async Task Deactive()
        {
            await Persist();
        }

        public Task BindAgent(IAgent agent)
        {
            Agent = agent;

            return Task.CompletedTask;
        }

        IMongoClient _IMongoClient;
        public override Task OnActivateAsync()
        {
            Identity = this.GetPrimaryKeyLong();
            _Logger = ServiceProvider.GetService<ILoggerFactory>().CreateLogger("Node[" + Identity + "]");
            _CacheClient = ServiceProvider.GetService<IRedisCacheClient>();
            _IMongoClient = ServiceProvider.GetService<IMongoClient>();

            EntityManager = new EntityManager();
            SyncManager = new EventManager<SyncType>();
            TimerManager = new TimerManager(this);

            BatchCahceList = new List<NList>();

            SyncManager.Register(SyncType.Entity, OnSyncEntity);
            SyncManager.Register(SyncType.Field, OnSyncField);
            SyncManager.Register(SyncType.Table, OnSyncTable);

            RegisterTimer(BatchCache, null, TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(TimeUtils.MINITE));

            return base.OnActivateAsync();
        }

        private Task OnSyncTable(INode node, Nuid id, INList args)
        {
            return Task.CompletedTask;
        }

        private Task OnSyncField(INode node, Nuid id, INList args)
        {
            return Task.CompletedTask;
        }

        private Task OnSyncEntity(INode node, Nuid id, INList args)
        {
            return Task.CompletedTask;
        }

        private async Task CallbackEntity(Nuid id, EntityEvent entity_event, NList args)
        {
            string entity_type = Global.NULL_STRING;
            Entity entity = EntityManager.Get(id);
            if (entity != null)
            {
                entity_type = entity.Type;
            }
            else
            {
                entity_type = await GetCacheType(id);
            }

            EntityPrefab entity_prefab = Prefabs.GetEntity(entity_type);
            if (entity_prefab == null)
            {
                return;
            }

            if (entity_prefab.ancestors != null && entity_prefab.ancestors.Count > 0)
            {
                for (int i = entity_prefab.ancestors.Count - 1; i >= 0; i--)
                {
                    string parent_type = entity_prefab.ancestors[i];

                    await NModule.CallbackEntity(this, id, parent_type, entity_event, args);
                }
            }

            await NModule.CallbackEntity(this, id, entity_type, entity_event, args);

            NList msg = NList.New().Add(id).Add((int)entity_event).Append(args);
            await SyncManager.Callback(SyncType.Entity, this, id, msg);
        }

        private async Task CallbackField(Nuid id, string field_name, FieldEvent field_event, NList args)
        {
            string entity_type = Global.NULL_STRING;
            Entity entity = EntityManager.Get(id);
            if (entity != null)
            {
                entity_type = entity.Type;
            }
            else
            {
                entity_type = await GetCacheType(id);
            }

            EntityPrefab entity_prefab = Prefabs.GetEntity(entity_type);
            if (entity_prefab == null)
            {
                return;
            }

            if (entity_prefab.ancestors != null && entity_prefab.ancestors.Count > 0)
            {
                for (int i = entity_prefab.ancestors.Count - 1; i >= 0; i--)
                {
                    string parent_type = entity_prefab.ancestors[i];

                    await NModule.CallbackField(this, id, parent_type, field_name, field_event, args);
                }
            }

            await NModule.CallbackField(this, id, entity_type, field_name, field_event, args);

            FieldPrefab field_prefab = entity_prefab.fields[field_name];
            if (field_prefab != null && field_prefab.sync)
            {
                NList msg = NList.New().Add(id).Add(field_name).Add((int)field_event).Append(args);
                await SyncManager.Callback(SyncType.Field, this, id, msg);
            }
        }

        private async Task CallbackTable(Nuid id, string table_name, TableEvent table_event, NList args)
        {
            string entity_type = Global.NULL_STRING;
            Entity entity = EntityManager.Get(id);
            if (entity != null)
            {
                entity_type = entity.Type;
            }
            else
            {
                entity_type = await GetCacheType(id);
            }

            EntityPrefab entity_prefab = Prefabs.GetEntity(entity_type);
            if (entity_prefab == null)
            {
                return;
            }

            if (entity_prefab.ancestors != null && entity_prefab.ancestors.Count > 0)
            {
                for (int i = entity_prefab.ancestors.Count - 1; i >= 0; i--)
                {
                    string parent_type = entity_prefab.ancestors[i];

                    await NModule.CallbackTable(this, id, parent_type, table_name, table_event, args);
                }
            }

            await NModule.CallbackTable(this, id, entity_type, table_name, table_event, args);

            TablePrefab table_prefab = entity_prefab.tables[table_name];
            if (table_prefab != null && table_prefab.sync)
            {
                NList msg = NList.New().Add(id).Add(table_name).Add((int)table_event).Append(args);
                await SyncManager.Callback(SyncType.Table, this, id, msg);
            }
        }

        public async Task Command(Nuid id, int command, NList msg)
        {
            Entity entity = EntityManager.Get(id);
            if (entity != null)
            {
                await NModule.CallbackCommand(this, id, command, msg);
            }
            else
            {
                if (id.Origin == Identity)
                {
                    await NModule.CallbackCommand(this, id, command, msg);
                }
                else
                {
                    INode node = GrainFactory.GetGrain<INode>(id.Origin);
                    await node.Command(id, command, msg);
                }
            }
        }

        public async Task Custom(Nuid id, int custom, NList msg)
        {
            Entity entity = EntityManager.Get(id);
            if (entity != null)
            {
                await NModule.CallbackCustom(this, id, custom, msg);
                await SyncManager.Callback(SyncType.Custom, this, id, NList.New().Add(custom).Append(msg));
            }
            else
            {
                if (id.Origin == Identity) return;

                INode node = GrainFactory.GetGrain<INode>(id.Origin);

                if (await node.IsActive())
                {
                    await node.Custom(id, custom, msg);
                }
            }
        }

        public Task<NList> GetEntities()
        {
            NList list = NList.New();
            IReadOnlyList<Entity> entities = EntityManager.GetEntities();
            foreach (Entity entity in entities)
            {
                list.Add(entity);
            }

            return Task.FromResult(list);
        }
    }
}
