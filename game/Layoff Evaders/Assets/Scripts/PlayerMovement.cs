using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    bool isDead = false;
    bool isGrounded = true;
    public float speed = 5.0f;
    public Rigidbody rb;
    AudioManager audioManager;
    int currentLane = 1;
    readonly float laneDistance = 4.0f;
    [SerializeField] GameObject playerAnim;
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }
    void SwitchLaneLeft()
    {
        if (currentLane > 0 & !isDead)
        {
            currentLane--;
            Vector3 newPos = transform.position;
            newPos.x = (currentLane - 1) * laneDistance;
            audioManager.PlaySFX(audioManager.action);
            transform.position = newPos;
        }
    }

    void SwitchLaneRight()
    {
        if (currentLane < 2 & !isDead)
        {
            currentLane++;
            Vector3 newPos = transform.position;
            newPos.x = (currentLane - 1) * laneDistance;
            audioManager.PlaySFX(audioManager.action);
            transform.position = newPos;
        }
    }

    void Jump()
    {
        if (!isGrounded || isDead)
        {
            return;
        }
        audioManager.PlaySFX(audioManager.action);
        playerAnim.GetComponent<Animator>().SetBool("isJumping", true); // Play the jump animation
        rb.AddForce(transform.up * 5, ForceMode.VelocityChange); // Jump
        isGrounded = false;
        Invoke("StopJumping", 1f); // Reset after 1 second
    }

    void StopJumping()
    {
        // Resume the running animation
        playerAnim.GetComponent<Animator>().SetBool("isJumping", false);
        isGrounded = true;
    }

    void Roll()
    {
        if (!isGrounded|| isDead)
        {
            return;
        }
        audioManager.PlaySFX(audioManager.action);
        playerAnim.GetComponent<Animator>().SetBool("isRolling", true); // Play the roll animation
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        col.center = new Vector3(0, 0, 0);
        col.height = 2;
        isGrounded = false;
        Invoke("StopRolling", 1.167f); // Reset after 1.167 second
    }

    void StopRolling()
    {
        // Resume the running animation
        playerAnim.GetComponent<Animator>().SetBool("isRolling", false);
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        col.center = new Vector3(0, 0.5f, 0);
        col.height = 3;
        isGrounded = true;
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }
        Vector3 forwardMove = transform.forward * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMove);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            SwitchLaneLeft();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            SwitchLaneRight();
        } 
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Roll();
        }
    }

    public void Die()
    {
        isDead = true;
        Debug.Log("Player died");
        Invoke("Restart", 2);
    }

    void Restart ()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
