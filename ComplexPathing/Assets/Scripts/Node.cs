using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class representing the nodes on our Grid.
public class Node
{
    //Set the needed fields.
    public int gridX;
    public int gridY;
    public bool isObstacle;
    public Vector3 pos;
    public Node parNode; 
    public CustomGrid myGrid;
    
    public int gCost;
    public int hCost;
    public int fCost {get {return gCost + hCost;}}

    //Basic constructor.
    public Node(int gridX, int gridY, bool isObstacle, Vector3 pos, CustomGrid myGrid){
        this.gridX = gridX;
        this.gridY = gridY;
        this.isObstacle = isObstacle;
        this.pos = pos;
        this.myGrid = myGrid;

        gCost = 1000;
        hCost = 1000;
    }
}

//One of the two subtypes of Node. This means it's one of two nodes on the ends of a bridge, and so it has a "connector" attribute which has the node on the other grid.
public class BridgeNode : Node{
    public BridgeNode connector;

    public BridgeNode(int gridX, int gridY, bool isObstacle, Vector3 pos, CustomGrid myGrid): base(gridX, gridY, isObstacle, pos, myGrid){
        connector = null;
    }

    //Also has setter/getter functions for the connector node.
    public void setConnector(BridgeNode aConnector){
        connector = aConnector;
    }

    public BridgeNode getConnector(){
        return connector;
    }
}

//The second subtype of Node, this one means it is one of the six nodes of a given Teleporter. Split into "waitNodes" and "teleporterNodes" as described in my solution (see pdf)
public class TeleporterNode : Node{
    private TeleporterNode connector; //if TeleporterNode is a WaitNode, this will always be null and won't be used.
    public bool isWaitNode;
    public bool isReserved;
    private Agent occupant;

    //Basic constructor and get/set/check functions for this type of Node.
    public TeleporterNode(int gridX, int gridY, bool isObstacle, Vector3 pos, CustomGrid myGrid, bool isWaitNode): base(gridX, gridY, isObstacle, pos, myGrid){
        this.connector = null;
        this.occupant = null;
        this.isWaitNode = isWaitNode;
        this.isReserved = false;
    }

    public void setConnector(TeleporterNode aConnector){
        connector = aConnector;
    }

    public TeleporterNode getConnector(){
        return connector;
    }

    public bool isOccupied(){
        return (occupant != null);
    }

    public void setOccupant(Agent aAgent){
        occupant = aAgent;
    }

    public Agent getOccupant(){
        return occupant;
    }
}
