using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class EnemyConfig
{
    public GameObject enemy;
    public float probability;
}

public class EnemySpawner: MonoBehaviour
{
    public EnemyConfig[] configs;
    public float spawnRadius = 20;
    public float delay = 3;

    private void Start()
    {
        ModeSwitcher.modeChanged.AddListener((mode) =>
        {
            if (mode == ModeSwitcher.Mode.Battle)
            {
                StartCoroutine("SpawnEnemies");
            }
            else
            {
                StopCoroutine("SpawnEnemies");
            }
        });
    }

    private IEnumerator SpawnEnemies()
    {
        var probSum = configs.Sum((config => config.probability));
        while (true)
        {
            var rand = Random.Range(0, probSum);
            var probCum = 0f;
            GameObject enemyPrefab = null;
            foreach (var enemyConfig in configs)
            {
                if (rand > probCum && rand < probCum + enemyConfig.probability)
                {
                    enemyPrefab = enemyConfig.enemy;
                    break;
                }

                probCum += enemyConfig.probability;
            }

            var position = Random.onUnitSphere;
            position.z = 0;
            position = position.normalized * spawnRadius;
            var enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
            yield return new WaitForSeconds(delay);
        }
    }

}