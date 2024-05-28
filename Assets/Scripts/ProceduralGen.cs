using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graphs;



public class ProceduralGen : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 10;
    public int roomCount = 10;
    
    private List<int> xValues = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 ,17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30};
    private List<int> zValues = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 ,17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30};

    

    private List<GameObject> Rooms = new List<GameObject>();
    private List<Edge> edges;
    private int[,] AdjMatrix;
    private int[,] GizmosAdjMatrix;
    public Material discovered;
    private List<int> visitedNodes = new List<int>();
    private Color customColor = new Color(0.4f, 0.9f, 0.7f, 1.0f);
    private Vector3 size = new Vector3(9, 9, 9);

    private int[,] mapGrid;
    // Start is called before the first frame update
    
    private List<int>leafNodes = new List<int>();


    Dictionary<int, int> nodeToBranch = new Dictionary<int, int>();
    int currentBranchId = 0;
    

    
    [Header("Map Gen Options Selection")]
    public int mapWidth = 300;
    public int mapHeight = 300;
    public int NumberOfRooms = 10;
    public int AddEdges = 3;
    public int xFartherstNode = 3;
    public float generationSpeed = 0.02f;
    [Header("Object Prefab Selection")]
    public GameObject basicRoom; 
    public GameObject hallwayZ; 
    public GameObject hallwayX; 

    public GameObject hallwayRightToDown;
    public GameObject hallwayLeftToDown;
    public GameObject hallwayRightToUp;
    public GameObject hallwayLeftToUp;
    void Start()
    {
        //Vector3 randomPosition = new Vector3(0, 0, 0);
        //GameObject room = Instantiate(hallwayZ, randomPosition, Quaternion.identity);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            GenerateNewRooms();
            
        }

        if (Input.GetKeyDown("s"))
        {
            GenerateMST(Rooms);
            //AddExtraEdges(1);
             // Assuming 0 is the index of the starting parent
            Rooms[0].GetComponent<Renderer>().material.SetColor("_Color", Color.green);
            //StartCoroutine(DFSvisualized(0,0));
            Debug.Log("DFS Path: " + string.Join(" -> ", visitedNodes));
        }

        if(Input.GetKeyDown("d")){
            AddLoops(AddEdges);
            //GenerateHallways(0);
        }

        if(Input.GetKeyDown("f")){
           StartCoroutine(DFSvisualized(0,0)) ; 
            
        }
        
        if(Input.GetKeyDown("g")){
            GenerateLoopHallways();
            
        }

        if(Input.GetKeyDown("l")){
            StartCoroutine(fullGen());
            
        }
       
    }

    IEnumerator fullGen()
    {
        GenerateRooms();
        yield return new WaitForSeconds(generationSpeed);
        GenerateMST(Rooms);
        yield return new WaitForSeconds(generationSpeed);
        yield return StartCoroutine(DFSvisualized(0,0));
        yield return new WaitForSeconds(generationSpeed);
        AddLoops(AddEdges);
        yield return new WaitForSeconds(generationSpeed);
        GenerateLoopHallways();
        yield return new WaitForSeconds(generationSpeed);

    }
    void GenerateRooms()
    {
        
        if(xValues.Count < 1 || zValues.Count < 1){
                    Debug.Log("youre out of rooms");
                    
        }

        mapGrid = new int[30, 30];

        for(int i = 0; i < 30; i++)
        {
            for(int j = 0; j < 30; j++)
            {
                mapGrid[i,j] = 0;
            }
        }

        int count = 0;
        while (xValues.Count >= 1 && zValues.Count >= 1)
        {
            // Generate random positions within the grid boundaries
            
            int xPos = xValues[Random.Range(0, xValues.Count)];
            int zPos = zValues[Random.Range(0, zValues.Count)];

            int randomX = xPos * 10;
            int randomZ = zPos * 10;

            Vector3 randomPosition = new Vector3(randomX, 0, randomZ);
            xValues.Remove(xPos);
            zValues.Remove(zPos);

            mapGrid[xPos-1, zPos-1] = 1;
            // Instantiate a cube at the random position
            GameObject room = Instantiate(basicRoom, randomPosition, Quaternion.identity);
            room.name = (count).ToString();
            count++;
            Rooms.Add(room);
        }

 /*
        for(int i = 0; i < 30; i++)
        {
            for(int j = 0; j < 30; j++)
            {
                Debug.Log(string.Join(" .", mapGrid[i,j]));
                
            }
            Debug.Log("\n");
        }
        */


    }

    void GenerateNewRooms()
    {

        int count = 0;
        while (count < NumberOfRooms)
        {
            // Generate random positions within the grid boundaries
            
            int xPos = xValues[Random.Range(0, mapWidth/10)];
            int zPos = zValues[Random.Range(0, mapHeight/10)];

            int randomX = xPos * 10;
            int randomZ = zPos * 10;

            Vector3 randomPosition = new Vector3(randomX, 0, randomZ);
            
            //xValues.Remove(xPos);
            //zValues.Remove(zPos);
            int roomWidth = Random.Range(1,5) * 10;
            int roomHeight = Random.Range(1,5) * 10;
            Vector3 roomSize = new Vector3(roomWidth, 5, roomHeight);
            Collider[] col = Physics.OverlapBox(randomPosition, roomSize/2, Quaternion.identity);
            if(col.Length == 0)
            {
                Debug.Log("no colliders");
                GameObject room = Instantiate(basicRoom, randomPosition, Quaternion.identity);
                room.transform.localScale = new Vector3(roomWidth,5,roomHeight);   
                room.name = (count).ToString();
                Rooms.Add(room);
                count++;
            }
            
        

 /*
        for(int i = 0; i < 30; i++)
        {
            for(int j = 0; j < 30; j++)
            {
                Debug.Log(string.Join(" .", mapGrid[i,j]));
                
            }
            Debug.Log("\n");
        }
        */
        }

    }

    

    void GenerateMST(List<GameObject> Rooms)
    {
        int roomCount = Rooms.Count;
        AdjMatrix = new int[roomCount, roomCount];
        GizmosAdjMatrix =  new int[roomCount, roomCount];
        bool[] inMST = new bool[roomCount];
        float[] minEdgeWeight = new float[roomCount];
        int[] parent = new int[roomCount];

        for (int i = 0; i < roomCount; i++)
        {
            minEdgeWeight[i] = float.MaxValue;
            parent[i] = -1; //-1 means the node has no parent. this will change further in the code
        }

        minEdgeWeight[0] = 0f;

        for (int count = 0; count < roomCount - 1; count++)
        {
            float min = float.MaxValue;
            int u = -1; 

            for (int i = 0; i < roomCount; i++)
            {
                if (!inMST[i] && minEdgeWeight[i] < min)
                {
                    min = minEdgeWeight[i];
                    u = i;
                }
            }

            inMST[u] = true; // this puts the room in the mst

            for (int i = 0; i < roomCount; i++)
            {
                if (!inMST[i])
                {
                    float distance = Vector3.Distance(Rooms[u].transform.position, Rooms[i].transform.position);
                    if (distance < minEdgeWeight[i])
                    {
                        minEdgeWeight[i] = distance;
                        parent[i] = u;
                    }
                }
            }
        }

        for (int i = 1; i < roomCount; i++)
        {
            AdjMatrix[parent[i], i] = 1;
            AdjMatrix[i, parent[i]] = 1;
            GizmosAdjMatrix[parent[i], i] = 1;
            GizmosAdjMatrix[i, parent[i]] = 1;
        }


  Debug.Log("Parents: " + string.Join(" -> ", parent));

  //Checks for leaf rooms and logs when the branch is done

  for (int i = 0; i < roomCount; i++)
    {
        bool isLeaf = true;
        for (int j = 0; j < roomCount; j++)
        {
            if (parent[j] == i)
            {
                isLeaf = false;
                break;
            }
        }
        if (isLeaf && i != 0)
        {
            
            leafNodes.Add(i);
        }
    }

    Debug.Log("all leaf nodes are: " + string.Join(" -> ", leafNodes));

    //create loops given the leaf nodes and the 4th farthest node from them
   
    
    }

    void AddLoops(int edgeCount)
    {
        for (int i = 0; i < edgeCount; i++)
        {
            
                List<KeyValuePair<float, int>> distances = new List<KeyValuePair<float, int>>();
                for (int j = 0; j < Rooms.Count; j++)
                {
                    if (!leafNodes.Contains(j))
                    {
                        float distance = Vector3.Distance(Rooms[leafNodes[i]].transform.position, Rooms[j].transform.position);
                        distances.Add(new KeyValuePair<float, int>(distance, j));
                    }

                }
                distances.Sort((x, y) => x.Key.CompareTo(y.Key));
                Debug.Log("distances in order: " + string.Join(" -> ", distances));
                int desiredNode = distances[xFartherstNode].Value;

                //add edge
                AdjMatrix[leafNodes[i], desiredNode] = 1;
                AdjMatrix[desiredNode, leafNodes[i]] = 1;
                GizmosAdjMatrix[leafNodes[i], desiredNode] = 1;
                GizmosAdjMatrix[desiredNode, leafNodes[i]] = 1;
            
        }
    }





    void OnDrawGizmos()
    {
        

         if ( AdjMatrix != null && Rooms != null)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < Rooms.Count; i++)
            {
                for (int j = 0; j < Rooms.Count; j++)
                {
                    if (AdjMatrix[i, j] == 1)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(Rooms[i].transform.position, Rooms[j].transform.position);
                    }
                    if (GizmosAdjMatrix[i, j] == 1 && AdjMatrix[i, j] == 0)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(Rooms[i].transform.position, Rooms[j].transform.position);
                    }
                }
            }
        }
    }



    bool IsPositionOccupied(Vector3 position, Vector3 size)
    {
        Collider[] colliders = Physics.OverlapBox(position, size / 2, Quaternion.identity);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != hallwayZ) // Ensure we're not detecting the room itself
            {
                return true;
            }
            if (collider.gameObject != hallwayX) // Ensure we're not detecting the room itself
            {
                return true;
            }
            if (collider.gameObject != hallwayLeftToDown) // Ensure we're not detecting the room itself
            {
                return true;
            }
            if (collider.gameObject != hallwayLeftToUp) // Ensure we're not detecting the room itself
            {
                return true;
            }
        }
        return false;
    }

