using UnityEngine;
using TMPro; // important !

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText; // Pour TextMeshPro - Text (UI)
    private float score = 0f;

    void Update()
    {
        score += Time.deltaTime * 2;
         if(scoreText != null){
            scoreText.text = Mathf.FloorToInt(score).ToString();
         }
            
    }
}
