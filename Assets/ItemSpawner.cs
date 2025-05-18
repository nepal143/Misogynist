using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [Header("Assign item prefabs here")]
    public List<GameObject> itemPrefabs;

    [Header("Assign spawn points here")]
    public List<Transform> spawnPoints;

    void Start()
    {
        SpawnItems();
    }
void SpawnItems()
{
    if (spawnPoints.Count < itemPrefabs.Count)
    {
        Debug.LogError("Not enough spawn points for the number of items!");
        return;
    }

    // Make a copy of spawnPoints to track unused positions
    List<Transform> availableSpawns = new List<Transform>(spawnPoints);

    foreach (GameObject itemPrefab in itemPrefabs)
    {
        // Choose a random spawn point from the available ones
        int randomIndex = Random.Range(0, availableSpawns.Count);
        Transform chosenPoint = availableSpawns[randomIndex];

        // Spawn the item at the chosen spawn point and make it a child of the spawn point
        GameObject spawnedItem = Instantiate(itemPrefab, chosenPoint.position, chosenPoint.rotation, chosenPoint);

        // Rename the spawned item to remove "(Clone)"
        spawnedItem.name = itemPrefab.name;

        // Remove that spawn point so it's not reused
        availableSpawns.RemoveAt(randomIndex);
    }
}
}