/*****************************/
/*    Visualized Functions   */
/*****************************/
IEnumerator  DFSvisualized(int node, int prevRoomCode)
    {
        // Mark the current node as visited
        yield return new WaitForSeconds(generationSpeed);
        visitedNodes.Add(node);
        //Rooms[node].GetComponent<Renderer>().material = discovered;
        prevRoomCode = decideRoomType(node, prevRoomCode);
        
        if(node == 0) Rooms[node].GetComponent<Renderer>().material.SetColor("_Color", Color.green);

                       
            // Recur for all the vertices adjacent to this vertex
            for (int i = 0; i < Rooms.Count; i++)
            {
                
                if ((AdjMatrix[node, i] == 1 && !visitedNodes.Contains(i)))
                {
                    AdjMatrix[node, i] = 0;
                    AdjMatrix[i, node] = 0;

                    yield return StartCoroutine(pathfindHallwaysVisualized(node, i));
                    //yield return new WaitForSeconds(generationSpeed *3);
                    yield return StartCoroutine(DFSvisualized(i, prevRoomCode));
                
                }
            }

        
        
    }

void GenerateLoopHallways()
{
    for (int i = 0; i < Rooms.Count; i++)
    {
        for (int j = 0; j < Rooms.Count; j++)
        {
            if(AdjMatrix[i,j] == 1)
            {
                AdjMatrix[i,j] = 0;
                AdjMatrix[j,i] = 0;
                StartCoroutine(pathfindHallwaysVisualized(i,j));
            }
        }
    }
}


