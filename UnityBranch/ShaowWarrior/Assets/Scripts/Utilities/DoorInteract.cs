using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class DoorInteract : MonoBehaviour
{
    private BoxCollider2D boxCollider2D;
    private bool isPlayerInside = false;
    
    // Start is called before the first frame update
    void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        boxCollider2D.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 检查进入的物体是否是玩家
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerInside = true;
        }
        this.transform.GetChild(2).gameObject.SetActive(true);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // 检查离开的物体是否是玩家
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false; // 玩家离开触发区域
        }
        this.transform.GetChild(2).gameObject.SetActive(false);
    }
    
    void Update()
    {
        // 如果玩家在触发区域内并且按下了E键，则执行FunctionA
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            // GUIManager.Instance.OpenShop();
            FinishLevel fl = this.gameObject.GetComponent<FinishLevel>();
            if (KillsManager.Instance.RemainingDeaths == 0)
            {
                LevelChooseManager.Instance.GoToNextLevel(fl.doorID);
                fl.GoToNextLevelButNotIE();
            }
        }
    }
}
