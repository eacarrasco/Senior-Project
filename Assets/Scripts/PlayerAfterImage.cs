using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImage : MonoBehaviour
{
    private float imageTimer = 0.1f;
    private float currentImageTime;
    private float alpha;
    private float baseAlpha = 0.5f;
    [SerializeField]
    private float alphaDecay = 1f;
  
    public SpriteRenderer sr;

    private void OnEnable()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        SpriteRenderer psr = player.GetComponent<SpriteRenderer>();

        alpha = baseAlpha;
        sr.sprite = psr.sprite;
        transform.position = player.position;
        transform.rotation = player.rotation;
        currentImageTime = imageTimer;
    }

    private void Update()
    {
        alpha -= alphaDecay * Time.deltaTime;
        currentImageTime -= Time.deltaTime;
        sr.color = new Color(1f, 1f, 1f, alpha);

        if (currentImageTime <= 0)
        {
            PlayerAfterImagePool.Instance.AddToPool(gameObject);
        }
    }
}
