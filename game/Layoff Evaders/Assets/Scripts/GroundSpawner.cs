using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    public GameObject groundTile;
    public GameObject[] environmentTile;
    Vector3 nextSpawnPoint;

    public void SpawnTile() 
    {
        GameObject temp = Instantiate(groundTile, nextSpawnPoint, Quaternion.identity);
        Vector3 spawnLeft = new Vector3(nextSpawnPoint.x - 20,nextSpawnPoint.y,nextSpawnPoint.z);
        Vector3 spawnRight = new Vector3(nextSpawnPoint.x + 20,nextSpawnPoint.y,nextSpawnPoint.z);
        GameObject temp2 = Instantiate(environmentTile[0],spawnLeft,Quaternion.identity);
        // GameObject temp3 = Instantiate(environmentTile[0],spawnRight,Quaternion.identity);
        // instantiate temp3 but flip it horizontally
        GameObject temp3 = Instantiate(environmentTile[0],spawnRight,Quaternion.identity);
        temp3.transform.localScale = new Vector3(-1,1,1);
        temp2.transform.SetParent(temp.transform);
        temp3.transform.SetParent(temp.transform);
        nextSpawnPoint = temp.transform.GetChild(1).transform.position;
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
