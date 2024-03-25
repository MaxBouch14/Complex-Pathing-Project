 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

//Bridge class w/ needed fields and a constructor.
public class Bridge{
    public BridgeState state;
    public BridgeNode node1; //Node1 will always be a "sidePlatform" node.
    public BridgeNode node2;
    public int onBridge;

    public Bridge(BridgeState aState, BridgeNode aNode1, BridgeNode aNode2){
        state = aState;
        node1 = aNode1;
        node2 = aNode2;
        onBridge = 0;
    }

}


public class PathfindingManager : MonoBehaviour
{

    //This manager is a singleton object, so make the appropriate field/function here. Can be called from other scripts via "Manager.instance".
    public static PathfindingManager instance = null;
    void Awake(){
        if(instance == null){
            instance = this;
        }else if (instance != this){
            Destroy(gameObject);
        }
    }

    //This script will have access to the Grids, bridges, teleporters, and NPCs. It will be the one who creates paths for NPCs, handles collisions, et cetera.
    //We instantiate all needed fields here.
    private bool started = false;
    private List<Agent> agents;
    private List<CustomGrid> grids;
    private List<Bridge> bridges = new List<Bridge>();
    public bool Astar = true; //If not true, do RRT instead.
    public LayerMask obstMask;

    public Text numPathsText;
    public Text timeText;
    public Text rePathingsText;
    public Text abandonmentsText;
    public int numPaths = 0;
    private float timeSpentTotal = 0f;
    private int numRepaths = 0;
    private int numAbandonments = 0;

    //This function is called once the GameManager has set up the initial state. It creates the bridges and gives each agent an initial path.
    public void startPathfinding(List<Agent> aAgents, List<CustomGrid> aGrids){
        numPathsText.text = "Number of paths: 0";
        timeText.text = "Time spent: 0";
        rePathingsText.text = "Repathings: 0";
        abandonmentsText.text = "Abandonments: 0";

        agents = aAgents;
        grids = aGrids;
        started = true;
        bridges.Add(new Bridge(BridgeState.emptyBridge, (BridgeNode) grids[0].getNode(0,12), (BridgeNode) grids[1].getNode(14,12) )); //Middle bridge
        bridges.Add(new Bridge(BridgeState.emptyBridge, (BridgeNode) grids[0].getNode(0,4), (BridgeNode) grids[2].getNode(14,4) )); //Left bridge
        bridges.Add(new Bridge(BridgeState.emptyBridge, (BridgeNode) grids[0].getNode(0,20), (BridgeNode) grids[2].getNode(14,20) )); //Right bridge 
        
        foreach(Agent ag in agents){
            setDest(ag);
            findPath(ag);
            numPaths++;
        }
        
    }

    //Self explanatory function that changed bridge state. Called by agents when getting onto/leaving a bridge.
    public void changeBridgeState(int bridgeIndex, BridgeState newState){
        //We only set the bridge to empty if there are no agents on it.
        if(newState != BridgeState.emptyBridge){
           bridges[bridgeIndex].onBridge++; 
        }else{
            bridges[bridgeIndex].onBridge--;
        }

        if(! (bridges[bridgeIndex].onBridge != 0 && newState == BridgeState.emptyBridge)){
        bridges[bridgeIndex].state = newState;
        }
    }

    //Given a node, we get its bridge.
    public Bridge getBridgeFromNode(BridgeNode bNode){
        foreach(Bridge b in bridges){
            if(bNode.Equals(b.node1) || bNode.Equals(b.node2)){
                return b;
            }
        }
        return null; //Should never happen.
    }

    //Called when a node is done with a path or abandons a path, sets a new destination.
    void setDest(Agent aAgent){
        //First, we select a random Grid.
        CustomGrid destGrid = grids[(int) Random.Range(0f, 2.9f)];
        //Then take a random node on that grid.
        aAgent.destNode = destGrid.getRandomNode(); 
        while(aAgent.destNode == aAgent.curNode){
            aAgent.destNode = destGrid.getRandomNode(); 
        }
        return;
    }

