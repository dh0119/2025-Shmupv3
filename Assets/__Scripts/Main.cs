using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;   // Enables the loading & reloading of scenes

[RequireComponent(typeof(BoundsCheck))]
public class Main : MonoBehaviour
{
    static public Main S;                        // A private singleton for Main
    static private Dictionary<eWeaponType, WeaponDefinition> WEAP_DICT;
    public static bool GAME_PAUSED = false;

    [Header("Run Progress")]
    public int score = 0;
    public int totalKills = 0;
    public int difficultyTier = 1;
    public bool bossActive = false;

    [Header("Difficulty Settings")]
    public int killsPerTier = 30;
    public int killsPerBoss = 60;
    public float spawnRateIncreasePerTier = 0.12f;
    public float speedIncreasePerTier = 0.10f;

    [Header("Boss")]
    public GameObject bossPrefab;

    [Header("Inscribed")]
    public bool spawnEnemies = true;
    public GameObject[] prefabEnemies;               // Array of Enemy prefabs
    public float enemySpawnPerSecond = 0.5f;  // # Enemies spawned/second
    public float enemyInsetDefault = 1.5f;    // Inset from the sides
    public float gameRestartDelay = 2.0f;
    public GameObject prefabPowerUp;
    public WeaponDefinition[] weaponDefinitions;
    public eWeaponType[] powerUpFrequency = new eWeaponType[] {        
                                     eWeaponType.blaster, eWeaponType.blaster,
                                     eWeaponType.spread,  eWeaponType.shield };
    private BoundsCheck bndCheck;

    GameObject GetRandomEnemyForTier()
{
    List<GameObject> pool = new List<GameObject>();

    if (prefabEnemies.Length > 0) pool.Add(prefabEnemies[0]); // level 1 basic

    if (difficultyTier >= 1 && prefabEnemies.Length > 1) pool.Add(prefabEnemies[1]);
    if (difficultyTier >= 2 && prefabEnemies.Length > 2) pool.Add(prefabEnemies[2]);
    if (difficultyTier >= 3 && prefabEnemies.Length > 3) pool.Add(prefabEnemies[3]);
    if (difficultyTier >= 4 && prefabEnemies.Length > 4) pool.Add(prefabEnemies[4]);

    if (pool.Count == 0) return null;

    int ndx = Random.Range(0, pool.Count);
    return pool[ndx];
}

    void Awake()
    {
        S = this;
        // Set bndCheck to reference the BoundsCheck component on this 
        // GameObject
        bndCheck = GetComponent<BoundsCheck>();

        // Invoke SpawnEnemy() once (in 2 seconds, based on default values)
        Invoke(nameof(SpawnEnemy), CurrentSpawnDelay);                // a

        // A generic Dictionary with eWeaponType as the key
        WEAP_DICT = new Dictionary<eWeaponType, WeaponDefinition>();          // a
        foreach (WeaponDefinition def in weaponDefinitions)
        {
            WEAP_DICT[def.type] = def;
        }

    }

    public float CurrentSpawnDelay
    {
        get
        {
            float currentSpawnRate = enemySpawnPerSecond * Mathf.Pow(1f + spawnRateIncreasePerTier, difficultyTier - 1);
            return 1f / currentSpawnRate;
        }
    }

    public float CurrentEnemySpeedMultiplier
    {
        get
        {
            return Mathf.Pow(1f + speedIncreasePerTier, difficultyTier - 1);
        }
    }


