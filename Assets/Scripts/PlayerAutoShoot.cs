using UnityEngine;

public class PlayerAutoShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float fireRate = 0.5f;
    public float range = 10f;
    public float facingThreshold = 0.01f;

    float timer;

    void Update()
    {
        timer -= Time.deltaTime;
        Transform target = FindClosestEnemy();

        if (target != null)
        {
            UpdateFacing(target);
        }

        if (timer <= 0f && target != null)
        {
            Shoot(target);
            timer = fireRate;
        }
    }

    Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Transform closest = null;
        float minDist = range;

        foreach (var e in enemies)
        {
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = e.transform;
            }
        }

        return closest;
    }

    void Shoot(Transform target)
    {
        Vector2 dir = (target.position - transform.position).normalized;

        GameObject b = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        b.GetComponent<Bullet>().SetDirection(dir);
    }

    void UpdateFacing(Transform target)
    {
        float deltaX = target.position.x - transform.position.x;

        if (Mathf.Abs(deltaX) < facingThreshold) return;

        transform.rotation = Quaternion.Euler(
            0f,
            deltaX < 0f ? 180f : 0f,
            0f
        );
    }
}