    //This function decides whether or not we should be going for a grid node, teleporter node or bridge node (see attached PDF). It then calls the "actual" pathfinder.
    public void findPath(Agent aAgent){
        //First get get our destination grid, and decide whether we'll need to head to a bridge, teleporter, or just a node.
        CustomGrid destGrid = aAgent.destNode.myGrid;
        Node pathDest;
        pathDest = aAgent.destNode;
        if(destGrid != aAgent.curNode.myGrid){
            //Here, we must decide which case we're in (do we need to use a bridge or a teleporter)
            if(((aAgent.curNode.myGrid.thisGrid == CustomGrid.gridType.sideGrid) && (destGrid.thisGrid == CustomGrid.gridType.bottomGrid || destGrid.thisGrid == CustomGrid.gridType.topGrid)) 
            || ((aAgent.curNode.myGrid.thisGrid == CustomGrid.gridType.topGrid) && (destGrid.thisGrid == CustomGrid.gridType.sideGrid))
            || ((aAgent.curNode.myGrid.thisGrid == CustomGrid.gridType.bottomGrid) && (destGrid.thisGrid == CustomGrid.gridType.sideGrid))){
                //In this case, we need to use a bridge.
                //So, we choose a bridge node. (MAYBE RETURN HERE IF THERE ARE NO AVAILABLE NODES?)
                pathDest = aAgent.curNode.myGrid.getBridgeNode(destGrid.thisGrid);
            }else if(((aAgent.curNode.myGrid.thisGrid == CustomGrid.gridType.topGrid) && (destGrid.thisGrid == CustomGrid.gridType.bottomGrid)) 
            || ((aAgent.curNode.myGrid.thisGrid == CustomGrid.gridType.bottomGrid) && (destGrid.thisGrid == CustomGrid.gridType.topGrid))){
                //In this case, we need to use a teleporter.
                //So, need a function that will attempt to give a teleporter wait node. (Treat "stopping" like with bridgenodes)
                pathDest = aAgent.curNode.myGrid.getTeleporterNode();
                if(pathDest == null){
                    aAgent.waitingForPath = true;
                    return;
                }
            }
        }
        //Depending on our selection in the editor, this is where we decide which pathfinding technique to use.
        if(Astar){
            findPathAstar(aAgent, pathDest);
        }else{
            findPathRRT(aAgent, pathDest);
        }
    }
    //Does pathfinding via the A* algorithm.
    private void findPathAstar(Agent aAgent, Node pathDest){

        float startTime = Time.realtimeSinceStartup; //Put this here so we can track time spent pathfinding.

        Node start = aAgent.curNode;
        aAgent.curNode.gCost = 0; //Set to 0 initially for the starting node.
        aAgent.curNode.hCost = getDistance(aAgent.curNode, pathDest);

        //Create open and closed lists, and add the current node to the open list.
        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();
        openList.Add(aAgent.curNode);

        //Main loop, functions as seen in class.
        while(openList.Count > 0){
            Node curNode = openList[0];

            for(int i = 1; i < openList.Count; i++){
                if(openList[i].fCost <= curNode.fCost && openList[i].hCost < curNode.hCost){
                    curNode = openList[i];
                }
            }
            openList.Remove(curNode);
            closedList.Add(curNode);

            //Situation where we've reached the destination.
            if(curNode == pathDest){
                //This means we're done, so we can reset all node costs on our way out.
                foreach(Node node in closedList){
                    node.gCost = 1000;
                    node.hCost = 1000;
                }
                foreach(Node node in openList){
                    node.gCost = 1000;
                    node.hCost = 1000;
                }

                //Call a function "travel", which reverses what we have to get the actual path, and begins moving the agent along it.
                aAgent.waitingForPath = false;
                setTravelPath(start, pathDest, aAgent);
                timeSpentTotal += (Time.realtimeSinceStartup - startTime);
                break;
            }

            //If we're not at the curNode, we need to find our neighbours to finish the loop.
            foreach(Node neighbourNode in aAgent.curNode.myGrid.getNeighbours(curNode)){
                //Extra check here in case the neighbour is a teleporterNode, since teleporterNodes are treated as obstacles when spawning agents/getting regular destinations.
                if(neighbourNode.isObstacle || closedList.Contains(neighbourNode) || (neighbourNode is TeleporterNode && neighbourNode != pathDest)){
                    continue;
                }
                int movCost = curNode.gCost + getDistance(curNode, neighbourNode); 
                if(movCost < neighbourNode.gCost){ 
                    neighbourNode.gCost = movCost;
                    neighbourNode.hCost = getDistance(neighbourNode, pathDest);
                    neighbourNode.parNode = curNode;

                    if(! openList.Contains(neighbourNode)){
                        openList.Add(neighbourNode);
                    }
                }
            }
        }
    }
    //Second pathfinding algorithm, this one implements RRT.
    private void findPathRRT(Agent aAgent, Node pathDest){
        float startTime = Time.realtimeSinceStartup; //Put this here so we can track time spent pathfinding.
        //First, we'll just store all nodes of our "tree" in a list (the tree really just works on a parent-child basis, like in Astar)
        List<Node> treeNodes = new List<Node>();
        //Add the "start" (current node) to the tree.
        treeNodes.Add(aAgent.curNode);
        //set a counter limit.
        int counterLimit = 9999;
        RaycastHit hit;

        //Before we start, we'll quickly check to see if the curNode is already visible to the destNode. If so, this will save us some work.
        if(! Physics.Raycast(aAgent.curNode.pos,(pathDest.pos - aAgent.curNode.pos).normalized, out hit, 1000f, obstMask)){ //If there is no hit, they are visible.
              pathDest.parNode = aAgent.curNode;
              aAgent.waitingForPath = false;
              setTravelPath(aAgent.curNode, pathDest, aAgent);
              timeSpentTotal += Time.realtimeSinceStartup - startTime;
              return;
        }

        Node randNode = aAgent.curNode.myGrid.getRandomNode();
        while(counterLimit >= 0){
            //Otherwise, we find a new randomNode which isn't already in the tree.
            while(treeNodes.Contains(randNode)){
                randNode = aAgent.curNode.myGrid.getRandomNode();
            }
            //Now, we must get the Node in the tree that is closest to this randNode.
            Node closest = treeNodes[0]; //Start with the root.
            int distClosest = getDistance(randNode, closest);
            foreach(Node tNode in treeNodes){
                //If the new distance is less than the closest distance, we set a new distClosest.
                int newDist = getDistance(randNode, tNode);
                if(newDist < distClosest){
                    closest = tNode;
                    distClosest = newDist;
                }
            }
            //Next, if the nodes are visible to each other, we add the RandNode to the closest node in the tree. 
            if(! Physics.Raycast(closest.pos,(randNode.pos - closest.pos).normalized, out hit, 1000f, obstMask)){ //If there is no hit, they are visible.
                randNode.parNode = closest;
                treeNodes.Add(randNode);

                //Finally, we check to see if it is visible to the destination. If so, we move to it and end the function.
                if(! Physics.Raycast(randNode.pos,(pathDest.pos - randNode.pos).normalized, out hit, 1000f, obstMask)){ //If there is no hit, they are visible.
                    pathDest.parNode = randNode;
                    aAgent.waitingForPath = false;
                    setTravelPath(aAgent.curNode, pathDest, aAgent);
                    timeSpentTotal += Time.realtimeSinceStartup - startTime;
                    break;
                }

            }

            counterLimit--;
        } 
    }

