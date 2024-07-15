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
        StartCoroutine(waitForCg());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 跳过cg
    /// </summary>
    public void SkipButton()
    {
        StopAllCoroutines();
        gtnl.GetComponent<FinishLevel>().GoToNextLevel();
    }

    IEnumerator waitForCg()
    {
        yield return new WaitForSeconds(31.0f);
        gtnl.GetComponent<FinishLevel>().GoToNextLevel();
        yield return null;
    }
}
