using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class EnemyProjectile : MonoBehaviour
{
    private BoundsCheck bndCheck;
    private Rigidbody rb;

    [Header("Inscribed")]
    public float damage = 1f;

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Main.GAME_PAUSED) return;

        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offDown) ||
            bndCheck.LocIs(BoundsCheck.eScreenLocs.offLeft) ||
            bndCheck.LocIs(BoundsCheck.eScreenLocs.offRight))
        {
            Destroy(gameObject);
        }
    }
}