    //Called after A* or RRT, creates and sets a TravelPath for the agent, which is travelled through in Agent.travel() in FixedUpdate().
    void setTravelPath(Node startNode, Node destNode, Agent aAgent){
        Node curNode = destNode;
        aAgent.travelPath.Clear();
        while(curNode != startNode){
            aAgent.travelPath.Add(curNode);
            curNode = curNode.parNode;
        }
        aAgent.travelPath.Add(curNode);
        aAgent.travelPath.Reverse();

        Node lastNode = aAgent.lastPathNode();
        if(lastNode is BridgeNode){
            aAgent.setNeededState(lastNode);
        }

        aAgent.travel();
    }

    //A function that gives us the 8-way distance between nodes.
    int getDistance(Node p, Node q){

        int result = (int) ((Mathf.Pow(Mathf.Pow(Mathf.Abs(q.gridX - p.gridX),2) + Mathf.Pow(Mathf.Abs(q.gridY - p.gridY),2), 0.5f)) * 10f);
        return result;
        // Chess distance: return Mathf.Max(Mathf.Abs(q.gridX - p.gridX), Mathf.Abs(q.gridY - p.gridY));
    }

    void FixedUpdate(){
        //Only do this if pathfinding has started.
        if(started){
            foreach(Agent ag in agents){
                Node lastPathNode = ag.lastPathNode();

                //If the last path node is a bridge, we do extra checks to ensure the bridge we're going to has compatible state. If not, we increment repath/abandonment counters.
                if(lastPathNode is BridgeNode){
                    Bridge target = getBridgeFromNode((BridgeNode) lastPathNode);
                    if(ag.isBridgeDirectionCompatible(target.state)){
                        ag.repathCounter = 0;
                        ag.waitingCounter = 0;
                        ag.travel();
                    }else if ((ag.myBridgeIndex == 1 || ag.myBridgeIndex == 2) && ag.repathCounter < 5){ //Repath up to only 5 times, should ensure we try both bridgeNodes.
                        ag.repathCounter++;
                        findPath(ag);
                        //Increment global repath counter AND global path amt.
                        numPaths++;
                        numRepaths++;
                    }else if(ag.waitingCounter > ag.waitingCap){

                        //Increment global abandoned paths if we had remaining nodes in the path.
                        if(ag.travelPath.Count != 0){
                            numAbandonments++;
                        }

                        ag.waitingCounter = 0;
                        ag.waitingCap = (int) Random.Range(100, 500);
                        ag.agentBridgeState = BridgeState.emptyBridge;
                        ag.myBridgeIndex = -1;
                        setDest(ag);
                        ag.waitingForPath = true;
                    }else{
                        ag.waitingCounter++;
                    }
                    //Similar to above, we have a check if moving to a teleporter to ensure there is a free spot.
                }else if (lastPathNode is TeleporterNode){
                    if(! ((TeleporterNode) lastPathNode).isOccupied()){
                        ag.repathCounter = 0;
                        ag.waitingCounter = 0;
                        ag.travel();
                    }else if(ag.repathCounter < 5){
                        ag.repathCounter++;
                        findPath(ag);
                        //Increment global repath counter AND global path amt.
                        numPaths++;
                        numRepaths++;
                    }else if(ag.waitingCounter > ag.waitingCap){

                        //Increment global abandoned paths if we had a remaining path.
                        if(ag.travelPath.Count != 0){
                            numAbandonments++;
                        }

                        ag.waitingCounter = 0;
                        ag.waitingCap = (int) Random.Range(100, 500);
                        ag.agentBridgeState = BridgeState.emptyBridge;
                        ag.myBridgeIndex = -1;
                        setDest(ag);
                        ag.waitingForPath = true;
                    }else{
                        ag.waitingCounter++;
                    }
                }else{ //If not heading to a teleporter or bridge node, we check to see if an agent was waiting. If so, give it a new path and otherwise just travel.
                    if(ag.waitingForPath){
                        findPath(ag);
                        numPaths++;
                        //Increment global path amount.
                    }else if(ag.waitingCounter > ag.waitingCap){ //In this case, we've been waiting too long and abandon our path to begin a new one.

                        //Increment global abandoned paths if we had a remaining path.
                        if(ag.travelPath.Count != 0){
                            numAbandonments++;
                        }

                        ag.waitingCounter = 0;
                        ag.waitingCap = (int) Random.Range(100, 500);
                        setDest(ag);
                        ag.waitingForPath = true;

                    }else{
                        ag.travel();
                    }
                }
            }

        //Extra stuff for data-analysis.
        numPathsText.text = "Number of paths: " + numPaths;
        timeText.text = "Time spent: " +  decimal.Round((decimal) timeSpentTotal, 10);
        rePathingsText.text = "Repathings: " + numRepaths;
        abandonmentsText.text = "Abandonments: " + numAbandonments;
        if(Time.realtimeSinceStartup > 120){
            Debug.Log("NumPaths: " + numPaths);
            Debug.Log("TimeSpentTotal: " + decimal.Round((decimal) timeSpentTotal, 10));
            Debug.Log("rePathings total : " + numRepaths);
            Debug.Log("Abandonments total: " + numAbandonments);
            EditorApplication.isPaused = true;
        }
        }
    }
}
