using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
    List<MeshRenderer> meshRenderers ;

    public void Awake()
    {
        ParseRooms();
        animator = GetComponent<Animator>();
        colliders = GetComponentsInChildren<Collider>();
        meshRenderers = new List<MeshRenderer>();
        MeshRenderer[] allMeshRenderers = GetComponentsInChildren<MeshRenderer>();

        foreach(MeshRenderer m in allMeshRenderers)
        {
            if(m.gameObject != gameObject) { 
                meshRenderers.Add(m);
            }
        }
    }

    public void Open()
    {
        isClosed = false;

        animator.SetBool("IsClosed", isClosed);

        foreach(Collider collider in colliders)
            collider.enabled = false;
        foreach (MeshRenderer m in meshRenderers)
            m.shadowCastingMode = ShadowCastingMode.Off;
    }

    public void Close()
    {
        isClosed = true;

        animator.SetBool("IsClosed", isClosed);

        foreach (Collider collider in colliders)
            collider.enabled = true;
        foreach (MeshRenderer m in meshRenderers)
            m.shadowCastingMode = ShadowCastingMode.On;
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
