using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawnerScript : MonoBehaviour
{
    private float lastSpawn = 0f; //Make higher to add initial spawn delay
    private float spawnTime = 5f;

    void Update()
    {
        if (Time.time > lastSpawn + spawnTime)
        {
            Transform[] newSpawn = EnemyPool.Instance.GetFromPool().GetComponentsInChildren<Transform>();
            newSpawn[1].position = transform.position;
            lastSpawn = Time.time;
        }
    }
}
