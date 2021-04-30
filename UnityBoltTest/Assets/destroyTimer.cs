using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyTimer : MonoBehaviour
{
    private float _time = 2.0f;

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(_time);
        Destroy(gameObject);
    }

}
