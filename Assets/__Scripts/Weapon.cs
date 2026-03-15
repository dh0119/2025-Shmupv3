using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary
/// This is an enum of the various possible weapon types.
/// It also includes a "shield" type to allow a shield PowerUp.
/// Items marked [NI] below are Not Implemented in this book.
/// </summary
public enum eWeaponType
{
    none,       // The default / no weapon
    blaster,    // A simple blaster
    spread,     // Multiple shots simultaneously
    phaser,     // [NI] Shots that move in waves
    missile,    // [NI] Homing missiles

    laser,      // [NI] Damage over time
    shield      // Raise shieldLevel
}


/// <summary
/// The WeaponDefinition class allows you to set the properties
///   of a specific weapon in the Inspector. The Main class has
///   an array of WeaponDefinitions that makes this possible.
/// </summary
[System.Serializable]                                                         // a
public class WeaponDefinition
{                                               // b
    public eWeaponType type = eWeaponType.none;
    [Tooltip("Letter to show on the PowerUp Cube")]                           // c
    public string letter;
    [Tooltip("Color of PowerUp Cube")]
    public Color powerUpColor = Color.white;                           // d
    [Tooltip("Prefab of Weapon model that is attached to the Player Ship")]
    public GameObject weaponModelPrefab;
    [Tooltip("Prefab of projectile that is fired")]
    public GameObject projectilePrefab;
    [Tooltip("Color of the Projectile that is fired")]
    public Color projectileColor = Color.white;                        // d
    [Tooltip("Damage caused when a single Projectile hits an Enemy")]
    public float damageOnHit = 0;
    [Tooltip("Damage caused per second by the Laser [Not Implemented]")]
    public float damagePerSec = 0;
    [Tooltip("Seconds to delay between shots")]
    public float delayBetweenShots = 0;
    [Tooltip("Velocity of individual Projectiles")]
    public float velocity = 50;
}

public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Dynamic")]                                                        // a
    [SerializeField]                                                           // a
    [Tooltip("Setting this manually while playing does not work properly.")]   // a
    private eWeaponType _type = eWeaponType.none;
    public WeaponDefinition def;
    public float nextShotTime; // Time the Weapon will fire next

    private GameObject weaponModel;
    private Transform shotPointTrans;

    void Start()
    {
        // Set up PROJECTILE_ANCHOR if it has not already been done
        if (PROJECTILE_ANCHOR == null)
        {                                       // b
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        shotPointTrans = transform.GetChild(0);                              // c

        // Call SetType() for the default _type set in the Inspector
        SetType(_type);                                                      // d

        // Find the fireEvent of a Hero Component in the parent hierarchy
        Hero hero = GetComponentInParent<Hero>();                              // e
        if (hero != null) hero.fireEvent += Fire;
    }

    public eWeaponType type
    {
        get { return (_type); }
        set { SetType(value); }
    }

    public void SetType(eWeaponType wt)
    {
        _type = wt;
        if (type == eWeaponType.none)
        {                                       // f
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }
        // Get the WeaponDefinition for this type from Main
        def = Main.GET_WEAPON_DEFINITION(_type);
        // Destroy any old model and then attach a model for this weapon     // g
        if (weaponModel != null) Destroy(weaponModel);
        weaponModel = Instantiate<GameObject>(def.weaponModelPrefab, transform);
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localScale = Vector3.one;

        nextShotTime = 0; // You can fire immediately after _type is set.    // h
    }

    private void Fire()
{
    if (Main.GAME_PAUSED) return;
    if (!gameObject.activeInHierarchy) return;
    if (Time.time < nextShotTime) return;

    Hero hero = GetComponentInParent<Hero>();
    int projectileBonus = 0;

    if (hero != null)
    {
        projectileBonus = hero.projectileBonus;
    }

    Vector3 vel = Vector3.up * def.velocity;

    switch (type)
    {
        case eWeaponType.blaster:
        {
            ProjectileHero p = MakeProjectile();
            p.vel = vel;

            List<float> extraAngles = GetBonusShotAngles(projectileBonus);
            foreach (float angle in extraAngles)
            {
                ProjectileHero extraP = MakeProjectile();
                extraP.transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
                extraP.vel = extraP.transform.rotation * vel;
            }
            break;
        }

        case eWeaponType.spread:
        {
            List<float> baseAngles = new List<float>() { 0f, 10f, -10f };

            foreach (float angle in baseAngles)
            {
                ProjectileHero p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
                p.vel = p.transform.rotation * vel;
            }

            List<float> extraAngles = GetBonusShotAngles(projectileBonus);
            foreach (float angle in extraAngles)
            {
                ProjectileHero extraP = MakeProjectile();
                extraP.transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
                extraP.vel = extraP.transform.rotation * vel;
            }
            break;
        }
    }
}

    List<float> GetBonusShotAngles(int projectileBonus)
{
    List<float> angles = new List<float>();

    if (projectileBonus <= 0) return angles;

    float step = 8f;

    for (int i = 1; i <= projectileBonus; i++)
    {
        int pairIndex = (i + 1) / 2;
        float angle = step * pairIndex;

        if (i % 2 == 1)
            angles.Add(-angle);
        else
            angles.Add(angle);
    }

    return angles;
}

    private ProjectileHero MakeProjectile()
{
    GameObject go;
    go = Instantiate<GameObject>(def.projectilePrefab, PROJECTILE_ANCHOR);
    ProjectileHero p = go.GetComponent<ProjectileHero>();

    Vector3 pos = shotPointTrans.position;
    pos.z = 0;
    p.transform.position = pos;

    p.type = type;

    Hero hero = GetComponentInParent<Hero>();
    float fireRateMultiplier = 1f;
    float damageMultiplier = 1f;

    if (hero != null)
    {
        fireRateMultiplier = hero.fireRateMultiplier;
        damageMultiplier = hero.damageMultiplier;
    }

    p.damageOnHit = def.damageOnHit * damageMultiplier;

    float adjustedDelay = def.delayBetweenShots / fireRateMultiplier;
    nextShotTime = Time.time + adjustedDelay;

    return p;
}
}


