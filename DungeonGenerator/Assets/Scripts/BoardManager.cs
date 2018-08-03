using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Linq;
using UnityEngine;

public class BoardManager : MonoBehaviour
{

    public float maxHeight = 15f;
    public float minHeight = 5f;
    public float maxWidth = 29f;
    public float minWidth = 5f;
    public int numRooms = 50;
    public int margin = 5;
    public GameObject room;
    public GameObject[] debugMats;
    //public GameObject player;

    private float height, width;
    private int minX, maxX, minY, maxY;
    private GameObject[] rooms;
    private Transform boardHolder;
    //private Heuristic heuristic;
    private bool[] visitedRoom;
    private int[,] boardMaterial;

    // DEBUGMATERIAL
    private Transform matsHolder;


    // Use this for initialization
    void Awake()
    {
        //heuristic = player.GetComponent<Heuristic>();
        MakeRooms();
        RunSimulation();
        visitedRoom = new bool[numRooms];
    }

    // Update is called once per frame
    void Update()
    {
        // See displacement movement of rooms
        /*for (int i = 0; i < numRooms; i++)
        {
            rooms[i].transform.position = SnapPoint(rooms[i].transform.GetChild(0).position);
            rooms[i].transform.GetChild(0).localPosition = Vector2.zero;
        }*/

        // First Step: Puts sprites in function of its orientation and adjacency with other rooms 
        if (Input.GetKeyDown(KeyCode.Q))
        {
            foreach (GameObject room in rooms)
            {
                room.GetComponent<Room>().PutSprite();
                float _minX = room.transform.position.x - room.transform.GetChild(0).localScale.x / 2;
                float _maxX = room.transform.position.x + room.transform.GetChild(0).localScale.x / 2;
                float _minY = room.transform.position.y - room.transform.GetChild(0).localScale.y / 2;
                float _maxY = room.transform.position.y + room.transform.GetChild(0).localScale.y / 2;
                if (minX > _minX) minX = (int)_minX;
                if (maxX < _maxX) maxX = (int)_maxX;
                if (minY > _minY) minY = (int)_minY;
                if (maxY < _maxY) maxY = (int)_maxY;
            }

            minX -= margin;
            minY -= margin;
            maxX += margin;
            maxY += margin;
        }

        // Second Step: Selects a start and end room
        if (Input.GetKeyDown(KeyCode.W))
            SelectStartEnd();

        // Third Step: Displays a possible way to have different chunks of rock materials (RANDOM)
        if (Input.GetKeyDown(KeyCode.E))
            MakeMaterialBoard();

    }

    // Instantiate empty rooms in random points of an ellipse
    void MakeRooms()
    {
        boardHolder = new GameObject("Board").transform;
        rooms = new GameObject[numRooms];
        for (int i = 0; i < numRooms; i++)
        {
            GameObject _room;
            _room = Instantiate(room, GetRandomPointInEllipse(70, 10), Quaternion.identity, boardHolder) as GameObject;

            height = Mathf.Floor(Random.Range(minHeight, maxHeight));
            if (height % 2 == 0)
                height++;

            width = Mathf.Floor(Random.Range(minWidth, maxWidth));
            if (width % 2 == 0)
                width++;

            Transform collider = _room.gameObject.transform.GetChild(0);
            collider.transform.localScale += new Vector3(width, height, 0f);
            _room.GetComponent<Room>().id = i;
            _room.GetComponent<Room>().numRooms = numRooms;
            rooms[i] = _room;
        }
    }

    // Rounds down a point
    Vector2 SnapPoint(Vector2 point)
    {
        return new Vector2(Mathf.Floor(point.x), Mathf.Floor(point.y));
    }

    // Returns a ponit included within of an ellipse of given width and height 
    Vector2 GetRandomPointInEllipse(int width, int height)
    {
        float t = 2 * Mathf.PI * Random.value;
        float u = Random.value + Random.value;
        float r = 0;
        if (u > 1)
            r = 2 - u;
        else
            r = u;
        return SnapPoint(new Vector2(width * r * Mathf.Cos(t), height * r * Mathf.Sin(t)));

    }

    // Simulates the physics of the rooms to separate them
    public void RunSimulation()
    {
        Physics2D.autoSimulation = false;

        for (int i = 0; i < 1000; ++i)
        {
            Physics2D.Simulate(Time.fixedDeltaTime);
            for (int j = 0; j < numRooms; j++)
            {
                rooms[j].transform.position = SnapPoint(rooms[j].transform.GetChild(0).position);
                rooms[j].transform.GetChild(0).localPosition = Vector2.zero;
            }
            if (rooms.All(room => room.GetComponentInChildren<Rigidbody2D>().IsSleeping()))
                break;
        }

        Physics2D.autoSimulation = true;
    }

