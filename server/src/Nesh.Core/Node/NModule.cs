using Nesh.Core.Data;
using Nesh.Core.Manager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nesh.Core
{
    [Serializable]
    public delegate Task EventCallback(INode node, Nuid id, INList args);

    public abstract class NModule
    {
        private static EventManager<Tuple<string, string, FieldEvent>> _FieldEvents = new EventManager<Tuple<string, string, FieldEvent>>();
        private static EventManager<Tuple<string, string, TableEvent>> _TableEvents = new EventManager<Tuple<string, string, TableEvent>>();
        private static EventManager<Tuple<string, EntityEvent>> _EntityEvents = new EventManager<Tuple<string, EntityEvent>>();
        private static EventManager<int> _CustomManager = new EventManager<int>();
        private static EventManager<int> _CommandManager = new EventManager<int>();

        private static Dictionary<string, NModule> _Modules = new Dictionary<string, NModule>();

        public static void LoadModule(Type type)
        {
            string name = type.FullName;
            NModule module = Activator.CreateInstance(type) as NModule;

            _Modules.Add(name, module);
            module.OnInit();
        }

        protected abstract void OnInit();

        protected void RCEntity(string entity_type, EntityEvent entity_event, EventCallback handler, int priority = 1)
        {
            _EntityEvents.Register(new Tuple<string, EntityEvent>(entity_type, entity_event), handler, priority);
        }

        protected void RCField(string entity_type, string field_name, FieldEvent field_event, EventCallback handler, int priority = 1)
        {
            _FieldEvents.Register(new Tuple<string, string, FieldEvent>(entity_type, field_name, field_event), handler, priority);
        }

        protected void RCTable(string entity_type, string table_name, TableEvent table_event, EventCallback handler, int priority = 1)
        {
            _TableEvents.Register(new Tuple<string, string, TableEvent>(entity_type, table_name, table_event), handler, priority);
        }

        protected void RCCustom(int custom_msg, EventCallback handler, int priority = 1)
        {
            _CustomManager.Register(custom_msg, handler, priority);
        }

        protected void RCCommand(int command_msg, EventCallback handler, int priority = 1)
        {
            _CommandManager.Register(command_msg, handler, priority);
        }

        public static async Task CallbackEntity(INode node, Nuid id, string entity_type, EntityEvent entity_event, NList args)
        {
            await _EntityEvents.Callback(new Tuple<string, EntityEvent>(entity_type, entity_event), node, id, args);
        }

        public static async Task CallbackField(INode node, Nuid id, string entity_type, string field_name, FieldEvent field_event, NList args)
        {
            await _FieldEvents.Callback(new Tuple<string, string, FieldEvent>(entity_type, field_name, field_event), node, id, args);
        }

        public static async Task CallbackTable(INode node, Nuid id, string entity_type, string table_name, TableEvent table_event, NList args)
        {
            await _TableEvents.Callback(new Tuple<string, string, TableEvent>(entity_type, table_name, table_event), node, id, args);
        }

        public static async Task CallbackCustom(INode node, Nuid id, int custom_msg, NList args)
        {
            await _CustomManager.Callback(custom_msg, node, id, args);
        }

        public static async Task CallbackCommand(INode node, Nuid id, int command_msg, NList args)
        {
            await _CommandManager.Callback(command_msg, node, id, args);
        }
    }
}