using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum DoorEventState
{
    Base,
    BiasToClose,
    BiasToOpen,
    ForceClose,
    ForceOpen
}

public class DoorManager : MonoBehaviour
{
    [SerializeField] GameObject doorsContainer;
    List<Door> doors;
    Dictionary<int, List<Door>> roomsMapHelper = new();
    Dictionary<Door, List<Door>> connectivityGraph = new (); // graph where each door is a node and the edges are when doors are in the same room
    DoorEventState currentState;

    float _baseDoorRandom = 0.5f;
    float _biasToCloseDoorRandom = 0.3f;
    float _biasToOpenDoorRandom = 0.7f;

    int _minRandomTime = 3;
    int _maxRandomTime = 10;

    const int MAX_CLOSE_DOOR_TRIES = 5;

    private void Start()
    {
        if (doorsContainer == null)
            throw new System.Exception("No doors container assigned on DoorsManager");

        doors = doorsContainer.GetComponentsInChildren<Door>().ToList();

        BuildRoomsDictionary();

        BuildDoorGraph();

        StartCoroutine(DoorRoutine());
    }

    IEnumerator DoorRoutine()
    {
        while (true)
        {
            float wait = UnityEngine.Random.Range(_minRandomTime, _maxRandomTime);
            yield return new WaitForSeconds(_minRandomTime);

            List<Door> openedDoors = doors.Where(d => !d.IsClosed).ToList();
            List<Door> closedDoors = doors.Where(d => !d.IsClosed).ToList();

            bool opened = false;

            if (openedDoors.Count == 0)
            {
                OpenDoor(closedDoors);
                opened = true;
            } 
            else if (closedDoors.Count == 0)
            {
                CloseDoor(openedDoors.Count, openedDoors); // it cannot close a door so it has to try until it can
            }
            else if (ShouldOpenDoor())
            {
                OpenDoor(openedDoors);
                opened = true;
            }
            else
            {
                bool closed = CloseDoor(MAX_CLOSE_DOOR_TRIES, openedDoors); // limited number of tries to close a door that leaves path connected

                if (!closed)
                { // if tried to many times to close a door just open one
                    OpenDoor(openedDoors);
                    opened = true;
                }
            }

            UpdateState(opened);
        }
    }

    bool ShouldOpenDoor()
    {
        float r = UnityEngine.Random.value;

        switch (currentState)
        {
            case DoorEventState.Base:
                return r < _baseDoorRandom;

            case DoorEventState.BiasToClose:
                return r < _biasToCloseDoorRandom;

            case DoorEventState.BiasToOpen:
                return r < _biasToOpenDoorRandom;

            case DoorEventState.ForceClose:
                return false;

            case DoorEventState.ForceOpen:
                return true;
        }

        return false;
    }

    void UpdateState(bool opened)
    {
        switch (currentState)
        {
            case DoorEventState.Base:
                currentState = opened ? DoorEventState.BiasToClose
                               : DoorEventState.BiasToOpen;
                break;

            case DoorEventState.BiasToClose:
                currentState = opened ? DoorEventState.ForceClose
                               : DoorEventState.Base;
                break;

            case DoorEventState.BiasToOpen:
                currentState = opened ? DoorEventState.Base
                               : DoorEventState.ForceOpen;
                break;

            case DoorEventState.ForceClose:
                if (!opened) currentState = DoorEventState.Base;
                break;

            case DoorEventState.ForceOpen:
                if (opened) currentState = DoorEventState.Base;
                break;
        }
    }

    void BuildRoomsDictionary()
     {
        foreach (Door door in doors)
        {
            // Add door to room A
            if (!roomsMapHelper.ContainsKey(door.RoomA))
                roomsMapHelper[door.RoomA] = new List<Door>();
            roomsMapHelper[door.RoomA].Add(door);

            // Add door to room B
            if (!roomsMapHelper.ContainsKey(door.RoomB))
                roomsMapHelper[door.RoomB] = new List<Door>();
            roomsMapHelper[door.RoomB].Add(door);
        }
     }

    void BuildDoorGraph()
    {
        foreach (var door in doors)
        {
            connectivityGraph[door] = new List<Door>();

            // Add neighbours (doors in room A that are not this door)
            foreach (var neighbor in roomsMapHelper[door.RoomA])
                if (neighbor != door)
                    connectivityGraph[door].Add(neighbor);

            // Add neighbours (doors in room B that are not this door)
            foreach (var neighbor in roomsMapHelper[door.RoomB])
                if (neighbor != door)
                    connectivityGraph[door].Add(neighbor);
        }
    }

    bool CloseDoor(int closeTries, List<Door> openDoors)
    {
        if (closeTries == 0) return false;

        // choose door at random that is currently open
        Dictionary<Door, List<Door>> graphDoorClosed = TryClosingDoor(openDoors);
        
        if (graphDoorClosed != null)
        {
            connectivityGraph = graphDoorClosed;
            return true;
        }
        else
        {
            return CloseDoor(closeTries - 1, openDoors);
        }
    }

    Dictionary<Door, List<Door>> TryClosingDoor(List<Door> openDoors)
    {
        // Deep copy the graph
        var copy = new Dictionary<Door, List<Door>>();
        foreach (var kv in connectivityGraph)
            copy[kv.Key] = new List<Door>(kv.Value);

        Door chosen = openDoors[UnityEngine.Random.Range(0, openDoors.Count)];

        // Remove edges
        foreach (var neighbor in copy[chosen])
            copy[neighbor].Remove(chosen);

        copy[chosen].Clear();

        // If connected close door and return graph copy, else return null
        if (CheckConnectivity(copy))
        {
            chosen.Close();
            return copy;

        }
        else
        {
            return null;
        }
    }

    void OpenDoor(List<Door> closedDoors)
    {
        Door chosen = closedDoors[UnityEngine.Random.Range(0, closedDoors.Count)];

        foreach (var neighbor in roomsMapHelper[chosen.RoomA])
        {
            if (neighbor == chosen) continue;

            if (!connectivityGraph[chosen].Contains(neighbor))
                connectivityGraph[chosen].Add(neighbor);

            if (!connectivityGraph[neighbor].Contains(chosen))
                connectivityGraph[neighbor].Add(chosen);
        }

        foreach (var neighbor in roomsMapHelper[chosen.RoomB])
        {
            if (neighbor == chosen) continue;

            if (!connectivityGraph[chosen].Contains(neighbor))
                connectivityGraph[chosen].Add(neighbor);

            if (!connectivityGraph[neighbor].Contains(chosen))
                connectivityGraph[neighbor].Add(chosen);
        }

        chosen.Open();
    }

    bool CheckConnectivity(Dictionary<Door, List<Door>> graphToCheck)
    {
        if (graphToCheck.Count == 0)
            return true;

        HashSet<Door> visited = new HashSet<Door>();
        Stack<Door> stack = new Stack<Door>();

        Door start = graphToCheck.Keys.First();
        stack.Push(start);
        visited.Add(start);

        while (stack.Count > 0)
        {
            Door current = stack.Pop();

            foreach (Door next in graphToCheck[current])
            {
                if (!visited.Contains(next))
                {
                    visited.Add(next);
                    stack.Push(next);
                }
            }
        }

        return visited.Count == graphToCheck.Count;
    }
}
