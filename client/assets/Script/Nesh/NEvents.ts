import NList, { Nuid } from "./NList";
import { INode } from "./Node";

class NEvent
{
    public Priority: number;
    public Callback: (node: INode, nuid: Nuid, args: NList) => void;
}

export class NEvents<EVENT_ID>
{
    _EventHandlers: Map<EVENT_ID, Array<NEvent>>;

    constructor()
    {
        this._EventHandlers = new Map<EVENT_ID, Array<NEvent>>();
    }

    Callback(event_id: EVENT_ID, node: INode, nuid: Nuid, args: NList) : void
    {
        let found = this._EventHandlers.get(event_id);
        if (found == null) return;

        found.forEach(event => event.Callback(node, nuid, args));
    }

    Register(event_id: EVENT_ID, callback: (node: INode, nuid: Nuid, args: NList) => void, priority = 1)
    {
        let e = new NEvent();
        e.Priority = priority;
        e.Callback = callback;

        if (!this._EventHandlers.has(event_id))
        {
            let found = new Array<NEvent>();
            this._EventHandlers.set(event_id, found);        
        }
        else
        {
            let found = this._EventHandlers.get(event_id);
            found.push(e);
            found.sort((x, y) => x.Priority - y.Priority);
        }
    }
}
