using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Legaturi UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI scorFinalText;
    public TextMeshProUGUI highText;

    void Awake()
    {
        instance = this;
    }

    public void DeclanseazaGameOver()
    {
        // 1. OPRIM TIMPUL (CRUCIAL: Asta previne restartul fantoma!)
        Time.timeScale = 0; 

        // 2. Afisam panoul
        gameOverPanel.SetActive(true);

        // 3. Calculam scorul
        int scorCurent = 0;
        if(ScoreManager.instance != null)
        {
            scorCurent = ScoreManager.instance.GetScore();
        }
        scorFinalText.text = "SCORE: " + scorCurent;

        // 4. High Score
        int highscore = PlayerPrefs.GetInt("HighScore", 0);
        if (scorCurent > highscore)
        {
            highscore = scorCurent;
            PlayerPrefs.SetInt("HighScore", highscore);
            PlayerPrefs.Save();
        }
        highText.text = "BEST: " + highscore;
    }

    public void ButonRestart()
    {
        Time.timeScale = 1; // Pornim timpul la loc
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ButonExit()
    {
        Debug.Log("Exit Joc");
        Application.Quit();
    }
}