IEnumerator pathfindHallwaysVisualized(int originalNode, int targetNode)
{
    float xPos = Rooms[originalNode].transform.position.x;
    float zPos = Rooms[originalNode].transform.position.z;
    float targetXPos = Rooms[targetNode].transform.position.x;
    float targetZPos = Rooms[targetNode].transform.position.z; 
                    
    int xSign = 0;
    int zSign;
    //determines where to go
    if(xPos-targetXPos < 0){
        xSign = 1;
        //RIGHT
    }else{
        xSign = -1;
        //LEFT
    }

    if(zPos-targetZPos < 0){
        zSign = 1;
         //UP
    }else{
        zSign = -1;
        //DOWN
    }
        xPos += 10*xSign;

    while(xPos != targetXPos)
    {
        yield return new WaitForSeconds(generationSpeed);
                        
        Vector3 newPosition = new Vector3(xPos, 0, zPos + 2.5f* -zSign);
                        
        if(!IsPositionOccupied(newPosition, size))
        {
            GameObject room = Instantiate(hallwayX, newPosition, Quaternion.identity);
            room.name = "hallwayX";
        }
                        
        xPos += 10*xSign;
    }
        yield return new WaitForSeconds(generationSpeed);
        //Checks which corner hallway to place
        if(xSign == 1 && zSign == 1)
        {
            Vector3 newPosition = new Vector3(xPos, 0, zPos);
            if(!IsPositionOccupied(newPosition, size))
            {
                GameObject room = Instantiate(hallwayLeftToUp, newPosition, Quaternion.identity);
            }
                    
        }else if(xSign == 1 && zSign == -1)
        {
            Vector3 newPosition = new Vector3(xPos, 0, zPos);
            if(!IsPositionOccupied(newPosition, size))
            {
                 GameObject room = Instantiate(hallwayLeftToDown, newPosition, Quaternion.identity);
            }
                       
        }else if(xSign == -1 && zSign == 1)
        {
            Vector3 newPosition = new Vector3(xPos, 0, zPos);
            if(!IsPositionOccupied(newPosition, size))
            {
               GameObject room = Instantiate(hallwayRightToUp, newPosition, Quaternion.identity);
            }
                        
        }
        else if(xSign == -1 && zSign == -1)
        {
            Vector3 newPosition = new Vector3(xPos, 0, zPos);
            if(!IsPositionOccupied(newPosition, size))
            {
                GameObject room = Instantiate(hallwayRightToDown, newPosition, Quaternion.identity);
            }
                        
        }
            zPos += 10*zSign;

            while(targetZPos != zPos)
            {
                yield return new WaitForSeconds(generationSpeed);
                Vector3 newPosition = new Vector3(xPos + 2.5f*xSign, 0, zPos);
                GameObject room = Instantiate(hallwayZ, newPosition, Quaternion.identity);
                room.name = "hallwayZ";
                zPos += 10*zSign;
            }
}

