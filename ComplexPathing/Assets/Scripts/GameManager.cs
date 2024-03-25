using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    //The Coroutine which handles the teleporters. It takes the below two teleporters and goes through the 3 states as defined in the attached PDF,
    //And on each state change it executes that state's specific action.
    IEnumerator teleportRoutine(){
    
    bool bottomTeleporters =  true;

    while(true){
        if(bottomTeleporters){
            teleporters[0].curState = TeleporterState.Empty;
            teleporters[1].curState = TeleporterState.Empty;
            teleporters[0].executeStateAction();
            teleporters[1].executeStateAction();
            yield return new WaitForSeconds(2f);

            teleporters[0].curState = TeleporterState.Enter;
            teleporters[1].curState = TeleporterState.Enter;
            teleporters[0].executeStateAction();
            teleporters[1].executeStateAction();
            for(int i = 0; i < 3; i++){
                teleporters[0].waitNodes[i].setOccupant(null);
                teleporters[0].waitNodes[i].isReserved = false;
                teleporters[1].waitNodes[i].setOccupant(null);
                teleporters[1].waitNodes[i].isReserved = false;
            }
            yield return new WaitForSeconds(2f);

            teleporters[0].curState = TeleporterState.Teleport;
            teleporters[1].curState = TeleporterState.Teleport;
            teleporters[0].executeStateAction();
            teleporters[1].executeStateAction();
            yield return new WaitForSeconds(1f);

            bottomTeleporters = false;
            //Change color of bottom teleport platforms to red, and others to green.
            teleportPlatforms[0].GetComponent<MeshRenderer>().material = teleportMaterials[1];
            teleportPlatforms[1].GetComponent<MeshRenderer>().material = teleportMaterials[1];
            teleportPlatforms[2].GetComponent<MeshRenderer>().material = teleportMaterials[0];
            teleportPlatforms[3].GetComponent<MeshRenderer>().material = teleportMaterials[0];
        }else{
            teleporters[2].curState = TeleporterState.Empty;
            teleporters[3].curState = TeleporterState.Empty;
            teleporters[2].executeStateAction();
            teleporters[3].executeStateAction();
            yield return new WaitForSeconds(2f);

            teleporters[2].curState = TeleporterState.Enter;
            teleporters[3].curState = TeleporterState.Enter;
            teleporters[2].executeStateAction();
            teleporters[3].executeStateAction();
            for(int i = 0; i < 3; i++){
                teleporters[2].waitNodes[i].setOccupant(null);
                teleporters[2].waitNodes[i].isReserved = false;
                teleporters[3].waitNodes[i].setOccupant(null);
                teleporters[3].waitNodes[i].isReserved = false;
            }
            yield return new WaitForSeconds(2f);

            teleporters[2].curState = TeleporterState.Teleport;
            teleporters[3].curState = TeleporterState.Teleport;
            teleporters[2].executeStateAction();
            teleporters[3].executeStateAction();
            yield return new WaitForSeconds(1f);
            
            bottomTeleporters = true;
            //Change color of bottom teleport platforms to green, and others to red.
            teleportPlatforms[0].GetComponent<MeshRenderer>().material = teleportMaterials[0];
            teleportPlatforms[1].GetComponent<MeshRenderer>().material = teleportMaterials[0];
            teleportPlatforms[2].GetComponent<MeshRenderer>().material = teleportMaterials[1];
            teleportPlatforms[3].GetComponent<MeshRenderer>().material = teleportMaterials[1];
        }

    }
}
    //This script will run first, defining three grids (one for each platform), the teleporters, and the bridge connections.
    //Then, it will randomly place obstacles (and adjust the grid accordingly) and finally it will place NPCs randomly.
    //When all this is done, it will "wake up" the Pathfinding manager.


    public List<CustomGrid> grids; //Grid 0 is for the side, 1 is the bottom and 2 is the top.
    public List<Teleporter> teleporters = new List<Teleporter>();
    public int numAgents = 1;
    public GameObject agentPrefab;
    public List<GameObject> teleportPlatforms;
    public List<Material> teleportMaterials;
    
    //Un-Comment this  and line 99 if we want to have seeded tests.
    //public int seed;

    // Start is called before the first frame update
    void Start()
    {
        //Random.seed = seed;

        //First, get our grids.
        grids.Add(GameObject.Find("SideGrid").GetComponent<CustomGrid>());
        grids.Add(GameObject.Find("BottomGrid").GetComponent<CustomGrid>());
        grids.Add(GameObject.Find("TopGrid").GetComponent<CustomGrid>());

        /*if(numAgents >= 350){ //Add a cap to number of agents (though it would break down far before this point)
            numAgents = 300;
        }*/

        //Instantiate our grids.
        foreach(CustomGrid grid in grids){
            grid.instantiateGrid();
        }

        //Now we connect our bridge nodes to each other
        ((BridgeNode) grids[0].getNode(0,4)).setConnector((BridgeNode) grids[2].getNode(14,4));
        ((BridgeNode) grids[2].getNode(14,4)).setConnector((BridgeNode) grids[0].getNode(0,4));
        ((BridgeNode) grids[0].getNode(0,12)).setConnector((BridgeNode) grids[1].getNode(14,12));
        ((BridgeNode) grids[1].getNode(14,12)).setConnector((BridgeNode) grids[0].getNode(0,12));
        ((BridgeNode) grids[0].getNode(0,20)).setConnector((BridgeNode) grids[2].getNode(14,20));
        ((BridgeNode) grids[2].getNode(14,20)).setConnector((BridgeNode) grids[0].getNode(0,20));

        //Similarly, we connect our TeleporterNodes to each other. 
        ((TeleporterNode) grids[1].getNode(2,2)).setConnector((TeleporterNode) grids[2].getNode(2,2));
        ((TeleporterNode) grids[2].getNode(2,2)).setConnector((TeleporterNode) grids[1].getNode(2,2));
        ((TeleporterNode) grids[1].getNode(2,3)).setConnector((TeleporterNode) grids[2].getNode(2,3));
        ((TeleporterNode) grids[2].getNode(2,3)).setConnector((TeleporterNode) grids[1].getNode(2,3));
        ((TeleporterNode) grids[1].getNode(2,4)).setConnector((TeleporterNode) grids[2].getNode(2,4));
        ((TeleporterNode) grids[2].getNode(2,4)).setConnector((TeleporterNode) grids[1].getNode(2,4));

        ((TeleporterNode) grids[1].getNode(2,20)).setConnector((TeleporterNode) grids[2].getNode(2,20));
        ((TeleporterNode) grids[2].getNode(2,20)).setConnector((TeleporterNode) grids[1].getNode(2,20));
        ((TeleporterNode) grids[1].getNode(2,21)).setConnector((TeleporterNode) grids[2].getNode(2,21));
        ((TeleporterNode) grids[2].getNode(2,21)).setConnector((TeleporterNode) grids[1].getNode(2,21));
        ((TeleporterNode) grids[1].getNode(2,22)).setConnector((TeleporterNode) grids[2].getNode(2,22));
        ((TeleporterNode) grids[2].getNode(2,22)).setConnector((TeleporterNode) grids[1].getNode(2,22));

        //Now, we create the four teleporter objects, and add them to the list. We'll have the two bottom teleporters take 0, 1 and the two top with 2, 3
        List<TeleporterNode> teleporter1Nodes = new List<TeleporterNode>();
        List<TeleporterNode> teleporter2Nodes = new List<TeleporterNode>();
        List<TeleporterNode> teleporter3Nodes = new List<TeleporterNode>();
        List<TeleporterNode> teleporter4Nodes = new List<TeleporterNode>();
        teleporter1Nodes.Add((TeleporterNode) grids[1].getNode(2,2));
        teleporter1Nodes.Add((TeleporterNode) grids[1].getNode(2,3));
        teleporter1Nodes.Add((TeleporterNode) grids[1].getNode(2,4));
        teleporter1Nodes.Add((TeleporterNode) grids[1].getNode(3,2));
        teleporter1Nodes.Add((TeleporterNode) grids[1].getNode(3,3));
        teleporter1Nodes.Add((TeleporterNode) grids[1].getNode(3,4));

        teleporter2Nodes.Add((TeleporterNode) grids[1].getNode(2,20));
        teleporter2Nodes.Add((TeleporterNode) grids[1].getNode(2,21));
        teleporter2Nodes.Add((TeleporterNode) grids[1].getNode(2,22));
        teleporter2Nodes.Add((TeleporterNode) grids[1].getNode(3,20));
        teleporter2Nodes.Add((TeleporterNode) grids[1].getNode(3,21));
        teleporter2Nodes.Add((TeleporterNode) grids[1].getNode(3,22));

        teleporter3Nodes.Add((TeleporterNode) grids[2].getNode(2,2));
        teleporter3Nodes.Add((TeleporterNode) grids[2].getNode(2,3));
        teleporter3Nodes.Add((TeleporterNode) grids[2].getNode(2,4));
        teleporter3Nodes.Add((TeleporterNode) grids[2].getNode(3,2));
        teleporter3Nodes.Add((TeleporterNode) grids[2].getNode(3,3));
        teleporter3Nodes.Add((TeleporterNode) grids[2].getNode(3,4));

        teleporter4Nodes.Add((TeleporterNode) grids[2].getNode(2,20));
        teleporter4Nodes.Add((TeleporterNode) grids[2].getNode(2,21));
        teleporter4Nodes.Add((TeleporterNode) grids[2].getNode(2,22));
        teleporter4Nodes.Add((TeleporterNode) grids[2].getNode(3,20));
        teleporter4Nodes.Add((TeleporterNode) grids[2].getNode(3,21));
        teleporter4Nodes.Add((TeleporterNode) grids[2].getNode(3,22));

        teleporters.Add(new Teleporter(grids[1], teleporter1Nodes));
        teleporters.Add(new Teleporter(grids[1], teleporter2Nodes));
        teleporters.Add(new Teleporter(grids[2], teleporter3Nodes));
        teleporters.Add(new Teleporter(grids[2], teleporter4Nodes));

        //Now we can instantiate our Agents.
        List<Agent> agents = new List<Agent>();
        List<Node> usedNodes = new List<Node>();
        for(int i = 0; i < numAgents; i++){
            CustomGrid randGrid = grids[(int) Random.Range(0f, 2.9f)];

            Node randNode = randGrid.getRandomNode();
            while(usedNodes.Contains(randNode)){
                randNode = randGrid.getRandomNode();
            }
            usedNodes.Add(randNode);
            Agent newAgent = new Agent(Instantiate(agentPrefab), randNode, randGrid.getRandomNode());
            newAgent.agentObj.GetComponent<Renderer>().material.color = Random.ColorHSV(0f,1f,1f,1f,0.5f,1f); //Give a random color to the agent.
            agents.Add(newAgent);
        }
        //Then, begin pathfinding.
        PathfindingManager.instance.startPathfinding(agents, grids);

        //Finally, we must initiate the teleporters. To start, teleporters 0 and 1 will be active, taking 5s total to complete teleportation (2s to empty, 2s to enter, 1s teleportation).
        //After that, teleporters 2 and 3 do the same thing, and we alternate infinitely. This happens in a Coroutine.
        StartCoroutine("teleportRoutine");
        
    }

}

