using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class CrystalSpawner : MonoBehaviour
{
    [SerializeField]
    int numCrystalsToSpawn = 13;
    [SerializeField]
    int minCrystalsPerBlock = 2;
    [SerializeField]
    // Spawns de cristales del bloque noroeste
    private List<GameObject> nwSpawns;
    [SerializeField]
    // Spawns de cristales del bloque noreste
    private List<GameObject> neSpawns;
    [SerializeField]
    // Spawns de cristales del bloque suroeste
    private List<GameObject> swSpawns;
    [SerializeField]
    // Spawns de cristales del bloque sureste
    private List<GameObject> seSpawns;

    // Spawns that are still unactive
    private List<GameObject>[] remainingSpawns;
    private int[] crystalsSpawnedPerBlock = new int[4] { 0, 0, 0, 0 };
    // Suma de los cristales que faltan en cada bloque para llegar al minimo de cada bloque
    private int crystalsTillMinimum;
    // Lista de los indices de bloques que aun tienen que llegar al minimo
    private List<int> blocksThatStillNeedMinimum;

    void Awake()
    {
        remainingSpawns = new List<GameObject>[] { nwSpawns, neSpawns, swSpawns, seSpawns };
        crystalsTillMinimum = 4 * minCrystalsPerBlock;
        blocksThatStillNeedMinimum = new List<int>() { 0, 1, 2, 3 };

        for (int i = 0; i < numCrystalsToSpawn; i++)
        {
            // We still need to reach our minimums, spawn a crystal in a block that needs to reach its minimum
            if(crystalsTillMinimum > 0 && numCrystalsToSpawn - i == crystalsTillMinimum)
            {
                // We dont care which one to fulfill the minimum since we're gonna eventually fulfill all, so get the first that needs it
                spawnCrystal(blocksThatStillNeedMinimum[0]);
            }
            // If we still don't need to fulfill minimums just spawn a random one
            else
            {
                int randomBlock = Random.Range(0, 4);
                spawnCrystal(randomBlock);
            }
        }
    }

    // Spawn a crystal in the specified block
    private void spawnCrystal(int blockIndex)
    {
        // Choose random spawn from remaining from this block and activate it
        int randomIndex = Random.Range(0, remainingSpawns[blockIndex].Count);
        GameObject spawn = remainingSpawns[blockIndex][randomIndex];
        spawn.SetActive(true);
        crystalsSpawnedPerBlock[blockIndex]++;

        // Remove it from remaining spawns
        remainingSpawns[blockIndex].RemoveAt(randomIndex);

        // Is this block still needing a minimum
        if(blocksThatStillNeedMinimum.Contains(blockIndex))
        {
            crystalsTillMinimum--;

            // Check if we fulfilled the minimum
            if (crystalsSpawnedPerBlock[blockIndex] == minCrystalsPerBlock)
            {
                blocksThatStillNeedMinimum.Remove(blockIndex);
            }
        }
    }
}