    public void SpawnEnemy()
    {
        if (GAME_PAUSED)
        {                                                // c
            Invoke(nameof(SpawnEnemy), 0.25f);
            return;
        }

        if (!spawnEnemies)
        {
            Invoke(nameof(SpawnEnemy), CurrentSpawnDelay);
            return;
        }

        if(bossActive)
        {
            Invoke(nameof(SpawnEnemy), 0.5f);
            return;
        }

        GameObject prefabToSpawn = GetRandomEnemyForTier();
        if(prefabToSpawn == null)
        {
            Invoke(nameof(SpawnEnemy), CurrentSpawnDelay);
            return;
        }

        GameObject go = Instantiate(prefabToSpawn);

        float enemyInset = enemyInsetDefault;
        BoundsCheck enemyBounds = go.GetComponent<BoundsCheck>();
        if (enemyBounds != null)
        {
            enemyInset = Mathf.Abs(enemyBounds.radius);
        }

        // Set the initial position for the spawned Enemy                    // f
        Vector3 pos = Vector3.zero;
        float xMin = -bndCheck.camWidth + enemyInset;
        float xMax = bndCheck.camWidth - enemyInset;
        pos.x = Random.Range(xMin, xMax);
        pos.y = bndCheck.camHeight + enemyInset;
        go.transform.position = pos;

        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.speed *= CurrentEnemySpeedMultiplier;
        }
        // Invoke SpawnEnemy() again
        Invoke(nameof(SpawnEnemy), CurrentSpawnDelay);                // g
    }

    void DelayedRestart()
    {                                                   // c
                                                        // Invoke the Restart() method in gameRestartDelay seconds
        Invoke(nameof(Restart), gameRestartDelay);
    }

    void Restart()
    {
        // Reload __Scene_0 to restart the game
        // "__Scene_0" below starts with 2 underscores and ends with a zero.
        SceneManager.LoadScene("__Scene_0");                               // d
    }

    static public void HERO_DIED()
    {
        S.DelayedRestart();                                                  // b
    }

    /// <summary>
    /// Static function that gets a WeaponDefinition from the WEAP_DICT static
    ///   protected field of the Main class.
    /// </summary>
    /// <returns>The WeaponDefinition, or if there is no WeaponDefinition with
    ///   the eWeaponType passed in, returns a new WeaponDefinition with a 
    ///   eWeaponType of eWeaponType.none.</returns>
    /// <param name="wt">The eWeaponType of the desired WeaponDefinition</param>
    static public WeaponDefinition GET_WEAPON_DEFINITION(eWeaponType wt)
    {  // a
        if (WEAP_DICT.ContainsKey(wt))
        {                                      // b
            return (WEAP_DICT[wt]);
        }
        // If no entry of the correct type exists in WEAP_DICT, return a new 
        //   WeaponDefinition with a type of eWeaponType.none (the default value)
        return (new WeaponDefinition());                                     // c
    }

    /// <summary>
    /// Called by an Enemy ship whenever it is destroyed. It sometimes creates
    ///   a PowerUp in place of the destroyed ship.
    /// </summary>
    /// <param name="e"The Enemy that was destroyed</param
    static public void SHIP_DESTROYED(Enemy e)
{
    if (S == null || e == null) return;

    // Add score
    S.score += e.score;

    // Count kills
    if (!e.isBoss)
    {
        S.totalKills += e.killValue;

        S.CheckDifficultyIncrease();
        S.CheckBossSpawn();
    }
    else
    {
        S.bossActive = false;
        Debug.Log("Boss defeated!");
    }


    if (!e.isBoss && Random.value <= e.powerUpDropChance)
        {
            GameObject go = Instantiate(S.prefabPowerUp);
            PowerUp pUp = go.GetComponent<PowerUp>();
            pUp.SetDropType(ePowerUpDropType.normalCrate);
            pUp.transform.position = e.transform.position;
        }

    if (e.isBoss)
        {
            GameObject go = Instantiate(S.prefabPowerUp);
            PowerUp pUp = go.GetComponent<PowerUp>();
            pUp.SetDropType(ePowerUpDropType.bossCrate);
            pUp.transform.position = e.transform.position;
        }
}

    void CheckDifficultyIncrease()
    {
        int targetTier = Mathf.Min(4, (totalKills / killsPerTier) + 1);
        if (targetTier > difficultyTier)
        {
            difficultyTier = targetTier;
            Debug.Log("Difficulty increased to Tier " + difficultyTier);
        }
    }

    void CheckBossSpawn()
{
    if (bossActive) return;
    if (bossPrefab == null) return;

    if (totalKills > 0 && totalKills % killsPerBoss == 0)
    {
        SpawnBoss();
    }
}

void SpawnBoss()
{
    if (bossActive) return;
    if (bossPrefab == null) return;

    bossActive = true;

    GameObject go = Instantiate(bossPrefab);

    Vector3 spawnPos = Vector3.zero;
    spawnPos.x = 0f;
    spawnPos.y = bndCheck.camHeight + 2f;
    go.transform.position = spawnPos;

    Enemy enemy = go.GetComponent<Enemy>();
    if (enemy != null)
        {
            enemy.isBoss = true;
            enemy.score = 500;
            enemy.powerUpDropChance = 1f;
        }

    Debug.Log("Boss spawned!");
}

public void ShowUpgradeSelection(ePowerUpDropType dropType)
{
    if (GAME_PAUSED) return;

    GAME_PAUSED = true;
    Debug.Log("Upgrade selection opened: " + dropType);

    List<string> options = GenerateUpgradeOptions(dropType);

    if (UpgradeUI.S != null)
    {
        UpgradeUI.S.ShowOptions(options);
    }
    else
    {
        Debug.LogWarning("UpgradeUI.S is null. Falling back to auto-pick.");

        int ndx = Random.Range(0, options.Count);
        string chosen = options[ndx];
        Hero.S.ApplyUpgrade(chosen);
        ResumeGameplay();
    }
}

    public void ResumeGameplay()
    {
        GAME_PAUSED = false;
        Debug.Log("Gameplay resumed");
    }

    List<string> GenerateUpgradeOptions(ePowerUpDropType dropType)
    {
        List<string> pool = new List<string>();

        Hero hero = Hero.S;

        pool.Add("projectile");
        pool.Add("firerate");
        pool.Add("damage");
        pool.Add("shield");

        if (!hero.missileUnlocked) pool.Add("missile");
        if (!hero.phaserUnlocked) pool.Add("phaser");
        if (!hero.laserUnlocked) pool.Add("laser");

        if(dropType == ePowerUpDropType.bossCrate && !hero.combineWeaponsUnlocked)
        {
            pool.Add("combine");
        }

        List<string> result = new List<string>();

        while (result.Count < 3 && pool.Count > 0)
            {
                int ndx = Random.Range(0, pool.Count);
                result.Add(pool[ndx]);
                pool.RemoveAt(ndx);
            }

        while (result.Count < 3)
            {
                result.Add("damage");
            }

        return result;
    }

}
