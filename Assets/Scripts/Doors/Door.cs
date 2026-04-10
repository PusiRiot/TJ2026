using UnityEngine;

public class Door : MonoBehaviour
{
    int roomA; // first room the door belongs to
    public int RoomA { get { return roomA; } }
    int roomB; // second room the door belongs to
    public int RoomB { get { return roomB; } }

    bool isClosed = false;
    public bool IsClosed { get { return isClosed; } }

    Animator animator;
    Collider[] colliders;

    public void Awake()
    {
        ParseRooms();
        animator = GetComponent<Animator>();
        colliders = GetComponentsInChildren<Collider>();
    }

    public void Open()
    {
        isClosed = false;

        animator.SetBool("IsClosed", isClosed);

        foreach(Collider collider in colliders)
            collider.enabled = false;
    }

    public void Close()
    {
        isClosed = true;

        animator.SetBool("IsClosed", isClosed);

        foreach (Collider collider in colliders)
            collider.enabled = true;
    }

    void ParseRooms()
    {
        // Example: "wall_door R6-R5" belongs to room 6 and room 5
        string[] parts = gameObject.name.Split(' ');
        string[] rooms = parts[1].Split('-');

        roomA = int.Parse(rooms[0].Substring(1));
        roomB = int.Parse(rooms[1].Substring(1));
    }
}
