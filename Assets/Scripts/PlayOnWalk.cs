using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOnWalk : MonoBehaviour
{
    public AudioSource walkSoundEffect;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                  if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow))
                  {
                      walkSoundEffect.loop = true;
                      walkSoundEffect.Play();
                  }
            }

            if (!Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow))
            {

                walkSoundEffect.Stop();
            }
        }
    }
}
