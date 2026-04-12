using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Door : MonoBehaviour
{
    const float animationStaticPartDuration = 1.0f;
    const float animationDynamicPartDuration = 1.0f;

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
        StartCoroutine(ToggleCollider(isClosed));
    }

    public void Close()
    {
        isClosed = true;

        animator.SetBool("IsClosed", isClosed);
        StartCoroutine(ToggleCollider(isClosed));
    }

    void ParseRooms()
    {
        // Example: "wall_door R6-R5" belongs to room 6 and room 5
        string[] parts = gameObject.name.Split(' ');
        string[] rooms = parts[1].Split('-');

        roomA = int.Parse(rooms[0].Substring(1));
        roomB = int.Parse(rooms[1].Substring(1));
    }

    IEnumerator ToggleCollider(bool enable)
    {
        yield return new WaitForSeconds(animationStaticPartDuration);
        //Audio
        AkUnitySoundEngine.PostEvent("Play_Doors", gameObject);
        foreach (Collider collider in colliders)
            collider.enabled = enable;

        if (enable)
        {
            foreach (MeshRenderer m in meshRenderers)
                m.shadowCastingMode = ShadowCastingMode.On;
        }

        yield return new WaitForSeconds(animationDynamicPartDuration);
        
        if(!enable)
        {
            foreach (MeshRenderer m in meshRenderers)
                m.shadowCastingMode = ShadowCastingMode.Off;
        }
        
    }
}
