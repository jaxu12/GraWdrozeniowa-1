using UnityEngine;

public class Health : MonoBehaviour
{
    public int hp = 1;

    public void TakeDamage(int damage)
    {
        hp -= damage;

        Debug.Log(gameObject.name + " HP = " + hp + " FRAME=" + Time.frameCount);

        if (hp <= 0)
        {
            Debug.Log(gameObject.name + " UMIERA");

            // !!! NOWA LINIJKA – DODAJEMY PUNKT DO WYNIKU !!!
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddPoint();
            }

            gameObject.SetActive(false);
        }
    }
}