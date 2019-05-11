using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public GameObject exitPortal;
    public GameObject mazeWall; //very primitive
    //public GameObject fill;
    //public GameObject player;

    // Trap public variables
    public int trapNumber;
    public GameObject trapPrefab;
    public GameObject[] corridorTraps; 
    public GameObject[] roomTraps;

    public GameObject ceilingTile;

    public int rows;
    public int cols;
    public int trapPercentage;  //helps determine how many traps to randomly spawn
    public int trapSpacerX; //bigger number is farther horizontal spacing
    public int trapSpacerY; //bigger number is farther vertical spacing
    public int corridorSpawnBoost;  //higher number means higher chance of spawning in a corridor

    public static byte[,] mazeArray;
    private List<Room> roomList;

    void Awake()
    {
        mazeArray = new byte[rows, cols];
        roomList = new List<Room>();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (col % 2 == 0 || row % 2 == 0)
                    mazeArray[row, col] = 1;                
            }
        }

        GenerateMaze();

        SpawnTraps();

        CreateExit();

        CreateCeiling();
    }

    private void GenerateMaze()
    {
        int trials = 0;
        int crashCheck = 0;

        while (trials++ < 20 * rows)
            GenerateRoom();

        GenerateTunnels();       
        OpenMainRoom();

        while (roomList.Count != 0 && crashCheck++ <= 100) 
        {
            for (int x = 0; x < roomList.Count; x++)
                ConnectRoom();
            ConnectTunnels();
        }

        ConnectTunnels();

        FillDeadEnds(FindDeadEnds());

        DeleteSingles();

        SpawnMaze(); //used for 2d maze generation

        CheckWalls();

        PrintArray(mazeArray);
        Debug.Log(crashCheck); 
    }

    private void GenerateRoom()
    {
        List<int> randomRows = new List<int>();
        List<int> randomCols = new List<int>();

        int maxRoomRows = mazeArray.GetLength(0) / 10;
        if (maxRoomRows % 2 == 1)
            maxRoomRows += 1;

        int maxRoomCols = mazeArray.GetLength(1) / 10;
        if (maxRoomCols % 2 == 1)
            maxRoomCols += 1;

        for (int i = 2; i <= maxRoomRows; i += 2)
            randomRows.Add(i);

        for (int i = 2; i <= maxRoomCols; i += 2)
            randomCols.Add(i);

        int roomRows = randomRows[UnityEngine.Random.Range(0, randomRows.Count)]; //dimensions are how many elements a room's width and height from origin is
        int roomCols = randomCols[UnityEngine.Random.Range(0, randomCols.Count)];

        List<int> xList = new List<int>();
        List<int> yList = new List<int>();

        for (int i = 1 + ((roomRows - 1) * 2); i <= mazeArray.GetLength(0) - (roomRows * 2); i += 2) //finds a suitable random origin for the room that won't go past bounds of maze
            xList.Add(i);

        for (int i = 1 + ((roomCols - 1) * 2); i <= mazeArray.GetLength(1) - (roomCols * 2); i += 2)
            yList.Add(i);

        int xOrigin = xList[UnityEngine.Random.Range(0, xList.Count)];
        int yOrigin = yList[UnityEngine.Random.Range(0, yList.Count)];

        for (int row = xOrigin - roomRows; row <= xOrigin + roomRows; row++)
        {
            for (int col = yOrigin - roomCols; col <= yOrigin + roomCols; col++)
            {
                if (mazeArray[row, col] == 3)
                    return;
            }
        }

        for (int row = xOrigin - roomRows; row <= xOrigin + roomRows; row++)
        {
            for (int col = yOrigin - roomCols; col <= yOrigin + roomCols; col++)
            {
                mazeArray[row, col] = 3;
            }
        }

        roomList.Add(new Room(new Pair(xOrigin - roomRows, yOrigin - roomCols),
                              new Pair(xOrigin - roomRows, yOrigin + roomCols),
                              new Pair(xOrigin + roomRows, yOrigin - roomCols),
                              new Pair(xOrigin + roomRows, yOrigin + roomCols)));
    }

    private void GenerateTunnels()
    {
        for (int row = 1; row <= mazeArray.GetLength(0) - 2; row += 2)
        {
            for (int col = 1; col <= mazeArray.GetLength(1) - 2; col += 2)
            {
                if (mazeArray[row, col] == 0)
                {
                    mazeArray[row, col] = 2;
                    Dig(row, col, FindDirections(row, col, 2));
                }
            }
        }
    }

    private void Dig(int row, int col, char[] directions)
    {
        while (directions.Length > 0)
        {
            char selection = directions[UnityEngine.Random.Range(0, directions.Length)];

            if (selection.Equals('L'))
            {
                mazeArray[row, col - 1] = 2;
                mazeArray[row, col - 2] = 2;
                Dig(row, col - 2, FindDirections(row, col - 2, 2));
            }
            else if (selection.Equals('R'))
            {
                mazeArray[row, col + 1] = 2;
                mazeArray[row, col + 2] = 2;
                Dig(row, col + 2, FindDirections(row, col + 2, 2));
            }
            else if (selection.Equals('U'))
            {
                mazeArray[row - 1, col] = 2;
                mazeArray[row - 2, col] = 2;
                Dig(row - 2, col, FindDirections(row - 2, col, 2));
            }
            else if (selection.Equals('D'))
            {
                mazeArray[row + 1, col] = 2;
                mazeArray[row + 2, col] = 2;
                Dig(row + 2, col, FindDirections(row + 2, col, 2));
            }

            directions = FindDirections(row, col, 2);          
        }
    }

    private char[] FindDirections(int row, int col, int spaces)
    {
        string directionSet = "";

        if (col - spaces >= 0 && mazeArray[row, col - spaces] == 0)
            directionSet += "L";
        if (col + spaces < mazeArray.GetLength(1) && mazeArray[row, col + spaces] == 0)
            directionSet += "R";
        if (row - spaces >= 0 && mazeArray[row - spaces, col] == 0)
            directionSet += "U";
        if (row + spaces < mazeArray.GetLength(0) && mazeArray[row + spaces, col] == 0)
            directionSet += "D";

        return directionSet.ToCharArray();
    }

    private void SpawnMaze()
    {
        //float bounds = rows / 2;

        for (int row = 0; row < mazeArray.GetLength(0); row++)
        {
            for (int col = 0; col < mazeArray.GetLength(1); col++)
            {
                if (mazeArray[row, col] == 1)
                    Instantiate(mazeWall, new Vector3(0.5f + col, 1, 0.5f + row ), Quaternion.identity);
                /*
                else if (mazeArray[row, col] == 0)
                    Instantiate(fill, new Vector2(-bounds + col, bounds - row), Quaternion.identity);

                if (row == 1 && col == 1)
                   Instantiate(player, new Vector2(-bounds + col, bounds - row), Quaternion.identity);
                */
            }
        }
    }

    private void FindRoomConnectors(Room roomObj)
    {
        for (int row = roomObj.topLeft.first - 1; row <= roomObj.bottomLeft.first + 1; row++)
        {
            for (int col = roomObj.topLeft.second - 1; col <= roomObj.topRight.second + 1; col++)
            {
                if (mazeArray[row, col] == 1 && row <= mazeArray.GetLength(0) - 2 && col <= mazeArray.GetLength(1) - 2 && row != 0 && col != 0) //if this tile is a wall
                {
                    if ((mazeArray[row + 1, col] >= 2 && mazeArray[row - 1, col] >= 2) || (mazeArray[row, col + 1] >= 2 && mazeArray[row, col - 1] >= 2)) //if UP and DOWN or LEFT and RIGHT are generated rooms or the path
                        roomObj.connectors.Add(new Pair(row, col));
                }
            }
        }
    }

    private void PrintArray(byte[,] outputArray)
    {
        string mazeString = "";

        for (int row = 0; row < outputArray.GetLength(0); row++)
        {
            for (int col = 0; col < outputArray.GetLength(1); col++)
                mazeString += outputArray[row, col] + " ";

            mazeString += "\n";
        }

        Debug.Log(mazeString);
    }

    private void FloodFill(int row, int col)
    {
        Stack<Pair> exploration = new Stack<Pair>();
        exploration.Push(new Pair(row, col));

        while (exploration.Count > 0)
        {
            Pair cell = exploration.Pop();

            int x = cell.first;
            int y = cell.second;

            if (mazeArray[x + 1, y] > 1)
            {
                mazeArray[x + 1, y] = 0;
                exploration.Push(new Pair(x + 1, y));
            }
            if (mazeArray[x, y + 1] > 1)
            {
                mazeArray[x, y + 1] = 0;
                exploration.Push(new Pair(x, y + 1));
            }
            if (mazeArray[x - 1, y] > 1)
            {
                mazeArray[x - 1, y] = 0;
                exploration.Push(new Pair(x - 1, y));
            }
            if (mazeArray[x, y - 1] > 1)
            {
                mazeArray[x, y - 1] = 0;
                exploration.Push(new Pair(x, y - 1));
            }
        }
    }

    private void OpenMainRoom()
    {
        bool roomSelected = false;

        foreach (Room obj in roomList)
            FindRoomConnectors(obj);

        Room mainRoom = roomList[UnityEngine.Random.Range(0, roomList.Count)];

        foreach (Pair openConnector in SelectConenctors(mainRoom))
        {
            mainRoom.connectors.Remove(openConnector);
            int x = openConnector.first;
            int y = openConnector.second;

            if ((mazeArray[x + 1, y] >= 2 && mazeArray[x - 1, y] >= 2 && mazeArray[x + 1, y] != mazeArray[x - 1, y]) || (mazeArray[x, y + 1] >= 2 && mazeArray[x, y - 1] >= 2 && mazeArray[x, y + 1] != mazeArray[x, y - 1]))
            {
                mazeArray[x, y] = 0;
                roomList.Remove(mainRoom);
                FloodFill(x, y);
                roomSelected = true;
            }
        }

        if (!roomSelected)
            OpenMainRoom();
    }

    private List<Pair> SelectConenctors(Room roomObj) //remove break to add more paths
    {
        List<Pair> connectors = new List<Pair>();
        int multiplier = 1;

        while (UnityEngine.Random.Range(0f, 1f) <= 1f / multiplier && roomObj.connectors.Count != 0)
        {
            Pair selection = roomObj.connectors[UnityEngine.Random.Range(0, roomObj.connectors.Count)];
            connectors.Add(selection);
            multiplier++;
            //break;
        }

        return connectors;
    }

    private void ConnectRoom()
    {
        Room mainRoom = roomList[UnityEngine.Random.Range(0, roomList.Count)];

        if (mainRoom.connectors.Count > 0)
        {
            foreach (Pair openConnector in SelectConenctors(mainRoom))
            {
                mainRoom.connectors.Remove(openConnector);
                int x = openConnector.first;
                int y = openConnector.second;

                if (mazeArray[x + 1, y] == 0 || mazeArray[x - 1, y] == 0 || mazeArray[x, y + 1] == 0 || mazeArray[x, y - 1] == 0)
                {
                    mazeArray[x, y] = 0;
                    roomList.Remove(mainRoom);
                    FloodFill(x, y);
                }
            }
        }
    }

    private void ConnectTunnels() //empty try-catches because too lazy to add conditionals to check mazeBounds 
    {                             //will fix later I promise
        List<Pair> corridors = new List<Pair>();

        for (int row = 1; row < mazeArray.GetLength(0); row++)
        {
            for (int col = 1; col < mazeArray.GetLength(1); col++)
            {
                if (mazeArray[row, col] == 2)
                {
                    try
                    {
                        if (mazeArray[row + 2, col] == 0)
                            corridors.Add(new Pair(row + 1, col));
                    }
                    catch
                    {

                    }
                    try
                    {
                        if (mazeArray[row - 2, col] == 0)
                            corridors.Add(new Pair(row - 1, col));
                    }
                    catch
                    {

                    }
                    try
                    {
                        if (mazeArray[row, col + 2] == 0)
                            corridors.Add(new Pair(row, col + 1));
                    }
                    catch
                    {

                    }
                    try
                    {
                        if (mazeArray[row, col - 2] == 0)
                            corridors.Add(new Pair(row, col - 1));
                    }
                    catch
                    {

                    }
                }
            }
        }

        if (corridors.Count != 0)
        {
            Pair selection = corridors[UnityEngine.Random.Range(0, corridors.Count)];

            mazeArray[selection.first, selection.second] = 0;
            FloodFill(selection.first, selection.second);
        }
    }

    private List<Pair> FindDeadEnds()
    {
        List<Pair> deadEnds = new List<Pair>();

        for (int row = 1; row < mazeArray.GetLength(0); row++)
        {
            for (int col = 1; col < mazeArray.GetLength(1); col++)
            {
                if (row == 1 && col == 1)
                    continue;
                else if (mazeArray[row, col] != 1 && FindDirections(row, col, 1).Length == 1)
                    deadEnds.Add(new Pair(row, col));
            }
        }

        return deadEnds;
    }

    private void FillDeadEnds(List<Pair> deadEnds)
    {
        List<Pair> newEnds = new List<Pair>();

        for (int i = deadEnds.Count - 1; i >= 0; i--)
        {
            char[] directions = FindDirections(deadEnds[i].first, deadEnds[i].second, 1);
            
            if (directions.Length == 1)
            {
                mazeArray[deadEnds[i].first, deadEnds[i].second] = 1;

                if (directions[0].Equals('L'))
                    newEnds.Add(new Pair(deadEnds[i].first, deadEnds[i].second - 1));
                else if (directions[0].Equals('R'))
                    newEnds.Add(new Pair(deadEnds[i].first, deadEnds[i].second + 1));
                else if (directions[0].Equals('U'))
                    newEnds.Add(new Pair(deadEnds[i].first - 1, deadEnds[i].second));
                else if (directions[0].Equals('D'))
                    newEnds.Add(new Pair(deadEnds[i].first + 1, deadEnds[i].second));
            }
        }
           
        if (newEnds.Count != 0)
            FillDeadEnds(newEnds);
    }

    private void DeleteSingles()
    {
        for (int row = 2; row < mazeArray.GetLength(0) - 2; row++)
        {
            for (int col = 2; col < mazeArray.GetLength(1) - 2; col++)
            {
                if (mazeArray[row - 1, col] == 0 && mazeArray[row + 1, col] == 0 && mazeArray[row, col - 1] == 0 && mazeArray[row, col + 1] == 0 && mazeArray[row, col] == 1)
                {
                    mazeArray[row, col] = 0;
                    //Debug.LogFormat("Wall removed at {0}", new Pair(row, col));
                }
            }
        }
    }

    /* Bracket means you are checking at this maze wall
     *  0  0  0         0  1  0         0  0  0         0  1  0
     *  0 [1] 1         0 [1] 1         1 [1] 0         1 [1] 0
     *  0  1  0         0  0  0         0  1  0         0  0  0    
     *  Corner = 2      Corner = 3      Corner = 4      Corner = 5
     *  
     *  0  0  0         0  1  0         0  1  0         0  1  0
     *  1 [1] 1         1 [1] 1         0 [1] 1         1 [1] 0
     *  0  1  0         0  0  0         0  1  0         0  1  0
     *  T-Sect. = 6     T-Sect. = 7     T-Sect. = 8     T-Sect. = 9
     */ 

    private void CheckWalls()
    {
        mazeArray[0, 0] = 2;
        mazeArray[0, mazeArray.GetLength(1) - 1] = 4;
        mazeArray[mazeArray.GetLength(0) - 1, 0] = 3;
        mazeArray[mazeArray.GetLength(0) - 1, mazeArray.GetLength(1) - 1] = 5;

        for (int row = 0; row < mazeArray.GetLength(0); row++)
        {
            for (int col = 0; col < mazeArray.GetLength(1); col++)
            {
                if (mazeArray[row, col] == 2 || mazeArray[row, col] == 3 || mazeArray[row, col] == 4 || mazeArray[row, col] == 5)
                    continue;
                else if (row == 0)
                {
                    if (mazeArray[row + 1, col] >= 1 && mazeArray[row + 1, col + 1] == 0 && mazeArray[row + 1, col - 1] == 0)
                        mazeArray[row, col] = 6;
                }
                else if (row == mazeArray.GetLength(0) - 1)
                {
                    if (mazeArray[row - 1, col] >= 1 && mazeArray[row - 1, col + 1] == 0 && mazeArray[row - 1, col - 1] == 0) 
                        mazeArray[row, col] = 7;
                }
                else if (col == 0)
                {
                    if (mazeArray[row, col + 1] >= 1 && mazeArray[row + 1, col + 1] == 0 && mazeArray[row - 1, col + 1] == 0)
                        mazeArray[row, col] = 8;
                }
                else if (col == mazeArray.GetLength(1) - 1)
                {
                    if (mazeArray[row, col - 1] >= 1 && mazeArray[row + 1, col - 1] == 0 && mazeArray[row - 1, col - 1] == 0)
                        mazeArray[row, col] = 9;
                }
            }
        }
    }


    private void SpawnTraps()  //maybe a bait trap will only appear in rooms
    {
        int trapCount = trapSpacerX;
        List<int> trapLocations = new List<int>();
        for(int i = 0; i < rows; i++)
        {
            for(int j = 0; j < cols; j++)
            {
                if(mazeArray[i, j] == 0 && trapCount >= trapSpacerX && trapLocations.Contains(i) == false)
                {
                    if(isCorridor(i, j) && !isCorner(i, j))
                    {
                        if((trapPercentage + corridorSpawnBoost) > Random.Range(0, 101))
                        {
                            Instantiate(corridorTraps[Random.Range(0, corridorTraps.Length)], new Vector3(0.5f + j, 0, 0.5f + i), Quaternion.identity);
                            mazeArray[i,j] = 1;
                            trapLocations.Add(j);
                            trapCount = 0;
                        }
                    }
                    else if(!isCorridor(i, j) && !isCorner(i, j))
                    {
                        if(trapPercentage > Random.Range(0, 101))
                        {
                            Instantiate(roomTraps[Random.Range(0, corridorTraps.Length)], new Vector3(0.5f + j, 0, 0.5f + i), Quaternion.identity);
                            mazeArray[i,j] = 1;
                            trapLocations.Add(j);
                            trapCount = 0;
                        }
                    }
                }
                trapCount++;
            }
            if(i % trapSpacerY == 0)     //resets the trapLocations list in publicly specified intervals
            {
                trapLocations.Clear();
            }
        }
    }
    /* 
    private void SpawnTraps()
    {
        List<Pair> emptyCells = FindEmptyCells();

        for (int count = 0; count < trapNumber; ++count)
        {
            // If there are no more empty cells, stop spawning traps.
            // Ideally, this would never happen because then the player
            // is fucked.
            if (emptyCells.Count == 0)
                break;

            int index = Random.Range(0, emptyCells.Count);
            Vector3 trapPosition = new Vector3(emptyCells[index].second, 0, emptyCells[index].first);

            Instantiate(trapPrefab, trapPosition, Quaternion.identity);

            // Remove the new used trap location from emptyCells.
            emptyCells.RemoveAt(index);
        }
    }
    */
