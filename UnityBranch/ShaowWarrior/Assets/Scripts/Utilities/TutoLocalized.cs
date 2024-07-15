using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using UnityEngine;
using UnityEngine.UI;

public class TutoLocalized : MonoBehaviour
{
    public Image s;
    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance.language == "Chinese")
        {
            print("chinese");
            s.sprite = Resources.Load<Sprite>("Tutorial/tut");
        }else
        {
            print("english");
            s.sprite = Resources.Load<Sprite>("Tutorial/tut_EN");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
