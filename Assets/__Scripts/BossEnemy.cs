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

    private int currentPhase = 1;
    private float maxHealth = 500f;
    private bool fireAltPattern = false;

    void Start()
    {
        isBoss = true;
        score = 500;
        killValue = 1;
        maxHealth = health;
        baseX = transform.position.x;

        SetPhase(1);
    }

    void UpdatePhase()
{
    int newPhase = 1;

    if (health <= 100) newPhase = 5;
    else if (health <= 200) newPhase = 4;
    else if (health <= 300) newPhase = 3;
    else if (health <= 400) newPhase = 2;
    else newPhase = 1;

    if (newPhase != currentPhase)
    {
        SetPhase(newPhase);
    }
}

    void SetPhase(int phase)
{
    currentPhase = phase;

    switch (currentPhase)
    {
        case 1:
            fireRate = 1.2f;
            horizontalAmplitude = 3.5f;
            horizontalFrequency = 0.8f;
            projectileSpeed = 8f;
            break;

        case 2:
            fireRate = 1.0f;
            horizontalAmplitude = 4.5f;
            horizontalFrequency = 1.0f;
            projectileSpeed = 9f;
            break;

        case 3:
            fireRate = 0.8f;
            horizontalAmplitude = 5.5f;
            horizontalFrequency = 1.2f;
            projectileSpeed = 10f;
            break;

        case 4:
            fireRate = 0.65f;
            horizontalAmplitude = 6.0f;
            horizontalFrequency = 1.4f;
            projectileSpeed = 11f;
            break;

        case 5:
            fireRate = 0.5f;
            horizontalAmplitude = 6.5f;
            horizontalFrequency = 1.7f;
            projectileSpeed = 12f;
            break;
    }

    Debug.Log("Boss entered Phase " + currentPhase);
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

    UpdatePhase();

    if (!enteredPosition) return;

    if (Time.time >= nextFireTime)
    {
        FireByPhase();
        nextFireTime = Time.time + fireRate;
    }
}

    void FireByPhase()
{
    switch (currentPhase)
    {
        case 1:
            FireSpread3(20f);
            break;

        case 2:
            FireSpread5(30f);
            break;

        case 3:
            if (fireAltPattern)
                FireSpread5(35f);
            else
                FireAimedShot();

            fireAltPattern = !fireAltPattern;
            break;

        case 4:
            FireSpread5(45f);
            break;

        case 5:
            FireSpread7(55f);
            break;
    }
}

        void FireSpread3(float angle)
{
    FireProjectile(Vector3.down);
    FireProjectile((Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.down).normalized);
    FireProjectile((Quaternion.AngleAxis(-angle, Vector3.forward) * Vector3.down).normalized);
}

void FireSpread5(float angle)
{
    FireProjectile(Vector3.down);
    FireProjectile((Quaternion.AngleAxis(angle * 0.5f, Vector3.forward) * Vector3.down).normalized);
    FireProjectile((Quaternion.AngleAxis(-angle * 0.5f, Vector3.forward) * Vector3.down).normalized);
    FireProjectile((Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.down).normalized);
    FireProjectile((Quaternion.AngleAxis(-angle, Vector3.forward) * Vector3.down).normalized);
}

void FireSpread7(float angle)
{
    FireProjectile(Vector3.down);
    FireProjectile((Quaternion.AngleAxis(angle * 0.33f, Vector3.forward) * Vector3.down).normalized);
    FireProjectile((Quaternion.AngleAxis(-angle * 0.33f, Vector3.forward) * Vector3.down).normalized);
    FireProjectile((Quaternion.AngleAxis(angle * 0.66f, Vector3.forward) * Vector3.down).normalized);
    FireProjectile((Quaternion.AngleAxis(-angle * 0.66f, Vector3.forward) * Vector3.down).normalized);
    FireProjectile((Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.down).normalized);
    FireProjectile((Quaternion.AngleAxis(-angle, Vector3.forward) * Vector3.down).normalized);
}

void FireAimedShot()
{
    if (Hero.S == null) return;

    Vector3 dir = (Hero.S.transform.position - transform.position).normalized;
    FireProjectile(dir);
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
