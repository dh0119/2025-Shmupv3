using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{

    static public Hero S { get; private set; }  // Singleton property    // a

    [Header("Inscribed")]
    // These fields control the movement of the ship
    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40;
    public Weapon[] weapons;

    [Header("Dynamic")]
    [Range(0, 4)]
    [SerializeField]                                        // b
    private float _shieldLevel = 1;

    [Tooltip("This field holds a reference to the last triggering GameObject")]
    private GameObject lastTriggerGo = null;

    [Header("Upgrade Stats")]
    public int projectileBonus = 0;
    public float fireRateMultiplier = 1f;
    public float damageMultiplier = 1f;

    public bool missileUnlocked = false;
    public bool phaserUnlocked = false;
    public bool laserUnlocked = false;
    public bool combineWeaponsUnlocked = false;

    // Declare a new delegate type WeaponFireDelegate
    public delegate void WeaponFireDelegate();                                // a     // Create a WeaponFireDelegate event named fireEvent.
    public event WeaponFireDelegate fireEvent;



    void Awake()
    {
        if (S == null)
        {
            S = this; // Set the Singleton only if it’s null                  // c
        }
        else
        {
            Debug.LogError("Hero.Awake() - Attempted to assign second Hero.S!");
        }
        //fireEvent += TempFire;

        // Reset the weapons to start _Hero with 1 blaster
        ClearWeapons();
        weapons[0].SetType(eWeaponType.blaster);
    }

    void Update()
    {
        if (Main.GAME_PAUSED) return;

        // Pull in information from the Input class
        float hAxis = Input.GetAxis("Horizontal");                            // d
        float vAxis = Input.GetAxis("Vertical");                              // d

        // Change transform.position based on the axes
        Vector3 pos = transform.position;
        pos.x += hAxis * speed * Time.deltaTime;
        pos.y += vAxis * speed * Time.deltaTime;
        transform.position = pos;

        // Rotate the ship to make it feel more dynamic                       // e
        transform.rotation = Quaternion.Euler(vAxis * pitchMult, hAxis * rollMult, 0);

        // Allow the ship to fire
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    TempFire();
        //}

        // Use the fireEvent to fire Weapons when the Spacebar is pressed.
        if (Input.GetAxis("Jump") == 1 && fireEvent != null)
        {
            fireEvent();
        }

    }


    //void TempFire()
    //{
    //    GameObject projGO = Instantiate<GameObject>(projectilePrefab);
    //    projGO.transform.position = transform.position;
    //    Rigidbody rigidB = projGO.GetComponent<Rigidbody>();
    //    //rigidB.velocity = Vector3.up * projectileSpeed;

    //    ProjectileHero proj = projGO.GetComponent<ProjectileHero>();         // h
    //    proj.type = eWeaponType.blaster;
    //    float tSpeed = Main.GET_WEAPON_DEFINITION(proj.type).velocity;
    //    rigidB.velocity = Vector3.up * tSpeed;

    //}

    void OnTriggerEnter(Collider other)
{
    GameObject directGO = other.gameObject;
    GameObject rootGO = other.transform.root.gameObject;

    // Prevent repeated trigger spam from the exact same object
    if (rootGO == lastTriggerGo || directGO == lastTriggerGo) return;

    Enemy enemy = rootGO.GetComponent<Enemy>();
    if (enemy == null) enemy = directGO.GetComponent<Enemy>();

    PowerUp pUp = rootGO.GetComponent<PowerUp>();
    if (pUp == null) pUp = directGO.GetComponent<PowerUp>();

    EnemyProjectile enemyProj = directGO.GetComponent<EnemyProjectile>();
    if (enemyProj == null) enemyProj = rootGO.GetComponent<EnemyProjectile>();

    if (enemy != null)
    {
        lastTriggerGo = rootGO;
        shieldLevel--;

        if (!enemy.isBoss)
        {
            Destroy(rootGO);
        }
    }
    else if (enemyProj != null)
    {
        lastTriggerGo = directGO;
        Debug.Log("Hero hit by enemy projectile: " + directGO.name);
        shieldLevel -= enemyProj.damage;
        Destroy(rootGO);
    }
    else if (pUp != null)
    {
        lastTriggerGo = rootGO;
        CollectUpgradeCrate(pUp);
    }
    else
    {
        Debug.LogWarning("Shield trigger hit by unknown object: " + directGO.name);
    }
}
    void OnTriggerExit(Collider other)
{
    if (other.gameObject == lastTriggerGo || other.transform.root.gameObject == lastTriggerGo)
    {
        lastTriggerGo = null;
    }
}

    public float shieldLevel
    {
        get { return (_shieldLevel); }                                      // b
        private set
        {                                                         // c
            _shieldLevel = Mathf.Min(value, 4);                             // d
            // If the shield is going to be set to less than zero…
            if (value < 0)
            {                                                  // e
                Destroy(this.gameObject);  // Destroy the Hero
                Main.HERO_DIED();
            }
        }
    }

    /// <summary>
    /// Finds the first empty Weapon slot (i.e., type=none) and returns it.
    /// </summary>
    /// <returnsThe first empty Weapon slot or null if none are empty</returns
    Weapon GetEmptyWeaponSlot()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].type == eWeaponType.none)
            {
                return (weapons[i]);
            }
        }
        return (null);
    }

    /// <summary>
    /// Sets the type of all Weapon slots to none
    /// </summary>
    void ClearWeapons()
    {
        foreach (Weapon w in weapons)
        {
            w.SetType(eWeaponType.none);
        }
    }

public void CollectUpgradeCrate(PowerUp pUp)
{
    Main.S.ShowUpgradeSelection(pUp.dropType);
    pUp.AbsorbedBy(this.gameObject);
}

public void ApplyUpgrade(string upgradeId)
{
    Debug.Log("Applying upgrade: " + upgradeId);

    switch (upgradeId)
    {
        case "projectile":
            projectileBonus += 1;
            break;

        case "firerate":
            fireRateMultiplier *= 1.2f;
            break;

        case "damage":
            damageMultiplier *= 1.2f;
            break;

        case "missile":
            missileUnlocked = true;
            break;

        case "phaser":
            phaserUnlocked = true;
            break;

        case "laser":
            laserUnlocked = true;
            break;

        case "shield":
            shieldLevel += 1;
            break;

        case "combine":
            combineWeaponsUnlocked = true;
            break;
    }

    Debug.Log(
        "Upgrades => ProjectileBonus: " + projectileBonus +
        " | FireRate x" + fireRateMultiplier +
        " | Damage x" + damageMultiplier +
        " | Missile: " + missileUnlocked +
        " | Phaser: " + phaserUnlocked +
        " | Laser: " + laserUnlocked +
        " | Combine: " + combineWeaponsUnlocked
    );
}

}
