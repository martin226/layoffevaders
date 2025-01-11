using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    public GameObject groundTile;
    public GameObject[] environmentTile;
    Vector3 nextSpawnPoint = new Vector3(0,0,0);

    public void SpawnTile()
    {
        // Instantiate the main ground tile
        GameObject temp = Instantiate(groundTile, nextSpawnPoint, Quaternion.identity);

        // Access the child "Plane" to get its Renderer
        Transform planeTransform = environmentTile[0].transform.Find("Plane");
        if (planeTransform == null)
        {
            Debug.LogError("Plane child not found in environmentTile!");
            return;
        }

        Renderer planeRenderer = planeTransform.GetComponent<Renderer>();
        if (planeRenderer == null)
        {
            Debug.LogError("No Renderer found on Plane!");
            return;
        }

        // Calculate the tile dimensions dynamically
        float tileWidth = planeRenderer.bounds.size.x; // X-axis width
        float tileLength = planeRenderer.bounds.size.z; // Z-axis length

        // Calculate spawn positions for the left and right tiles
        Vector3 spawnLeft = new Vector3(nextSpawnPoint.x - 30, nextSpawnPoint.y, nextSpawnPoint.z);
        Vector3 spawnRight = new Vector3(nextSpawnPoint.x + 30, nextSpawnPoint.y, nextSpawnPoint.z);

        // Instantiate the left and right environment tiles
        int randomIndex = Random.Range(0, environmentTile.Length);
        GameObject temp2 = Instantiate(environmentTile[randomIndex], spawnLeft, Quaternion.identity);
        randomIndex = Random.Range(0, environmentTile.Length);
        GameObject temp3 = Instantiate(environmentTile[randomIndex], spawnRight, Quaternion.identity);

        // Flip the right tile horizontally
        temp3.transform.localScale = new Vector3(-1 * Mathf.Abs(temp3.transform.localScale.x),
                                                temp3.transform.localScale.y,
                                                temp3.transform.localScale.z);

        // Parent the environment tiles to the ground tile
        temp2.transform.SetParent(temp.transform);
        temp3.transform.SetParent(temp.transform);

        // Update the nextSpawnPoint for the next ground tile
        nextSpawnPoint = temp.transform.GetChild(1).transform.position + new Vector3(0, 0, tileLength / 2);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < 15; i++)
        {
            SpawnTile();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
