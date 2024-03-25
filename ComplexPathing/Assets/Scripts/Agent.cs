using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The BridgeState enum, used both here and in the bridge class. 
public enum BridgeState{
    onBridgeGoingUp,
    onBridgeGoingDown,
    onBridgeToBottom,
    onBridgeToSide,
    emptyBridge
}

//Agent class, the ones following the paths.
public class Agent 
{
    //First, instantiate all needed fields.
    public GameObject agentObj;
    public List<Node> travelPath;
    public Node curNode;
    public Node destNode;
    public bool onBridge;
    public bool waitingForPath;
    public BridgeState agentBridgeState;
    public int myBridgeIndex;
    public int waitingCounter;
    public int waitingCap;
    public int repathCounter;

    private float spd = 5f;

//Basic constructor for said fields.
    public Agent(GameObject gameObject, Node curNode, Node destNode){
        this.curNode = curNode;
        this.destNode = destNode;
        agentObj = gameObject;
        gameObject.transform.position = curNode.pos;
        travelPath = new List<Node>();
        waitingForPath = true;
        onBridge = false;
        agentBridgeState = BridgeState.emptyBridge;
        myBridgeIndex = -1;
        waitingCounter = 0;
        waitingCap = (int) Random.Range(100, 500);
        repathCounter = 0;
    }

    //This function gets the last node of an Agent's current travelPath. Used to see if going to a bridge/teleporter.
    public   Node lastPathNode(){
        Node result = null;
        for(int i = 0; i < travelPath.Count; i++){
            result = travelPath[i];
        }

        return result;
    }
    
    //Determines if an agent is compatible with the passed-in bridge (Ex: true if bridge is empty or agent on the bridge is heading in same direction)
    public bool isBridgeDirectionCompatible(BridgeState aState){ //only called when we have determined an agent is heading for a bridge.
        if(onBridge){
            return true;
        }else{
            //Either check the lastPathNode manually, and then figure out which it is (lots of else/if statements) or
            if((this.agentBridgeState == aState) || (aState == BridgeState.emptyBridge)){
                return true;
            }else{
                return false;
            }

        }
    }

    //Here we set the Needed state of an agent according to the last node of its travelPath. Only really called when we know the last node is a bridge node.
    public void setNeededState(Node lastNode){
        if(lastNode.gridX == 14 && lastNode.gridY == 4){ 
            agentBridgeState = BridgeState.onBridgeGoingDown;
        }else if(lastNode.gridX == 14 && lastNode.gridY == 12){
            agentBridgeState = BridgeState.onBridgeToSide;
        }else if(lastNode.gridX == 14 && lastNode.gridY == 20){
            agentBridgeState = BridgeState.onBridgeGoingDown;
        }else if(lastNode.gridX == 0 && lastNode.gridY == 4){
            agentBridgeState = BridgeState.onBridgeGoingUp;
        }else if(lastNode.gridX == 0 && lastNode.gridY == 12){
            agentBridgeState = BridgeState.onBridgeToBottom;
        }else if(lastNode.gridX == 0 && lastNode.gridY == 20){
            agentBridgeState = BridgeState.onBridgeGoingUp;
        }
    }

    //This function is repeatedly called in PathfindingManager's fixedUpdate. Moves our agent or sets/increments some of its fields depending on the situation.
    public void travel(){
        //Check to see if we've made it to a BridgeNode with intent to cross.
        if(travelPath.Count == 0 && curNode is BridgeNode && (curNode.myGrid != destNode.myGrid)){
            //In this case, we need to "transfer" the agent to the grid on the other end of the bridge.
            curNode = ((BridgeNode) curNode).getConnector();
            if(curNode.gridX == 0 && curNode.gridY == 4){ 
                PathfindingManager.instance.changeBridgeState(1, BridgeState.onBridgeGoingDown);
                this.myBridgeIndex = 1;
                agentBridgeState = BridgeState.onBridgeGoingDown;
            }else if(curNode.gridX == 0 && curNode.gridY == 12){
                PathfindingManager.instance.changeBridgeState(0, BridgeState.onBridgeToSide);
                this.myBridgeIndex = 0;
                agentBridgeState = BridgeState.onBridgeToSide;
            }else if(curNode.gridX == 0 && curNode.gridY == 20){
                PathfindingManager.instance.changeBridgeState(2, BridgeState.onBridgeGoingDown);
                this.myBridgeIndex = 2;
                agentBridgeState = BridgeState.onBridgeGoingDown;
            }else if(curNode.gridX == 14 && curNode.gridY == 4){
                PathfindingManager.instance.changeBridgeState(1, BridgeState.onBridgeGoingUp);
                this.myBridgeIndex = 1;
                agentBridgeState = BridgeState.onBridgeGoingUp;
            }else if(curNode.gridX == 14 && curNode.gridY == 12){
                PathfindingManager.instance.changeBridgeState(0, BridgeState.onBridgeToBottom);
                this.myBridgeIndex = 0;
                agentBridgeState = BridgeState.onBridgeToBottom;
            }else if(curNode.gridX == 14 && curNode.gridY == 20){
                PathfindingManager.instance.changeBridgeState(2, BridgeState.onBridgeGoingUp);
                this.myBridgeIndex = 2;
                agentBridgeState = BridgeState.onBridgeGoingUp;
            }
            waitingForPath = true;
            onBridge = true;

        //Check to see if we're on a teleporterNode, and set it as occupied if so.
        }else if(travelPath.Count == 0 && curNode is TeleporterNode && ((TeleporterNode) curNode).isWaitNode){
            TeleporterNode wNode = (TeleporterNode) curNode;
            wNode.setOccupant(this);

        }else if(travelPath.Count == 0 && curNode is TeleporterNode && ! ((TeleporterNode) curNode).isWaitNode){
            TeleporterNode tNode = (TeleporterNode) curNode;
            tNode.setOccupant(this);
        
        //The case most commonly called.
        }else if(! (travelPath.Count == 0)){
            waitingCounter = 0; //If we're here, then we're on the move, so we reset the counter.
            Node nextNode = travelPath[0];
            //Move slightly towards the next node in our path.
            if(agentObj.transform.position != nextNode.pos){
                Vector3 curPos = Vector3.MoveTowards(agentObj.transform.position, nextNode.pos, spd *Time.deltaTime);
                agentObj.GetComponent<Rigidbody>().MovePosition(curPos);
                //If we're on the next node, we set curNode to nextNode in the travelPath, and shorten the travelPath.
            }else{
                //In this special case, we have crossed a bridge and need to "broadcast" that we have done so.
                if((curNode is BridgeNode) && (! (nextNode is BridgeNode)) && (onBridge == true) && (myBridgeIndex != -1)){
                    onBridge = false;
                    agentBridgeState = BridgeState.emptyBridge;
                    PathfindingManager.instance.changeBridgeState(this.myBridgeIndex, BridgeState.emptyBridge);
                    this.myBridgeIndex = -1;
                }
                curNode = travelPath[0];
                travelPath.RemoveAt(0);
            }
        }else{
            waitingCounter++; //If we get here, we're done our path or waiting on a blockage, so increment the wait counter.
        }

    }
}
