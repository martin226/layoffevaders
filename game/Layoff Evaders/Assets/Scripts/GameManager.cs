using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    int score = 0;
    public static GameManager instance;
    public TextMeshProUGUI scoreText;
    

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void IncreaseScore()
    {
        score++;
        scoreText.text = "Total Compensation: $" + score;
        Debug.Log(score);
    }

    public int GetScore()
    {
        return score;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
