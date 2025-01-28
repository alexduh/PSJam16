using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    private float SPAWN_TIME = 10f;
    [SerializeField] private float enemySpawnTimer;

    void Start()
    {
        // TODO: initialize player, map, enemies
        Player.Instance.Initialize();
        enemySpawnTimer = SPAWN_TIME;
        // TODO: 
    }

    void SpawnEnemy(Vector3 spawnPos)
    {
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    void Update()
    {
        enemySpawnTimer -= Time.deltaTime;
        if (enemySpawnTimer <= 0)
        {
            float xSpawn = Random.Range(-10f, 10f);
            float ySpawn = Random.Range(-10f, 10f);
            while (Mathf.Abs(xSpawn) + Mathf.Abs(ySpawn) <= 5)
            {
                xSpawn = Random.Range(-10f, 10f);
                ySpawn = Random.Range(-10f, 10f);
            }

            SpawnEnemy(new Vector3(xSpawn, ySpawn, 0));
            enemySpawnTimer = SPAWN_TIME;
        }
    }
}
