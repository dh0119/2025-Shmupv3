using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eWeaponType
{
    none,
    blaster,
    spread,
    phaser,
    missile,
    laser,
    shield
}

[System.Serializable]
public class WeaponDefinition
{
    public eWeaponType type = eWeaponType.none;
    public string letter;
    public Color powerUpColor = Color.white;
    public GameObject weaponModelPrefab;
    public GameObject projectilePrefab;
    public Color projectileColor = Color.white;
    public float damageOnHit = 0;
    public float damagePerSec = 0;
    public float delayBetweenShots = 0;
    public float velocity = 50;

    public Vector3 modelPositionOffset = Vector3.zero;
    public Vector3 modelRotationOffset = Vector3.zero;
    public Vector3 modelScale = Vector3.one;
}

public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Dynamic")]
    [SerializeField]
    [Tooltip("Setting this manually while playing does not work properly.")]
    private eWeaponType _type = eWeaponType.none;

    public WeaponDefinition def;
    public float nextShotTime;

    private GameObject weaponModel;
    [SerializeField] private Transform shotPointTrans;

    void Start()
    {
        if (PROJECTILE_ANCHOR == null)
        {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        if (shotPointTrans == null)
        {
            Transform foundShotPoint = transform.Find("ShotPoint");
            if (foundShotPoint != null)
            {
                shotPointTrans = foundShotPoint;
            }
        }

        SetType(_type);

        Hero hero = GetComponentInParent<Hero>();
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
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }

        def = Main.GET_WEAPON_DEFINITION(_type);

        if (weaponModel != null) Destroy(weaponModel);

        if (def.weaponModelPrefab != null)
        {
            weaponModel = Instantiate(def.weaponModelPrefab, transform);
            weaponModel.transform.localPosition = def.modelPositionOffset;
            weaponModel.transform.localRotation = Quaternion.Euler(def.modelRotationOffset);
            weaponModel.transform.localScale = def.modelScale;
        }

        nextShotTime = 0;
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
                if (p != null) p.vel = vel;

                List<float> extraAngles = GetBonusShotAngles(projectileBonus);
                foreach (float angle in extraAngles)
                {
                    ProjectileHero extraP = MakeProjectile();
                    if (extraP != null)
                    {
                        extraP.transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
                        extraP.vel = extraP.transform.rotation * vel;
                    }
                }
                break;
            }

            case eWeaponType.spread:
            {
                List<float> baseAngles = new List<float>() { 0f, 10f, -10f };

                foreach (float angle in baseAngles)
                {
                    ProjectileHero p = MakeProjectile();
                    if (p != null)
                    {
                        p.transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
                        p.vel = p.transform.rotation * vel;
                    }
                }

                List<float> extraAngles = GetBonusShotAngles(projectileBonus);
                foreach (float angle in extraAngles)
                {
                    ProjectileHero extraP = MakeProjectile();
                    if (extraP != null)
                    {
                        extraP.transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
                        extraP.vel = extraP.transform.rotation * vel;
                    }
                }
                break;
            }

            case eWeaponType.phaser:
            {
                ProjectileHero p = MakeProjectile();
                if (p != null) p.vel = vel;
                break;
            }

            case eWeaponType.missile:
            {
                ProjectileHero p = MakeProjectile();
                if (p != null) p.vel = vel;
                break;
            }

            case eWeaponType.laser:
            {
                FireLaser();
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

    void FireLaser()
    {
        if (shotPointTrans == null)
        {
            Debug.LogError("Weapon " + name + " is missing ShotPoint.");
            return;
        }

        GameObject go = Instantiate(def.projectilePrefab, PROJECTILE_ANCHOR);

        Vector3 pos = shotPointTrans.position;
        pos.z = 0;
        go.transform.position = pos;
        go.transform.rotation = Quaternion.identity;

        Hero hero = GetComponentInParent<Hero>();
        float fireRateMultiplier = 1f;
        float damageMultiplier = 1f;

        if (hero != null)
        {
            fireRateMultiplier = hero.fireRateMultiplier;
            damageMultiplier = hero.damageMultiplier;
        }

        ProjectileLaser laser = go.GetComponent<ProjectileLaser>();
        if (laser != null)
        {
            laser.damage = def.damageOnHit * damageMultiplier;
        }

        float adjustedDelay = def.delayBetweenShots / fireRateMultiplier;
        nextShotTime = Time.time + adjustedDelay;
    }

    private ProjectileHero MakeProjectile()
    {
        if (shotPointTrans == null)
        {
            Debug.LogError("Weapon " + name + " is missing ShotPoint.");
            return null;
        }

        GameObject go = Instantiate(def.projectilePrefab, PROJECTILE_ANCHOR);
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


