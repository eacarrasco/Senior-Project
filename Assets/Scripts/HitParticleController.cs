using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitParticleController : MonoBehaviour
{
    private void FinishAnim()
    {
        HitParticlePool.Instance.AddToPool(gameObject);
    }
}
