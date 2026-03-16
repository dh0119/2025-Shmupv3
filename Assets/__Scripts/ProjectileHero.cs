using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class ProjectileHero : MonoBehaviour
{
    private BoundsCheck bndCheck;
    private Renderer rend;

    [Header("Dynamic")]
    public Rigidbody rigid;
    [SerializeField]
    private eWeaponType _type;
    public float damageOnHit = 0f;

    [Header("Special Motion")]
    public float missileRotateSpeed = 180f;
    public float phaserWaveFrequency = 10f;
    public float phaserWaveAmplitude = 0.5f;

    private Enemy missileTarget;
    private float birthTime;
    private Vector3 basePos;

    public eWeaponType type
    {
        get { return (_type); }
        set { SetType(value); }
    }

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        rend = GetComponent<Renderer>();
        rigid = GetComponent<Rigidbody>();
    }

    void Start()
    {
        birthTime = Time.time;
        basePos = transform.position;

        if (_type == eWeaponType.missile)
        {
            missileTarget = Main.GetClosestEnemy(transform.position);
        }
    }

    void Update()
    {
        if (Main.GAME_PAUSED) return;

        switch (_type)
        {
            case eWeaponType.phaser:
                UpdatePhaser();
                break;

            case eWeaponType.missile:
                UpdateMissile();
                break;
        }

        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offUp) ||
            bndCheck.LocIs(BoundsCheck.eScreenLocs.offLeft) ||
            bndCheck.LocIs(BoundsCheck.eScreenLocs.offRight))
        {
            Destroy(gameObject);
        }
    }

    public void SetType(eWeaponType eType)
    {
        _type = eType;
        WeaponDefinition def = Main.GET_WEAPON_DEFINITION(_type);
        rend.material.color = def.projectileColor;
        damageOnHit = def.damageOnHit;
    }

    public Vector3 vel
    {
        get { return rigid.velocity; }
        set { rigid.velocity = value; }
    }

    void UpdateMissile()
    {
        if (missileTarget == null)
        {
            missileTarget = Main.GetClosestEnemy(transform.position);
        }

        if (missileTarget != null)
        {
            Vector3 dir = (missileTarget.transform.position - transform.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                missileRotateSpeed * Time.deltaTime
            );

            rigid.velocity = transform.up * Main.GET_WEAPON_DEFINITION(_type).velocity;
        }
    }

    void UpdatePhaser()
    {
        float age = Time.time - birthTime;
        Vector3 pos = transform.position;
        pos += Vector3.up * Main.GET_WEAPON_DEFINITION(_type).velocity * Time.deltaTime;

        float offsetX = Mathf.Sin(age * phaserWaveFrequency) * phaserWaveAmplitude;
        pos.x += offsetX * Time.deltaTime * 6f;

        transform.position = pos;
    }
}
