import NList, { Entity, Nuid } from "./Nesh/NList";
import { ClientNode, EntityEvent } from "./Nesh/Node";

const {ccclass, property} = cc._decorator;

@ccclass
export default class Helloworld extends cc.Component {

    @property(cc.Label)
    label: cc.Label = null;

    @property
    text: string = 'hello';

    start () {
        // init logic
        this.label.string = this.text;

        let ws = new WebSocket("ws://127.0.0.1:8080/websocket");
        ws.onopen = function (event) {
            console.log("Send Text WS was opened.");
            /*const lst = NList.New();
            lst.AddBool(true).AddInt(1000).AddLong(20000000).AddFloat(0.0005).AddString("stringgggg").AddTime(new Date());
            const lst2 = NList.New();
            lst2.AddBool(false).AddInt(200).AddLong(40000000).AddFloat(1.0005).AddString("gfadasdasdasdasd").AddTime(new Date());
            lst2.AddList(lst);

            const json = NList.AsJSON(lst2);

            console.log("NList.AsJSON: json = " + json);

            let testlst = NList.AsList(json);

            const cjson = NList.AsJSON(testlst);

            console.log("NList.AsJSON: cjson= " + cjson);*/
            
            ws.send("hello nesh");
        };
        ws.onmessage = function (event) {
            console.log("response text msg: " + event.data);

            let lst = NList.AsList(event.data);

            let json = NList.AsJSON(lst);

            console.log("response json msg: " + json);

            lst._List.forEach(element => {
                let entity = element.Value;
                let client_node = new ClientNode();
                client_node.LoadEntity(entity);

                let id = new Nuid();
                id.Origin = 12058624;
                id.Unique = 12058624;

                let level:number = client_node.GetField(id, "level");

                console.log("client_node.GetField: " + level);

                client_node.RCEntity("player", EntityEvent.OnCreate, ()=>
                {
                    console.log("player EntityEvent.OnCreate");
                });

                client_node.CallbackEntity("player", EntityEvent.OnCreate, null, null);
            });
        };
        ws.onerror = function (event) {
            console.log("Send Text fired an error");
        };
        ws.onclose = function (event) {
            console.log("WebSocket instance closed.");
        };
    }
}