/*****************************/
/*      Normal Functions     */
/*****************************/
void DFS(int node, int prevRoomCode)
{
     // Mark the current node as visited
        
        visitedNodes.Add(node);
        //Rooms[node].GetComponent<Renderer>().material = discovered;
        prevRoomCode = decideRoomType(node, prevRoomCode);
        
        if(node == 0) Rooms[node].GetComponent<Renderer>().material.SetColor("_Color", Color.green);

                       
            // Recur for all the vertices adjacent to this vertex
            for (int i = 0; i < Rooms.Count; i++)
            {
                if (AdjMatrix[node, i] == 1 && !visitedNodes.Contains(i) )
                {

                    //Debug.Log("Current Room X: " + Rooms[node].transform.position.x + "   and Z:  " + Rooms[node].transform.position.z);
                    //Debug.Log("NEXT Room X: " + Rooms[i].transform.position.x + "   and Z:  " + Rooms[i].transform.position.z);
                    AdjMatrix[node, i] = 0;
                    AdjMatrix[i, node] = 0;
                    pathfindHallways(node,i);
                    DFS(i, prevRoomCode);
                
                }
            }
        for (int i = 0; i < Rooms.Count; i++)
        {
            for (int j = 0; j < Rooms.Count; j++)
            {
                if(AdjMatrix[i,j] == 1)
                {
                    AdjMatrix[i,j] = 0;
                    AdjMatrix[j,i] = 0;
                    pathfindHallways(i,j);
                }
            }
        }
}


