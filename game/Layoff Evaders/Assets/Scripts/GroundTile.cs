using System.Collections.Generic;
using UnityEngine;

public class GroundTile : MonoBehaviour
{
    GroundSpawner groundSpawner;
    public GameObject obstaclePrefab;
    public GameObject coinPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        groundSpawner = GameObject.FindObjectOfType<GroundSpawner>();
        SpawnObstacle();
        SpawnCoin();
    }

    void OnTriggerExit(Collider other)
    {
        groundSpawner.SpawnTile();
        Destroy(gameObject, 2);
    }

    void SpawnObstacle()
    {
        int obstaclesToSpawn = Random.value < 0.2f ? 2 : 1;
        HashSet<int> usedIndices = new HashSet<int>();
        for (int i = 0; i < obstaclesToSpawn; i++)
        {
            int obstacleSpawnIndex;

            // Ensure unique spawn index for each obstacle
            do
            {
                obstacleSpawnIndex = Random.Range(2, 5); // Random index from 3 to 5 inclusive
            } while (usedIndices.Contains(obstacleSpawnIndex));

            usedIndices.Add(obstacleSpawnIndex);

            Transform spawnPoint = transform.GetChild(obstacleSpawnIndex).transform;
            // 50% chance for y position of the obstacle to be 0.5
            // 50% chance for y position of the obstacle to be 3
            float yPosition = Random.value < 0.5f ? 0.5f : 3f;
            spawnPoint.position = new Vector3(spawnPoint.position.x, yPosition, spawnPoint.position.z);

            // Spawn the obstacle
            Instantiate(obstaclePrefab, spawnPoint.position, Quaternion.identity, transform);
        }
    }

    void SpawnCoin(int coinCount = 3)
    {
        if (coinCount <= 0)
        {
            return;
        }
        int coinSpawnIndex = Random.Range(5, 14);
        Transform spawnPoint = transform.GetChild(coinSpawnIndex).transform;
        // if the coin is spawned on the obstacle, recusively call the function again
        if (spawnPoint.childCount > 0)
        {
            SpawnCoin();
            return;
        }

        Instantiate(coinPrefab, spawnPoint.position, Quaternion.Euler(-90, 0, 0), transform);
        SpawnCoin(coinCount - 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
