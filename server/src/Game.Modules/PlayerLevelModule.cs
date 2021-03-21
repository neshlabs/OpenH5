using Game.Resources.Entity;
using Game.Resources.Msg;
using Nesh.Core;
using Nesh.Core.Data;
using Nesh.Core.Utils;
using System.Threading.Tasks;

namespace Game.Module
{
    class PlayerLevelModule : NModule
    {
        protected override void OnInit()
        {
            RCField("player", "level", FieldEvent.Change, OnLevelChange);
            RCEntity("player", EntityEvent.OnCreate, OnPlayerCreate);

            RCCommand(CommandMsg.TEST_CMD, OnCommand_Test);
        }

        private static async Task OnCommand_Test(INode node, Nuid id, INList args)
        {
            Nuid target_id = args.Get<Nuid>(0);

            int level = await node.GetField<int>(id, "level");
            level++;
            await node.SetField(target_id, "level", level);
        }

        private static async Task OnPlayerCreate(INode node, Nuid id, INList args)
        {
            await node.SetField(id, "level", 1100);

            await node.SetField(id, "nick_name", "1sadasdasd");

            await node.AddHeartbeat(id, "test", 10000, 10, OnHeartbeat);

            await node.AddRow(id, Player.Tables.QuestTable.TABLE_NAME, NList.New().Add(1001).Add(1).Add(TimeUtils.Now));

            await node.AddRow(id, Player.Tables.QuestTable.TABLE_NAME, NList.New().Add(2002).Add(2).Add(TimeUtils.Now));

            await node.Create("item", id, NList.New());
        }

        private static async Task OnHeartbeat(INode node, Nuid id, INList args)
        {
            string timer_name = args.Get<string>(0);
            long now_ticks = args.Get<long>(1);
            int RemainBeatCount = args.Get<int>(2);

            await node.Error(string.Format("{0} beat {1}", timer_name, RemainBeatCount));
        }

        private static Task OnCountdown(INode node, Nuid id, INList args)
        {
            return Task.CompletedTask;
        }

        private static async Task OnLevelChange(INode node, Nuid id, INList args)
        {
            int level = await node.GetField<int>(id, "level");
        }
    }
}
