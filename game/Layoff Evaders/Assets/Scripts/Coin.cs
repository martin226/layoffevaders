using UnityEngine;

public class Coin : MonoBehaviour
{
    public float turnSpeed = 90f;
    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.GetComponent<Obstacle>() != null)
        {
            Destroy(gameObject);
            return;
        }
        if (other.gameObject.name != "Player")
        {
            return;
        }

        // Increase the score
        GameManager.instance.IncreaseScore();

        // Destroy the coin
        Destroy(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, turnSpeed * Time.deltaTime);
    }
}
