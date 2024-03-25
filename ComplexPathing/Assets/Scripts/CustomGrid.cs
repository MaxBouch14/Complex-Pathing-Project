using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGrid : MonoBehaviour
{
    //Initialize needed fields.
    public Transform initPos;
    public LayerMask ObstacleMask;
    public Vector2 gridSize;
    public float dist;
    public List<GameObject> obstacles;

    private Node[,] grid;
    private int gridXSize;
    private int gridYSize;

    //Each grid is one of 3 types.
    public enum gridType{
        sideGrid,
        bottomGrid,
        topGrid
    }
    public gridType thisGrid;

    // Start is called before the first frame update
    public void instantiateGrid()
    {
        gridXSize = (int) gridSize.x;
        gridYSize = (int) gridSize.y;

        placeObstacles();
        createGrid();
    }

    //This function creates the grid. They are 15 x 26.
    void createGrid(){ 
        grid = new Node[gridXSize, gridYSize];
        
        for(int i = 0; i < gridXSize; i++){
            for(int j = 0; j < gridYSize; j++){
                Vector3 location = initPos.position + new Vector3(i, 0, j);
                
                bool isObst = false;
                if(Physics.CheckSphere(location, 0.4f, ObstacleMask)){
                    isObst = true;
                }
                //This chunk of code creates the BridgeNodes and TeleporterNodes at the appropriate locations.
                if((thisGrid == gridType.sideGrid) && i == 0 && (j == 4 || j == 20 || j == 12)){
                    grid[i,j] = new BridgeNode(i, j, isObst, location, this);
                }else if((thisGrid == gridType.bottomGrid) && i == 14 && j == 12){
                    grid[i,j] = new BridgeNode(i, j, isObst, location, this);
                }else if((thisGrid == gridType.topGrid) && i == 14 && (j == 4 || j == 20)){
                    grid[i,j] = new BridgeNode(i, j, isObst, location, this);
                }else if((thisGrid == gridType.topGrid || thisGrid == gridType.bottomGrid) && (i == 3) && (j == 2 || j == 3 || j == 4 )){
                    grid[i,j] = new TeleporterNode(i, j, isObst, location, this, true);
                }else if((thisGrid == gridType.topGrid || thisGrid == gridType.bottomGrid) && (i == 2) && (j == 2 || j == 3 || j == 4 )){
                    grid[i,j] = new TeleporterNode(i, j, isObst, location, this, false);
                }else if((thisGrid == gridType.topGrid || thisGrid == gridType.bottomGrid) && (i == 3) && (j == 20 || j == 21 || j == 22 )){
                    grid[i,j] = new TeleporterNode(i, j, isObst, location, this, true);
                }else if((thisGrid == gridType.topGrid || thisGrid == gridType.bottomGrid) && (i == 2) && (j == 20 || j == 21 || j == 22 )){
                    grid[i,j] = new TeleporterNode(i, j, isObst, location, this, false);
                }else{
                    grid[i, j] = new Node(i, j, isObst, location, this);
                }
            }
        }
    }

        //Called before creating the grid, this function places the obstacles. That way, when creating the grid we can assign "ObstacleNodes".
        void placeObstacles(){
        if(obstacles.Count >= 5){
            //Places obstacles on the "side" grid (six obstacles)
            int obstCount = 0;
            while(obstCount < 6){
                GameObject curObst = obstacles[((int) Random.Range(0f, 4f))];
                Vector3 attemptedLoc = new Vector3(Random.Range(-4f, 4.5f), initPos.position.y, Random.Range(-11f,11f));
                if(!Physics.CheckSphere(attemptedLoc, 3f, ObstacleMask)){
                    Quaternion randomRotation = Quaternion.Euler(90,0,Random.Range(0, 360));
                    if(curObst == obstacles[2]){
                        randomRotation = Quaternion.Euler(0,Random.Range(0, 360),0);
                    }
                    Instantiate(curObst, attemptedLoc, randomRotation);
                    obstCount++;
                }
            }
        }else{
            //Places obstacles on the "top" and "bottom" grid (two obstacles each)
            int obstCount = 0;
            while(obstCount < 2){
                GameObject curObst = obstacles[((int) Random.Range(0f, 4f))];
                Vector3 attemptedLoc;
                if(initPos.position.y < 5){
                    attemptedLoc = new Vector3(Random.Range(-30f, -25f), initPos.position.y, Random.Range(-11f,11f));
                    if(attemptedLoc.z >= -3 && attemptedLoc.z <= 3){
                    attemptedLoc.x = Random.Range(-25f, -35f);
                    }
                }else{
                    attemptedLoc = new Vector3(Random.Range(-34f, -29f), initPos.position.y, Random.Range(-11f,11f));
                    if(attemptedLoc.z >= -3 && attemptedLoc.z <= 3){
                    attemptedLoc.x = Random.Range(-39f, -29f);
                    }
                }

                if(!Physics.CheckSphere(attemptedLoc, 3f, ObstacleMask)){
                    Quaternion randomRotation = Quaternion.Euler(90,0,Random.Range(0, 360));
                    if(curObst == obstacles[2]){
                        randomRotation = Quaternion.Euler(0,Random.Range(0, 360),0);
                    }
                    Instantiate(curObst, attemptedLoc, randomRotation);
                    obstCount++;
                }
            }
        }
    }

    //Get a random node from this grid which is not an obstacle or a teleporter.
    public Node getRandomNode(){ 
        Node result;
        while(true){
            result = grid[(int) Random.Range(0f,14f), (int) Random.Range(0f, 24f)];
            if(!result.isObstacle && ! (result is BridgeNode) && ! (result is TeleporterNode)){
                return result;
            }
        }
    }

    //Gets a random bridgeNode, for setting an Agent's travel to a bridge.
    public BridgeNode getBridgeNode(gridType type){
        List<BridgeNode> nodes = new List<BridgeNode>();
        if(thisGrid == gridType.sideGrid && type == gridType.bottomGrid){
            return (BridgeNode) grid[0,12];
        }else if(thisGrid == gridType.sideGrid && type == gridType.topGrid){
            nodes.Add((BridgeNode) grid[0,4]);
            nodes.Add((BridgeNode) grid[0,20]);
            return nodes[(int) Random.Range(0f, 1.9f)];
        }else if(thisGrid == gridType.topGrid && type == gridType.sideGrid){
            nodes.Add((BridgeNode) grid[14,4]);
            nodes.Add((BridgeNode) grid[14,20]);
            return nodes[(int) Random.Range(0f, 1.9f)];
        }else if(thisGrid == gridType.bottomGrid && type == gridType.sideGrid){
            return (BridgeNode) grid[14,12];
        }
        return null;
    }

    //Similar to the above function, returns an available teleporter waitNode. Null means none are available.
    public TeleporterNode getTeleporterNode(){
        if((thisGrid == gridType.bottomGrid) || (thisGrid == gridType.topGrid)){
            List<TeleporterNode> waitNodes = new List<TeleporterNode>();
            waitNodes.Add((TeleporterNode) grid[3, 20]);
            waitNodes.Add((TeleporterNode) grid[3, 21]);
            waitNodes.Add((TeleporterNode) grid[3, 22]);
            waitNodes.Add((TeleporterNode) grid[3, 2]);
            waitNodes.Add((TeleporterNode) grid[3, 3]);
            waitNodes.Add((TeleporterNode) grid[3, 4]);

            foreach(TeleporterNode tNode in waitNodes){
                if(tNode.isWaitNode && ! tNode.isReserved){
                    tNode.isReserved = true;
                    return tNode;
                }
            }
            return null;

        }else{
            return null; //Should never happen.
        }
    }

    //Gets a specified node on the grid.
    public Node getNode(int x, int y){
        return grid[x,y];
    }

    //Function used in A* to get a node's 8 neighbours.
    public List<Node> getNeighbours(Node aNode){

        List<Node> result = new List<Node>();
        int xCheck;
        int yCheck;

        for(int i = -1; i <=1 ; i++){
            for(int j = -1; j <= 1 ; j++){
                if(i == 0 && j == 0){ continue; }

                xCheck = aNode.gridX + i;
                yCheck = aNode.gridY + j;

                if(xCheck >= 0 && xCheck < gridXSize && yCheck >= 0 && yCheck < gridYSize){
                    result.Add(grid[xCheck, yCheck]);
                }

            }
        }
        return result;
    }

    //Function that can be un-commented to visualize the grid using Gizmos.
    /*
    private void OnDrawGizmos(){
        if(grid != null){
            foreach(Node node in grid){
                if(node.isObstacle){
                    Gizmos.color = Color.red;
                }else if(node is BridgeNode){
                    Gizmos.color = Color.black;
                }else if(node is TeleporterNode){
                    Gizmos.color = Color.magenta;
                    if(((TeleporterNode) node).isWaitNode){
                        Gizmos.color = Color.cyan;
                    }
                }else{
                    Gizmos.color = Color.green;
                }
                //Gizmos.DrawCube(node.pos, Vector3.one);
                Gizmos.DrawSphere(node.pos, 0.1f);
            }
        }
    }*/

}