    // Returns a list of connected components of a room with ID = roomID
    List<int> ExamineConnexComponent(int roomID)
    {
        List<int> connectedComponent = new List<int>();
        if (!visitedRoom[roomID])
        {
            visitedRoom[roomID] = true;
            connectedComponent = new List<int> { roomID };
            List<int> adjacentRooms = rooms[roomID].GetComponent<Room>().adjacentRoomsID;
            foreach (int adjacentRoom in adjacentRooms)
                connectedComponent.AddRange(ExamineConnexComponent(adjacentRoom));
        }
        return connectedComponent;
    }

    // Selects two rooms of the biggest connected component. The room with highest value of y will be the initial room
    private void SelectStartEnd()
    {
        List<List<int>> connectedComponents = new List<List<int>>();

        for (int i = 0; i < numRooms; i++)
        {
            if (!visitedRoom[i])
                connectedComponents.Add(ExamineConnexComponent(i));
        }

        int maxSize = 0;
        int maxSizeID = 0;
        for (int i = 0; i < connectedComponents.Count; ++i)
        {
            if (connectedComponents[i].Count > maxSize)
            {
                maxSize = connectedComponents[i].Count;
                maxSizeID = i;
            }
        }

        List<int> biggestComponent = connectedComponents[maxSizeID];

        int it = 100;
        Vector2 startRoom, endRoom;
        startRoom = endRoom = Vector2.zero;
        int idRoom, idRoom2;
        idRoom = idRoom2 = 0;
        while (it > 0 && startRoom == Vector2.zero)
        {
            idRoom = biggestComponent[Random.Range(0, biggestComponent.Count)];
            startRoom = rooms[idRoom].GetComponent<Room>().GetWideFloor();

            --it;
        }
        while (it > 0 && endRoom == Vector2.zero)
        {
            idRoom2 = biggestComponent[Random.Range(0, biggestComponent.Count)];
            if (idRoom2 != idRoom)
                endRoom = rooms[idRoom2].GetComponent<Room>().GetWideFloor();
            --it;
        }

        // The room with highest value of y will be the initial room
        if (endRoom.y > startRoom.y)
        {
            Vector2 aux = startRoom;
            startRoom = endRoom;
            endRoom = aux;
            int auxID = idRoom;
            idRoom = idRoom2;
            idRoom2 = auxID;
        }

        rooms[idRoom].GetComponent<Room>().EnableDebugSprite();
        rooms[idRoom2].GetComponent<Room>().EnableDebugSprite();
    }

    private class FloodingPoint
    {
        public int x;
        public int y;
        public int material;

        public FloodingPoint(int _x, int _y, int _material)
        {
            x = _x;
            y = _y;
            material = _material;
        }

    }

    // Displays a random way to make chunks of rock material using a flooding method
    void MakeMaterialBoard()
    {
        matsHolder = new GameObject("Materials").transform;
        boardMaterial = new int[Mathf.Abs(minY) + Mathf.Abs(maxY), Mathf.Abs(minX) + Mathf.Abs(maxX)];
        int numMaterials = 6;
        Queue<FloodingPoint> floodingPoints = new Queue<FloodingPoint>();

        for (int i = 1; i <= numMaterials; ++i)
        {
            float numSources = Random.Range(150, 250);

            for (int j = 0; j < numSources; ++j)
                floodingPoints.Enqueue(new FloodingPoint((int)Random.Range(0, boardMaterial.GetLength(1)-1), (int)Random.Range(0, boardMaterial.GetLength(0)-1), i));

        }

        while (floodingPoints.Count > 0)
        {
            FloodingPoint fp = floodingPoints.Dequeue();
            if (boardMaterial[fp.y, fp.x] == 0)
            {
                Instantiate(debugMats[fp.material - 1], new Vector2(fp.x + minX, fp.y + minY), Quaternion.identity, matsHolder);

                boardMaterial[fp.y, fp.x] = fp.material;
                if (fp.x - 1 > -1) floodingPoints.Enqueue(new FloodingPoint(fp.x - 1, fp.y, fp.material));
                if (fp.x + 1 < boardMaterial.GetLength(1)) floodingPoints.Enqueue(new FloodingPoint(fp.x + 1, fp.y, fp.material));
                if (fp.y - 1 > -1) floodingPoints.Enqueue(new FloodingPoint(fp.x, fp.y - 1, fp.material));
                if (fp.y + 1 < boardMaterial.GetLength(0)) floodingPoints.Enqueue(new FloodingPoint(fp.x, fp.y + 1, fp.material));
            }
        }

    }



}
