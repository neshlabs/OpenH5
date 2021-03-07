using Nesh.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nesh.Core.Manager
{
    public class EntityManager
    {
        private Dictionary<Nuid, Entity> _EntityDic;

        private Entity _CurEntity;

        public EntityManager()
        {
            _EntityDic = new Dictionary<Nuid, Entity>();
            _CurEntity = null;
        }

        public Entity Get(Nuid id)
        {
            if (_CurEntity != null && _CurEntity.Id == id)
            {
                return _CurEntity;
            }

            _EntityDic.TryGetValue(id, out _CurEntity);
            return _CurEntity;
        }

        public void Add(Entity entity)
        {
            if (entity == null || _EntityDic.ContainsKey(entity.Id))
            {
                return;
            }

            _EntityDic.Add(entity.Id, entity);
        }

        public bool Find(Nuid id)
        {
            if (_CurEntity != null && _CurEntity.Id == id)
            {
                return true;
            }

            return _EntityDic.ContainsKey(id);
        }

        private Entity Gen(string entity_type)
        {
            entity_def entity_def = DefineManager.GetEntity(entity_type);
            if (entity_def == null)
            {
                return null;
            }

            Entity new_entity = new Entity();
            new_entity.Type = entity_def.name;

            foreach (field_def field_def in entity_def.all_fields)
            {
                switch (field_def.type)
                {
                    case VarType.Bool:
                        new_entity.CreateField(field_def.name, Global.NULL_BOOL);
                        break;
                    case VarType.Int:
                        new_entity.CreateField(field_def.name, Global.NULL_INT);
                        break;
                    case VarType.Float:
                        new_entity.CreateField(field_def.name, Global.NULL_FLOAT);
                        break;
                    case VarType.Long:
                        new_entity.CreateField(field_def.name, Global.NULL_LONG);
                        break;
                    case VarType.String:
                        new_entity.CreateField(field_def.name, Global.NULL_STRING);
                        break;
                    case VarType.Nuid:
                        new_entity.CreateField(field_def.name, Nuid.Empty);
                        break;
                    case VarType.Time:
                        new_entity.CreateField(field_def.name, DateTime.MinValue);
                        break;
                    case VarType.List:
                        new_entity.CreateField(field_def.name, NList.Empty);
                        break;
                    default:
                        break;
                }
            }

            foreach (table_def table_def in entity_def.all_tables)
            {
                new_entity.CreateTable(table_def.name);
            }

            return new_entity;
        }

        public Entity Create(Nuid id, string entity_type)
        {
            if (Nuid.IsEmpty(id))
            {
                return null;
            }

            if (_EntityDic.ContainsKey(id))
            {
                return null;
            }

            Entity new_entity = Gen(entity_type);
            if (new_entity == null)
            {
                return null;
            }

            _EntityDic.Add(id, new_entity);
            new_entity.Id = id;

            return new_entity;
        }

        public bool Remove(Nuid id)
        {
            Entity found = null;
            if (_EntityDic.TryGetValue(id, out found))
            {
                found.Clear();
            }

            if (_CurEntity != null && _CurEntity.Id == id)
            {
                _CurEntity = null;
            }

            return _EntityDic.Remove(id);
        }

        public IReadOnlyList<Entity> GetEntities()
        {
            List<Entity> list = _EntityDic.Values.ToList();
            list.Sort((x, y) =>
            {
                entity_def entity_x = DefineManager.GetEntity(x.Type);
                entity_def entity_y = DefineManager.GetEntity(y.Type);

                if (entity_x.priority > entity_y.priority)
                    return -1;
                else if (entity_x.priority == entity_y.priority)
                    return 0;
                else
                    return 1;
            });

            return list;
        }

        public void Clear()
        {
            foreach (Entity co in _EntityDic.Values)
            {
                co.Clear();
            }

            _EntityDic.Clear();
            _CurEntity = null;
        }
    }
}
