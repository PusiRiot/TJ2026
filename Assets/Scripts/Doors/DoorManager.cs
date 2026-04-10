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
    #region variables
    [SerializeField] GameObject doorsContainer;

    List<Door> doors;
    List<Door> openedDoors;
    List<Door> closedDoors;

    Door chosenDoor;

    Dictionary<int, List<Door>> roomsMapHelper = new(); // rooms and which doors has each room
    Dictionary<int, List<int>> connectivityGraph = new (); // each room is a node
    DoorEventState currentState;

    int _closedDoorsOnAwake;
    float _baseDoorRandom;
    float _biasToCloseDoorRandom;
    float _biasToOpenDoorRandom;
    int _minRandomTime;
    int _maxRandomTime;

    // lightning effect
    DoorLightningEffect lightningEffect;
    #endregion

    #region MonoBehaviour
    private void Start()
    {
        lightningEffect = GetComponent<DoorLightningEffect>();

        _closedDoorsOnAwake = GameStatsAccess.Instance.GetClosedDoorsOnAwake();
        _baseDoorRandom = GameStatsAccess.Instance.GetBaseDoorRandom();
        _biasToCloseDoorRandom = GameStatsAccess.Instance.GetBiasToCloseDoorRandom();
        _biasToOpenDoorRandom = GameStatsAccess.Instance.GetBiasToOpenDoorRandom();
        _minRandomTime = GameStatsAccess.Instance.GetMinDoorRandomTime();
        _maxRandomTime = GameStatsAccess.Instance.GetMaxDoorRandomTime();

        if (doorsContainer == null)
            throw new System.Exception("No doors container assigned on DoorsManager");

        doors = doorsContainer.GetComponentsInChildren<Door>().ToList();

        openedDoors = doors.Where(d => !d.IsClosed).ToList();
        closedDoors = doors.Where(d => d.IsClosed).ToList();

        BuildRoomsDictionary();

        BuildConnectivityGraph();

        for (int i = 0; i <= _closedDoorsOnAwake; i++)
        {
            bool closedSuccesfully = CloseDoor();

            if (!closedSuccesfully)
            {
                // there are no more doors to close that kep path connected, stop and open one next time
                currentState = DoorEventState.ForceOpen;
                break;
            }
        }

        StartCoroutine(DoorRoutine());
    }
    #endregion

    IEnumerator DoorRoutine()
    {
        while (true)
        {
            float wait = Random.Range(_minRandomTime, _maxRandomTime);
            yield return new WaitForSeconds(_minRandomTime);

            if (openedDoors.Count == doors.Count || !ShouldOpenDoor()) // if all doors are open close a door, else check randomlly based on state to either close or open
            {
                bool closedSuccesfully = CloseDoor();

                if (!closedSuccesfully) // if it checked all doors and could find one that it could safelly clase
                {
                    OpenDoor(); // if it checked all doors and couldn't close one just open a door
                    currentState = DoorEventState.ForceOpen; // next state it also opens one
                }
                else
                {
                    UpdateState(false);
                }
            }
            else
            {
                OpenDoor();
                UpdateState(true);
            }

            lightningEffect.GenerateLighningEffect(chosenDoor.transform);
        }
    }

    #region Decision randomness and state

    bool ShouldOpenDoor()
    {
        float r = Random.value;

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
    #endregion

    #region Create graphs
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

    void BuildConnectivityGraph()
    {
        // Initialize all rooms
        foreach (int room in roomsMapHelper.Keys)
            connectivityGraph[room] = new List<int>();

        // Add edges for open doors
        foreach (Door door in doors)
        {
            if (!door.IsClosed)
            {
                connectivityGraph[door.RoomA].Add(door.RoomB);
                connectivityGraph[door.RoomB].Add(door.RoomA);
            }
        }
    }
    #endregion

    #region Open/Close

    bool CloseDoor()
    {
        // randomize list to go through it searching for a room that can be closed and keeps path connected
        List<int> indices = Enumerable.Range(0, openedDoors.Count).OrderBy(_ => Random.value).ToList();

        foreach (int index in indices)
        {
            chosenDoor = openedDoors[index];

            // Remove edges
            connectivityGraph[chosenDoor.RoomA].Remove(chosenDoor.RoomB);
            connectivityGraph[chosenDoor.RoomB].Remove(chosenDoor.RoomA);

            // If the graph stays connected it can be closed, else, reassign edges
            if (CheckConnectivity())
            {
                // execute door's Close() method and update lists
                openedDoors.Remove(chosenDoor);
                closedDoors.Add(chosenDoor);
                chosenDoor.Close();
                return true;
            }
            else
            {
                connectivityGraph[chosenDoor.RoomA].Add(chosenDoor.RoomB);
                connectivityGraph[chosenDoor.RoomB].Add(chosenDoor.RoomA);
            }
        }

        return false;
    }

    void OpenDoor()
    {
        chosenDoor = closedDoors[Random.Range(0, closedDoors.Count)];

        connectivityGraph[chosenDoor.RoomA].Add(chosenDoor.RoomB);
        connectivityGraph[chosenDoor.RoomB].Add(chosenDoor.RoomA);

        // update lists and execute doors Open() method
        closedDoors.Remove(chosenDoor);
        openedDoors.Add(chosenDoor);
        chosenDoor.Open();
    }

    bool CheckConnectivity()
    {
        if (connectivityGraph.Count == 0)
            return true;

        HashSet<int> visited = new();
        Stack<int> stack = new();

        int start = connectivityGraph.Keys.First();
        stack.Push(start);
        visited.Add(start);

        while (stack.Count > 0)
        {
            int current = stack.Pop();

            foreach (int next in connectivityGraph[current])
            {
                if (!visited.Contains(next))
                {
                    visited.Add(next);
                    stack.Push(next);
                }
            }
        }

        return visited.Count == connectivityGraph.Count;
    }
    #endregion
}
