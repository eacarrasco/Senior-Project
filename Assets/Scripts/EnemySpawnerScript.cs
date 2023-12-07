using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawnerScript : MonoBehaviour
{
    [SerializeField]
    private float lastSpawn = 0f;
    [SerializeField]
    private float spawnTime = 10f;
    [SerializeField]
    private float minSpawnX;
    [SerializeField]
    private float maxSpawnX;

    private Transform player;

    private void Start()
    {
        lastSpawn += Time.time;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (Time.time > lastSpawn + spawnTime)
        {
            if (player.position.x > minSpawnX && player.position.x < maxSpawnX)
            {
                Transform[] newSpawn = EnemyPool.Instance.GetFromPool().GetComponentsInChildren<Transform>();
                newSpawn[1].position = transform.position;
            }
            lastSpawn = Time.time;
        }
    }
}
