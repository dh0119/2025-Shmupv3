using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ProjectileLaser : MonoBehaviour
{
    [Header("Laser Settings")]
    public float damage = 20f;
    public float duration = 0.12f;
    public float beamWidth = 0.3f;
    public float extraBeamLength = 2f;

    private float birthTime;
    private BoxCollider box;
    private float range;

    void Awake()
    {
        box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    void Start()
    {
        birthTime = Time.time;

        // Make the beam long enough to feel infinite on screen
        if (Camera.main != null)
        {
            range = Camera.main.orthographicSize * 2f + extraBeamLength;
        }
        else
        {
            range = 25f;
        }

        // Force beam to point straight up in gameplay space
        transform.rotation = Quaternion.identity;

        // Stretch the beam
        transform.localScale = new Vector3(beamWidth, range, beamWidth);

        // Move beam upward so it starts at the weapon muzzle and extends forward
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + range * 0.5f,
            0f
        );
    }

    void Update()
    {
        if (Main.GAME_PAUSED) return;

        if (Time.time > birthTime + duration)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerStay(Collider other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            float damageThisFrame = damage * Time.deltaTime;
            enemy.health -= damageThisFrame;

            if (enemy.health <= 0)
            {
                Main.SHIP_DESTROYED(enemy);
                Destroy(enemy.gameObject);
            }
        }
    }
}
