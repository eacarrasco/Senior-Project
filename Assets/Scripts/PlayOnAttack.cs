using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOnAttack : MonoBehaviour
{
    public AudioSource keySound1;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
      if(Input.GetKeyDown(KeyCode.X))
        keySound1.Play();
    }
}
