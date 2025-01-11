using UnityEngine;

public class Obstacle : MonoBehaviour
{
    PlayerMovement playerMovement;
    AudioManager audioManager;
    [SerializeField] GameObject playerAnim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }    
    void Start()
    {
        playerMovement = GameObject.FindObjectOfType<PlayerMovement>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Player")
        {
            // Play SFX
            audioManager.PlaySFX(audioManager.crash);
            playerMovement.Die();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
