using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Database;
using Firebase;
using Firebase.Extensions;
using Unity.VisualScripting;
using TMPro;
using System;
using UnityEngine.XR;

public class PlayerMovement : MonoBehaviour
{
    //for the hiit stuff
    private float initialDelay = 15f;
    private float sprintDuration = 30f;
    private float restDuration = 120f;
    private float timer;
    private bool isFirstCycle = true;
    private bool isSprinting = false;
    public TextMeshProUGUI stateText;
    public float sprintSpeedMultiplier = 1.5f;
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
    private DatabaseReference reference;
    private int squatCount = 0;
    private int jumpCount = 0;
    private int lateralRaiseCount = 0;
    private int initialSquatCount = 0;
    private int initialJumpCount = 0;
    private int initialLateralRaiseCount = 0;
    private string startTime;
    private Vector3 headsetInitialPosition;
    private bool isHeadsetPositionSet = false;

    private bool isJumping = false;
    private bool isSquatting = false;
    private bool isLeftRaising = false;
    private bool isRightRaising = false;
    [SerializeField] private TextMeshProUGUI sprintTimerText;
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        startTime = DateTime.UtcNow.ToString("o");
        baseSpeed = speed;
        timer = initialDelay;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                FirebaseDatabase database = FirebaseDatabase.GetInstance("https://layoff-evaders-default-rtdb.firebaseio.com/");
                Debug.Log("Firebase Initialized");

                // Setting "test" to 1 when connected
                reference = database.RootReference;
                reference.Child("users").Child("user1").Child("squatCount").GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        initialSquatCount = int.Parse(snapshot.Value.ToString());
                    }
                });
                reference.Child("users").Child("user1").Child("jumpCount").GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        initialJumpCount = int.Parse(snapshot.Value.ToString());
                    }
                });
                reference.Child("users").Child("user1").Child("lateralRaiseCount").GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        initialLateralRaiseCount = int.Parse(snapshot.Value.ToString());
                    }
                });
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {task.Result}");
            }
        });
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
            RecordLateralRaise();
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
            RecordLateralRaise();
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
        RecordJump();
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
        RecordSquat();
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
            
            int seconds = Mathf.CeilToInt(timer);
            sprintTimerText.text = isSprinting ? 
                $"Next Standup Meeting: {seconds}s" : 
                $"Next Budget Cut: {seconds}s";
            sprintTimerText.gameObject.SetActive(true);

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
        TrackVRInputs();
    }

    private void TrackVRInputs()
    {
        // Get the position of the VR headset
        InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        InputDevice leftControllerDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        InputDevice rightControllerDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (!isHeadsetPositionSet)
        {
            // Set initial headset position
            if (headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 initialPosition))
            {
                headsetInitialPosition = initialPosition;
                isHeadsetPositionSet = true;
            }
        }

        // Check headset Y-axis for Jump and Squat
        if (headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 headsetPosition))
        {
            float yChange = headsetPosition.y - headsetInitialPosition.y;

            if (yChange > 0.1f && !isJumping) // Adjust threshold for jump detection
            {
                isJumping = true; // Mark as jumping
                Jump();
            }
            else if (yChange <= 0.1f && isJumping) // Reset when player lands
            {
                isJumping = false;
            }

            else if (yChange < -0.3f && !isSquatting) // Adjust threshold for squat detection
            {
                isSquatting = true; // Mark as squatting
                Roll();
            }
            else if (yChange >= -0.3f && isSquatting) // Reset when player stands up
            {
                isSquatting = false;
            }

            else if (leftControllerDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftControllerPosition) &&
            rightControllerDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightControllerPosition)) {
                float leftYChange = headsetPosition.y - leftControllerPosition.y; // Relative to current headset position
                float rightYChange = headsetPosition.y - rightControllerPosition.y; // Relative to current headset position
                if (leftYChange < 0.3f && !isLeftRaising && !isRightRaising) // Left lateral raise
                {
                    isLeftRaising = true; // Mark as raising
                    SwitchLaneLeft();
                }
                else if (leftYChange >= 0.3f && isLeftRaising) // Reset when arm moves out of range
                {
                    isLeftRaising = false;
                }

                if (rightYChange < 0.3f && !isRightRaising && !isLeftRaising) // Right lateral raise
                {
                    isRightRaising = true; // Mark as raising
                    SwitchLaneRight();
                }
                else if (rightYChange >= 0.3f && isRightRaising) // Reset when arm moves out of range
                {
                    isRightRaising = false;
                }
            }
        }
    }

    public void Die()
    {
        isDead = true;
        playerAnim.GetComponent<Animator>().Play("Stumble Backwards");
        Debug.Log("Player died");
        // add game to games array
        // { score: x, startTime: y, endTime: z, squatCount: a, jumpCount: b, lateralRaiseCount: c }
        reference.Child("users").Child("user1").Child("games").Push().SetRawJsonValueAsync(
            "{\"score\":" + GameManager.instance.GetScore() + "," +
            "\"startTime\":\"" + startTime + "\"," +
            "\"endTime\":\"" + DateTime.UtcNow.ToString("o") + "\"," +
            "\"squatCount\":" + squatCount + "," +
            "\"jumpCount\":" + jumpCount + "," +
            "\"lateralRaiseCount\":" + lateralRaiseCount + "}"
        );
    }

    void StartSprintPhase()
    {
        isSprinting = true;
        timer = sprintDuration;
        speed = baseSpeed * sprintSpeedMultiplier;
        stateText.text = "ANNUAL BUDGET CUT";
        stateText.gameObject.SetActive(true);
        sprintTimerText.gameObject.SetActive(false);
        Invoke("HideText", 3f);
    }

    void StartRestPhase()
    {
        isSprinting = false;
        timer = restDuration;
        speed = baseSpeed;
        stateText.text = "INVESTORS ARE PLEASED";
        stateText.gameObject.SetActive(true);
        Invoke("HideText", 3f);
    }

    void HideText()
    {
        stateText.gameObject.SetActive(false);
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("RESTART!");
    }
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
        Debug.Log("MAIN MENU!");
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("QUIT!");
    }
    public void RecordSquat()
    {
        squatCount++;
        reference.Child("users").Child("user1").Child("squatCount").SetValueAsync(initialSquatCount + squatCount);
    }
    public void RecordJump()
    {
        jumpCount++;
        reference.Child("users").Child("user1").Child("jumpCount").SetValueAsync(initialJumpCount + jumpCount);
    }
    public void RecordLateralRaise()
    {
        lateralRaiseCount++;
        reference.Child("users").Child("user1").Child("lateralRaiseCount").SetValueAsync(initialLateralRaiseCount + lateralRaiseCount);
    }
}
