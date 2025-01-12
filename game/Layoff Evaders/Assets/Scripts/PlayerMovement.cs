using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class PlayerMovement : MonoBehaviour
{
    //for the hiit stuff
    private float initialDelay = 30f;
    private float sprintDuration = 30f;
    private float restDuration = 120f;
    private float timer;
    private bool isFirstCycle = true;
    private bool isSprinting = false;
    public TextMeshProUGUI stateText;
    public float sprintSpeedMultiplier = 2f;
    private float baseSpeed;
    
    bool isDead = false;
    bool isGrounded = true;
    public float speed = 5.0f;
    public Rigidbody rb;
    AudioManager audioManager;
    int currentLane = 1;
    readonly float laneDistance = 4.0f;
    [SerializeField] GameObject playerAnim;
    public GameObject gameOverUI;
    
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        baseSpeed = speed;
        timer = initialDelay;
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
            gameOverUI.SetActive(true);
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
        Vector3 forwardMove;
    
        if (isDead)
        {
            gameOverUI.SetActive(true);
            forwardMove = Vector3.zero;
        }
        else
        {
            forwardMove = transform.forward * speed * Time.fixedDeltaTime;
        }
        
        rb.MovePosition(rb.position + forwardMove);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            timer -= Time.deltaTime;
            
            if (timer <= 0)
            {
                if (isFirstCycle)
                {
                    StartSprintPhase();
                    isFirstCycle = false;
                }
                else if (isSprinting)
                {
                    StartRestPhase();
                }
                else
                {
                    StartSprintPhase();
                }
            }
        }
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
    void StartSprintPhase()
    {
        isSprinting = true;
        timer = sprintDuration;
        speed = baseSpeed * sprintSpeedMultiplier;
        stateText.text = "SPRINT!";
        stateText.gameObject.SetActive(true);
        Invoke("HideText", 3f);
    }

    void StartRestPhase()
    {
        isSprinting = false;
        timer = restDuration;
        speed = baseSpeed;
        stateText.text = "REST";
        stateText.gameObject.SetActive(true);
        Invoke("HideText", 3f);
    }

    void HideText()
    {
        stateText.gameObject.SetActive(false);
    }

    public void Die()
    {
        isDead = true;
        playerAnim.GetComponent<Animator>().Play("Stumble Backwards");
        Debug.Log("Player died");
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void mainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("QUIT!");
    }
}
