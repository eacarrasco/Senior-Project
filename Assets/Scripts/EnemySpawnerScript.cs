using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerScript : MonoBehaviour
{
    private float lastSpawn = 0f; //Make higher to add initial spawn delay
    private float spawnTime = 5f;

    void Update()
    {
        if (Time.time > lastSpawn + spawnTime)
        {
            GameObject newSpawn = EnemyPool.Instance.GetFromPool();
            Transform newSpawnTransform = newSpawn.GetComponent<Transform>();
            newSpawnTransform.position = transform.position;
            newSpawnTransform.rotation = transform.rotation;
            lastSpawn = Time.time;
        }
    }
}
