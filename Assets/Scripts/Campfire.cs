using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campfire : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float sizePercent = Manager.Instance.life / 60f;
        if (sizePercent < 0 || sizePercent > 1f)
        {
            gameObject.SetActive(false);
            Manager.Instance.EndGame(sizePercent > 1f); // win if sizePercent > 1f, lose otherwise.
        }
        transform.localScale = Vector2.one * sizePercent * 3f;
    }
}
