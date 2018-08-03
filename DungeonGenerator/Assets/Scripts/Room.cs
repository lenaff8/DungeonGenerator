using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {


    public GameObject[] tileGameObject;
    public LayerMask mask;
    public int id;
    public int numRooms;

    private Transform floor;
    public List<int> adjacentRoomsID;

    // Use this for initialization
    void Start () {
        floor = gameObject.transform.GetChild(1);
    }

    // Puts sprites arround the room to delimitate it. If there's a adjacent room, it doesn't put sprite so they can comunicate
    public void PutSprite()
    {
        bool[] adjacentRooms = new bool[numRooms];

        gameObject.GetComponentInChildren<BoxCollider2D>().enabled = false;
        gameObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
        int tilesX = (int)gameObject.transform.GetChild(0).localScale.x;
        int tilesY = (int)gameObject.transform.GetChild(0).localScale.y;

        float maxX, minX, maxY, minY;
        maxX = gameObject.transform.position.x + (tilesX - 1) / 2.0f;
        minX = gameObject.transform.position.x - (tilesX - 1) / 2.0f;
        maxY = gameObject.transform.position.y + (tilesY - 1) / 2.0f;
        minY = gameObject.transform.position.y - (tilesY - 1) / 2.0f;

        // Collision array
        Vector2[] directionCollision = new[] { new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, -1), new Vector2(-1, 1)  };
        Vector2[] directionCollisionCorner = new[] {  Vector2.right, Vector2.down, Vector2.left, Vector2.up };

        // Beginning
        Vector2 origin = new Vector2(minX, maxY);
        RaycastHit2D[] collisions = new RaycastHit2D[] {    Physics2D.Raycast(origin, transform.TransformDirection(Vector2.left), 1.5f, mask),
                                                            Physics2D.Raycast(origin, transform.TransformDirection(directionCollision[3]), 1.5f, mask),
                                                            Physics2D.Raycast(origin, transform.TransformDirection(Vector2.up), 1.5f, mask)
                                                        };

        bool emptyFirstCorner = false;

        bool auxCol2 = Physics2D.Raycast(origin, transform.TransformDirection(directionCollision[2]), 1.5f, mask);
        bool auxCol1 = Physics2D.Raycast(new Vector2(minX, maxY-1), transform.TransformDirection(directionCollision[2]), 1.5f, mask);
        GameObject lastTile = null;
        Vector2 lastOrigin = origin;
        bool recentCorner;

        for (int i = 0; i < 4; i++)
        {
            RaycastHit2D next = Physics2D.Raycast(origin, transform.TransformDirection(directionCollision[i]), 1.5f, mask);
            if (collisions[0])
            {
                if (collisions[2])
                {
                    if (!collisions[1])
                        lastTile = Instantiate(tileGameObject[(4 + i * 2) % 8], origin, Quaternion.identity, floor);
                    else  
                    {
                        if (auxCol2 && !next)
                        {
                            Instantiate(tileGameObject[(6 + i * 2) % 8], origin, Quaternion.identity, floor);
                            lastTile = null;
                        }
                        else if (!auxCol2 && next)
                            lastTile = Instantiate(tileGameObject[(2 + i * 2) % 8], origin, Quaternion.identity, floor);
                             
                        else // There's a connexion with a room
                        {
                            if (i == 0)
                                emptyFirstCorner = true;
                            lastTile = null;
                            int idRoom = collisions[1].collider.gameObject.transform.parent.gameObject.GetComponent<Room>().id;
                            if (!adjacentRooms[idRoom])
                            {
                                adjacentRooms[idRoom] = true;
                                adjacentRoomsID.Add(idRoom);
                            }
                        }
                        
                    }
                }
                else
                {
                    if (auxCol2)
                    {
                        if (auxCol1)
                        {
                            Instantiate(tileGameObject[(5 + i * 2) % 8], origin, Quaternion.identity, floor);
                            lastTile = null;
                        }
                        else if (lastTile != null)
                        {
                            Destroy(lastTile);
                            Instantiate(tileGameObject[(3 + i * 2) % 8], lastOrigin, Quaternion.identity, floor);
                            lastTile = null;
                        }
                    }
                    else 
                        lastTile = null;
                }
            }
            else if (collisions[2])
                lastTile = Instantiate(tileGameObject[(3 + i * 2) % 8], origin, Quaternion.identity, floor);
            else
            {
                if (lastTile != null)
                {
                    Destroy(lastTile);
                    Instantiate(tileGameObject[(3 + i * 2) % 8], lastOrigin, Quaternion.identity, floor);
                }
                lastTile = null;
            }

            collisions[0] = collisions[1];
            collisions[1] = collisions[2];
            collisions[2] = next;

            float toPosX = i < 2 ? maxX : minX;
            float toPosY = i % 3 == 0 ? maxY : minY;

            lastOrigin = origin;
            recentCorner = true;
            if (origin.y < toPosY) ++origin.y;
            else if (origin.y > toPosY) --origin.y;
            if (origin.x < toPosX) ++origin.x;
            else if (origin.x > toPosX) --origin.x;

            do
            {
                do
                {
                    collisions[0] = collisions[1];
                    collisions[1] = collisions[2];
                    collisions[2] = Physics2D.Raycast(origin, transform.TransformDirection(directionCollision[i]), 1.5f, mask);

                    if (collisions[1]) { 
                        if (collisions[0]) { 
                            if (!collisions[2]) { 
                                if (lastTile != null)
                                {
                                    Destroy(lastTile);
                                    Instantiate(tileGameObject[(5 + i * 2) % 8], origin, Quaternion.identity, floor);
                                    lastTile = null;
                                }
                                else
                                {
                                    Instantiate(tileGameObject[(6 + i * 2) % 8], origin, Quaternion.identity, floor);
                                    lastTile = null;
                                }

                            }
                            else // There's a connexion with a room
                            {
                                lastTile = null;

                                int idRoom = collisions[1].collider.gameObject.transform.parent.gameObject.GetComponent<Room>().id;
                                if (!adjacentRooms[idRoom])
                                {
                                    adjacentRooms[idRoom] = true;
                                    adjacentRoomsID.Add(idRoom);
                                }
                            }
                        }
                        else if (collisions[2])
                            lastTile = Instantiate(tileGameObject[(4 + i * 2) % 8], origin, Quaternion.identity, floor);
                    }
                    else
                    {
                        if (recentCorner && lastTile != null)
                            Destroy(lastTile);
                            
                        Instantiate(tileGameObject[(5 + i * 2) % 8], origin, Quaternion.identity, floor);
                        lastTile = null;
                    }

                    lastOrigin = origin;
                    recentCorner = false;

                    if (origin.y < toPosY) ++origin.y;
                    else if (origin.y > toPosY) --origin.y;
                    
                } while (origin.y != toPosY);

                if (origin.x < toPosX) ++origin.x;
                else if (origin.x > toPosX) --origin.x;

            } while (origin.x != toPosX);

            auxCol1 = collisions[0];
            auxCol2 = collisions[1];
            collisions[0] = collisions[2];
            collisions[1] = Physics2D.Raycast(origin, transform.TransformDirection(directionCollision[i]), 1.5f, mask);
            collisions[2] = Physics2D.Raycast(origin, transform.TransformDirection(directionCollisionCorner[i]), 1.5f, mask);

            
        }
        if (lastTile && !emptyFirstCorner)
        {
            Destroy(lastTile);
            Instantiate(tileGameObject[3], lastOrigin, Quaternion.identity, floor);
        }
        gameObject.GetComponentInChildren<BoxCollider2D>().enabled = true;

    }

    // Returns if there's three consecutive positions where there's floor below
    public Vector2 GetWideFloor()
    {
        gameObject.GetComponentInChildren<BoxCollider2D>().enabled = false;
        int tilesX = (int)gameObject.transform.GetChild(0).localScale.x;
        int tilesY = (int)gameObject.transform.GetChild(0).localScale.y;

        float maxX;
        maxX = (gameObject.transform.position.x + (tilesX - 1) / 2.0f);

        Vector2 origin = new Vector2((gameObject.transform.position.x - (tilesX - 1) / 2.0f) + 1, (gameObject.transform.position.y - (tilesY - 1) / 2.0f) + 1);

        int terrain = 0;
        while (origin.x < maxX)
        {
            Debug.DrawRay(origin, transform.TransformDirection(Vector2.down), Color.yellow, 10);

            if (Physics2D.Raycast(origin, transform.TransformDirection(Vector2.down), 1))
            {
                if (terrain == 2)
                    return origin;
                ++terrain;
            }
            else terrain = 0;
            ++origin.x;
        }
        gameObject.GetComponentInChildren<BoxCollider2D>().enabled = true;

        return Vector2.zero;

    }

    // Enables the DEBUG sprite of the room
    public void EnableDebugSprite()
    {
        gameObject.GetComponentInChildren<SpriteRenderer>().enabled = true;
    }
}
