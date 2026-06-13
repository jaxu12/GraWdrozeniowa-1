using UnityEngine;
using TMPro;

public class Health : MonoBehaviour
{
    public int hp = 5;
    public TextMeshProUGUI lifeText;

    private void Start()
    {
        UpdateLifeText();
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        UpdateLifeText();

        if (hp <= 0)
        {
            // Sprawdzamy, czy ten obiekt ma tag Player
            if (gameObject.CompareTag("Player"))
            {
                Debug.Log("URUCHOMIONO SMIERC: Padl GRACZ!");
                Time.timeScale = 0f; // Zamraża grę tylko po śmierci gracza
            }
            else
            {
                // Jeśli to potwór stracił HP, po prostu go kasujemy ze sceny!
                Debug.Log("URUCHOMIONO SMIERC: Padl POTWOR " + gameObject.name);
                Destroy(gameObject); 
            }
        }
    }

    private void UpdateLifeText()
    {
        // Aktualizujemy tekst tylko jeśli skrypt należy do gracza i ma podpięte UI
        if (gameObject.CompareTag("Player") && lifeText != null)
        {
            lifeText.text = "Zycia: " + hp;
        }
    }
}