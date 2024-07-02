using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class Drivefr : MonoBehaviour
{
    public GameObject gtnl;
    // Start is called before the first frame update
    void Start()
    {
        // LevelChooseManager.Instance.GoToNextLevel(0);
        gtnl.GetComponent<FinishLevel>().GoToNextLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