/*
    private List<Pair> FindEmptyCells()
    {
        List<Pair> result = new List<Pair>();

        for (int row = 0; row < rows; ++row)
        {
            for (int col = 0; col < cols; ++col)
            {
                if (mazeArray[row, col] == 0)
                {
                    result.Add(new Pair(row, col));
                }
            }
        }

        return result;
    }
    */

    private void CreateExit()
    {
        int randCol;
        int randRow;
        while(true) //for testing
        {
            randCol = UnityEngine.Random.Range((int)((cols/2) - (0.2 * cols)), (int)((cols/2) + (0.2 * cols)));
            randRow = UnityEngine.Random.Range((int)((rows/2) - (0.2 * rows)), (int)((rows/2) + (0.2 * rows)));
            if(mazeArray[randRow, randCol] == 0)
            {
                if(!isCorridor(randRow, randCol) && !isCorner(randRow, randCol))
                {
                    Instantiate(exitPortal, new Vector3(0.5f + randCol, 1, 0.5f + randRow ), Quaternion.identity);
                    break;
                }
            }
        }
    }

    private bool isCorridor(int x, int y)  //returns true if piece is a corridor
    {
        if(mazeArray[x + 1, y] != 0)
        {
            if(mazeArray[x - 1, y] != 0)
            {
                return true;
            }
        }
        if(mazeArray[x, y + 1] != 0)
        {
            if(mazeArray[x, y - 1] != 0)
            {
                return true;
            }
        }
        return false;
    }

    private bool isCorner(int x, int y)  //returns true if piece is a corner piece
    {
        if(mazeArray[x + 1, y] != 0)
        {
            if(mazeArray[x, y + 1] != 0)
            {
                return true;
            }
        }
        if(mazeArray[x - 1, y] != 0)
        {
            if(mazeArray[x, y - 1] != 0)
            {
                return true;
            }
        }
        if(mazeArray[x - 1, y] != 0)
        {
            if( mazeArray[x, y + 1] != 0)
            {
                return true;
            }
        }
        if(mazeArray[x + 1, y] != 0)
        {
            if(mazeArray[x, y - 1] != 0)
            {
                return true;
            }
        }
        return false;
    }

    public void CreateCeiling()
    {
        for(int i = 0; i < rows; i++)
        {
            for(int j = 0; j < cols; j++)
            {
                Instantiate(ceilingTile, new Vector3(0.5f + i, 2, 0.5f + j ), Quaternion.identity);
            }
        }
    }
}

