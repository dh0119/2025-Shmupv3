using UnityEngine;

public class BossEnemy : Enemy
{
    [Header("Boss Movement")]
    public float targetY = 3.5f;
    public float horizontalAmplitude = 5f;
    public float horizontalFrequency = 1f;
    public float enterSpeed = 4f;

    [Header("Boss Firing")]
    public GameObject enemyProjectilePrefab;
    public float projectileSpeed = 10f;
    public float shotSpacing = 0.6f;

    private float baseX;
    private bool enteredPosition = false;
    private float nextFireTime = 0f;

    void Start()
    {
        isBoss = true;
        score = 500;
        killValue = 1;
        baseX = transform.position.x;
    }

    public override void Move()
    {
        Vector3 tempPos = pos;

        if (!enteredPosition)
        {
            tempPos.y = Mathf.MoveTowards(tempPos.y, targetY, enterSpeed * Time.deltaTime);

            if (Mathf.Abs(tempPos.y - targetY) < 0.05f)
            {
                enteredPosition = true;
            }
        }
        else
        {
            float x = baseX + Mathf.Sin(Time.time * horizontalFrequency) * horizontalAmplitude;
            tempPos.x = x;
        }

        pos = tempPos;
    }

    void LateUpdate()
    {
        if (Main.GAME_PAUSED) return;

        if (!enteredPosition) return;

        if (Time.time >= nextFireTime)
        {
            FireSpread();
            nextFireTime = Time.time + fireRate;
        }
    }

    void FireSpread()
    {
        FireProjectile(Vector3.down);
        FireProjectile((Quaternion.AngleAxis(20f, Vector3.forward) * Vector3.down).normalized);
        FireProjectile((Quaternion.AngleAxis(-20f, Vector3.forward) * Vector3.down).normalized);
    }

    void FireProjectile(Vector3 dir)
    {
        if (enemyProjectilePrefab == null) return;

        GameObject go = Instantiate(enemyProjectilePrefab);
        go.transform.position = transform.position + Vector3.down * shotSpacing;

        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = dir * projectileSpeed;
        }
    }
}
