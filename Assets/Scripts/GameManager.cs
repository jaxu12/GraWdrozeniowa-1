using UnityEngine;
using TMPro; // Wymagane do obsługi TextMeshPro

public class GameManager : MonoBehaviour
{
    // Statyczna referencja, żeby każdy skrypt w grze mógł łatwo dodać punkt
    public static GameManager Instance;

    public TextMeshProUGUI scoreText; // Tu przeciągniemy nasz napis z hierarchii
    private int score = 0;

    private void Awake()
    {
        // Tworzymy tzw. Singletona, żeby dostęp do wyniku był banalnie prosty
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateScoreUI();
    }

    // Tę funkcję wywołamy ze skryptu Health, kiedy mob dostanie śmiertelny cios
    public void AddPoint()
    {
        score++;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Wynik: " + score;
        }
    }
}