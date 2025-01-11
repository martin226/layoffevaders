using UnityEngine;

public class Obstacle : MonoBehaviour
{
    PlayerMovement playerMovement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovement = GameObject.FindObjectOfType<PlayerMovement>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Player")
        {
            playerMovement.Die();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
