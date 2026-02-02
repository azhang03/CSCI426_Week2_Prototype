using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Text scoreText;
    private int score;
    void Start()
    {
        score = 0;
        UpdateScoreText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    public void ScorePlus()
    {
        score++;
    }
}