//Needed for the Teleporter object
public enum TeleporterState{
    Empty,
    Enter,
    Teleport
}

//Class representing the teleporters.
public class Teleporter{

    public CustomGrid myGrid;
    public TeleporterState curState;
    //Each teleporter has 3 wait nodes and 3 teleporter nodes.
    public List<TeleporterNode> waitNodes = new List<TeleporterNode>();
    public List<TeleporterNode> teleportNodes = new List<TeleporterNode>();

    public Teleporter(CustomGrid inputGrid, List<TeleporterNode> nodes){
        this.myGrid = inputGrid;
        this.curState = TeleporterState.Empty;
        foreach(TeleporterNode node in nodes){
            if(node.isWaitNode){
                this.waitNodes.Add(node);
            }else{
                this.teleportNodes.Add(node);
            }
        }
    }

    //This function does something depending on the Teleporter object's state at the time of the call.
    public void executeStateAction(){
        if(curState == TeleporterState.Empty){
            //The "Empty" action finds a path for each aAgent on the teleportNodes. 
            foreach(TeleporterNode tNode in teleportNodes){
                if(tNode.isOccupied()){
                    PathfindingManager.instance.findPath(tNode.getOccupant());
                    PathfindingManager.instance.numPaths++;
                    //This should let them travel instantly via the PathFindingManager. In fact, it  may not even be necessary to call it here.
                    tNode.setOccupant(null);
                }
            }
        }else if (curState == TeleporterState.Enter){
            //Agents occupying the waitingNodes are moved to the teleportNodes
            for(int i = 0; i < 3; i++){
                TeleporterNode wNode = waitNodes[i];
                TeleporterNode tNode = teleportNodes[i];
                if(wNode.isOccupied()){
                    Agent occupant = wNode.getOccupant();
                    occupant.travelPath.Clear();
                    occupant.travelPath.Add(tNode);
                }
            }

        }else{ //Case for teleporting.
            //Agents occupying the teleportNodes are moved to the corresponding connected teleportNodes.
            for(int i = 0; i < 3; i++){
                TeleporterNode tNode = teleportNodes[i];
                if(tNode.isOccupied()){
                    Agent aAgent = tNode.getOccupant();
                    TeleporterNode connectorNode = tNode.getConnector();
                    aAgent.agentObj.transform.position = connectorNode.pos;
                    aAgent.curNode = connectorNode;
                    connectorNode.setOccupant(aAgent);

                }
            }

        }
    }

}