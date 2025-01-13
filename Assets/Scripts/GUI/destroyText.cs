using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyText : MonoBehaviour
{
    bool check = true;
    // Update is called once per frame
    void Update()
    {
        if (check)
        {
            check = false;
            StartCoroutine(wait(0.3f));
        }
    }

    private IEnumerator wait(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }
}
