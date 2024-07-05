using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SubmitName : MonoBehaviour
{
    public GameObject Username_field;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// 设置玩家名字
    /// </summary>
    /// <param name="s"></param>
    public void SetPlayerName()
    {
        string s = Username_field.GetComponent<TMP_InputField>().text;
        PlayerManager.Instance.SetName(s);
    }
}
