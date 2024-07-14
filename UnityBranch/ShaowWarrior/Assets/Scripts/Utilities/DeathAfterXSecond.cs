using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class DeathAfterXSecond : MonoBehaviour
{
    public float time = 1;
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(KillMyself), time);
    }

    public void KillMyself()
    {
        this.gameObject.GetComponent<Health>().Kill();
    }
}