void pathfindHallways(int originalNode, int targetNode)
{
    float xPos = Rooms[originalNode].transform.position.x;
    float zPos = Rooms[originalNode].transform.position.z;
    float targetXPos = Rooms[targetNode].transform.position.x;
    float targetZPos = Rooms[targetNode].transform.position.z; 
                    
    int xSign = 0;
    int zSign;
    //determines where to go
    if(xPos-targetXPos < 0){
        xSign = 1;
        //RIGHT
    }else{
        xSign = -1;
        //LEFT
    }

    if(zPos-targetZPos < 0){
        zSign = 1;
         //UP
    }else{
        zSign = -1;
        //DOWN
    }
        xPos += 10*xSign;

    while(xPos != targetXPos)
    {
        
                        
        Vector3 newPosition = new Vector3(xPos, 0, zPos + 2.5f* -zSign);
                        
        if(!IsPositionOccupied(newPosition, size))
        {
            GameObject room = Instantiate(hallwayX, newPosition, Quaternion.identity);
            room.name = "hallwayX";
        }
                        
        xPos += 10*xSign;
    }
        
        //Checks which corner hallway to place
        if(xSign == 1 && zSign == 1)
        {
            Vector3 newPosition = new Vector3(xPos, 0, zPos);
            if(!IsPositionOccupied(newPosition, size))
            {
                GameObject room = Instantiate(hallwayLeftToUp, newPosition, Quaternion.identity);
            }
                    
        }else if(xSign == 1 && zSign == -1)
        {
            Vector3 newPosition = new Vector3(xPos, 0, zPos);
            if(!IsPositionOccupied(newPosition, size))
            {
                 GameObject room = Instantiate(hallwayLeftToDown, newPosition, Quaternion.identity);
            }
                       
        }else if(xSign == -1 && zSign == 1)
        {
            Vector3 newPosition = new Vector3(xPos, 0, zPos);
            if(!IsPositionOccupied(newPosition, size))
            {
               GameObject room = Instantiate(hallwayRightToUp, newPosition, Quaternion.identity);
            }
                        
        }
        else if(xSign == -1 && zSign == -1)
        {
            Vector3 newPosition = new Vector3(xPos, 0, zPos);
            if(!IsPositionOccupied(newPosition, size))
            {
                GameObject room = Instantiate(hallwayRightToDown, newPosition, Quaternion.identity);
            }
                        
        }
            zPos += 10*zSign;

            while(targetZPos != zPos)
            {
                
                Vector3 newPosition = new Vector3(xPos + 2.5f*xSign, 0, zPos);
                GameObject room = Instantiate(hallwayZ, newPosition, Quaternion.identity);
                room.name = "hallwayZ";
                zPos += 10*zSign;
            }
}



void decideRooms(int node)
{
    Rooms[node].GetComponent<Renderer>().material.SetColor("_Color", Color.green);
    for(int i = 1; i < Rooms.Count-1; i++)
    {
        Rooms[i].GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
    }
    bool bossRoomMade = false;
    for(int i = 1; i < Rooms.Count-1; i++)
    {
        if(i > Rooms.Count-5 && bossRoomMade == false && Random.Range(1, 3) == 1)
        {
            Rooms[visitedNodes[i]].GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            bossRoomMade = true;
        }else
        {
            if(Random.Range(1, 6) == 1) Rooms[visitedNodes[i]].GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        }
        
       
    }
     if(bossRoomMade == false) Rooms[visitedNodes[Rooms.Count-1]].GetComponent<Renderer>().material.SetColor("_Color", Color.red);
}

int decideRoomType(int room_Number, int previous_Room_Code)
{
    
    int spawn_room = -1;
    int normal_room = 1;
    int item_room = 2;
    int challenge_room = 3;
    //int boss_room = 4;
    int r;
    //Debug.Log("Size of Rooms: " + visitedNodes.Count);
    switch(previous_Room_Code) 
    {
        case 0:
            Debug.Log("WE IN SPAWN");
            Rooms[visitedNodes[visitedNodes.Count-1]].GetComponent<Renderer>().material.SetColor("_Color", Color.green);
            return spawn_room;

        case -1:
            Debug.Log("this is the room right after spawn and must be normal room");
            Rooms[visitedNodes[visitedNodes.Count-1]].GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
            return normal_room;

        case 1:
            r = Random.Range(1, 9);
            if(r == 1 || r == 2)
            {
                //Debug.Log("Painted White ");
                Rooms[visitedNodes[visitedNodes.Count-1]].GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                return item_room;
            }
            if(r == 3)
            {
                Rooms[visitedNodes[visitedNodes.Count-1]].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                return challenge_room;
            } 
            break;
        case 3:
            r = Random.Range(1, 3);
            if(r == 1 || r == 2)
            {
                Rooms[visitedNodes[visitedNodes.Count-1]].GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                 return item_room;
            }
            break;
            
        default:
            break;
 
    }
    Rooms[visitedNodes[visitedNodes.Count-1]].GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
    return normal_room;
}


    class Edge
    {
        public Vector3 v0;
        public Vector3 v1;

        public Edge(Vector3 v0, Vector3 v1)
        {
            this.v0 = v0;
            this.v1 = v1;
        }
    }
      
}


