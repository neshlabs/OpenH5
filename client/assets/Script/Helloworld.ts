import { Player } from "./Game/Entity/Player";
import GameClient from "./Game/GameClient";

const {ccclass, property} = cc._decorator;

@ccclass
export default class Helloworld extends cc.Component {

    @property(cc.Label)
    label: cc.Label = null;

    @property
    text: string = 'hello';

    client: GameClient;

    start () {
        console.log(Player.TYPE);
        console.log(Player.Fields.NICK_NAME);
        // init logic
        this.label.string = this.text;

        this.client = new GameClient();
        this.client.Startup();

        var request = cc.loader.getXMLHttpRequest();
        var url = "http://localhost:5001/api/auth/sim_login?user_name=test1";
        let xhr = new XMLHttpRequest();
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && (xhr.status >= 200 && xhr.status < 400)) {
                var response = xhr.responseText;
                console.log(response);
            }
        };
        xhr.open("POST", url, true);
        xhr.send();
    }
}
