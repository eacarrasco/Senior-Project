using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawnerScript : MonoBehaviour
{
    private float lastSpawn = 0f;
    private float spawnTime = 10f;

    private void Start()
    {
        lastSpawn = Time.time;
    }

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
