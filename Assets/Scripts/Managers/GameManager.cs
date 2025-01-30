using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    [SerializeField] private int wave_number;
    [SerializeField] private int wave_size;
    [SerializeField] private float wave_timer;
    [SerializeField] private float wave_time;
    [SerializeField] private int kill_threshold;
    private int kill_count;
    static int BASE_KILL_THRESHOLD = 20;
    [SerializeField] GameObject enemies;
    [SerializeField] GameObject spawnLocations;

    void Initialize()
    {
        wave_number = 0;
        wave_size = (int)(BASE_KILL_THRESHOLD * 1.5f);
        kill_count = 0;
        kill_threshold = BASE_KILL_THRESHOLD;
        wave_time = 30;
        wave_timer = wave_time;
        // TODO: reset wave spawn timer, reset wave #, spawn player, map, final chamber enemies
    }

    void Start()
    {
        Initialize();
    }

    void SpawnEnemy()
    {
        Vector3 spawnPos = new Vector3();
        int numSpawns = spawnLocations.transform.hierarchyCount;

        bool charCloseby = true;
        
        while (charCloseby)
        {
            Transform attemptSpawn = spawnLocations.transform.GetChild(Random.Range(0, numSpawns-2));

            if (enemies.transform.hierarchyCount <= 1)
                charCloseby = false;
            // if there is already a character near a location, do not spawn a new one there!
            foreach (Transform enemy in enemies.transform)
            {
                if (Vector3.Distance(attemptSpawn.position, enemy.position) < 10 && Vector3.Distance(attemptSpawn.position, Player.Instance.transform.position) < 10)
                {
                    charCloseby = false;
                    break;
                }
            }

            spawnPos = attemptSpawn.transform.position;
        }

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    void SpawnWave(int size)
    {
        wave_number += 1;
        wave_time *= 1.2f;
        wave_timer = wave_time;
        kill_threshold = (wave_number + 1) * (int)Mathf.Pow(1.2f, (wave_number + 1)) * BASE_KILL_THRESHOLD;
        wave_size = (int)(wave_size * 1.2f);
        for (int i = 0; i < size; i++)
            SpawnEnemy();
    }

    void Update()
    {
        // check wave spawn timer, and check # of enemies killed
        wave_timer -= Time.deltaTime;
        if (wave_timer <= 0 || kill_count >= kill_threshold)
            SpawnWave(wave_size);
    }
}
