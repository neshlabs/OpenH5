import { ClientNode } from "../Nesh/Node";
import { Network } from "./Network";

export default class GameClient
{
    static Node: ClientNode;
    static Network: Network;

    constructor()
    {
        GameClient.Node = new ClientNode();
        GameClient.Network = new Network();
    }

    Startup()
    {
        GameClient.Network.Open();
    }
}
