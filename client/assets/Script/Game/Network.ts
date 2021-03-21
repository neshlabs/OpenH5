import NList from "../Nesh/NList";
import { NPrefabs } from "../Nesh/NPrefabs";
import { Equip } from "./Entity/Equip";
import { Player } from "./Entity/Player";
import { SystemMsg } from "./Message/SystemMsg";
import { DefStringProtocol, INetworkTips, NetData } from "./Network/NetInterface";
import { NetManager } from "./Network/NetManager";
import { NetNode } from "./Network/NetNode";
import { WebSock } from "./Network/WebSock";

class NetTips implements INetworkTips {

    connectTips(isShow: boolean): void {
        console.debug("connectTips " + isShow);
        NetManager.getInstance().send("hello nesh");
    }

    reconnectTips(isShow: boolean): void {
        console.debug("reconnectTips " + isShow);
    }

    requestTips(isShow: boolean): void {
        console.debug("requestTips " + isShow);
    }
}

export class Network
{
    _DispatchMsgs: Map<number, (args: NList) => void>

    constructor()
    {
        this._DispatchMsgs = new Map<number, (args: NList) => void>();
        this.Regiser(SystemMsg.SERVER.ACCESS_TOKEN, this.OnAccessToken);

        let node = new NetNode();
        node.init(new WebSock(), new DefStringProtocol(), new NetTips());
        node.setResponeHandler(0, (cmd: number, data: NetData) => {
            let json = data.toString();
            let lst:NList = NList.AsList(json);

            console.log("response json msg: " + json);
    
            let message_id: number = lst.GetInt(0);
            let message: NList = lst.GetRange(1, lst._List.length-1);
    
            this.Callback(message_id, message);
        });
        NetManager.getInstance().setNetNode(node);
    }

    Open()
    {
        NetManager.getInstance().connect({url: "ws://127.0.0.1:8080/websocket"});
    }

    OnAccessToken(msg: NList)
    {
        /*const json = NList.AsJSON(msg);

        console.debug("OnAccessToken old json:" + json);
        let prefabs: NList= NList.AsList(json);
        const njson = NList.AsJSON(prefabs);
        console.debug("OnAccessToken new json:" + njson);*/

        NPrefabs.AsPrefabs(msg.GetString(0));
    }

    OnError(e: any)
    {
        console.log("Send Text fired an error");
    }

    Regiser(message_id: number, callfunc: (args: NList) => void)
    {
        this._DispatchMsgs.set(message_id, callfunc);
    }

    Callback(message_id: number, message: NList)
    {
        this._DispatchMsgs.get(message_id)(message);
    }
}
