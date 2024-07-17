using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using MoreMountains.Feedbacks;
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
            this.transform.GetChild(2).gameObject.SetActive(true);
            LevelManager.Instance.Players[0].GetComponent<Character>()._animator.ResetTrigger("inDoor");
            this.GetComponent<SpriteRenderer>().material = Resources.Load<Material>("Materials/Custom_Outline_Door");
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // 检查离开的物体是否是玩家
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false; // 玩家离开触发区域
            this.transform.GetChild(2).gameObject.SetActive(false);
            LevelManager.Instance.Players[0].GetComponent<Character>()._animator.ResetTrigger("inDoor");
            this.GetComponent<SpriteRenderer>().material = Resources.Load<Material>("Materials/LitSprite");
        }
    }
    
    void Update()
    {
        // 如果玩家在触发区域内并且按下了E键，则执行FunctionA
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            if (KillsManager.Instance.RemainingDeaths == 0)
            {
                // 过0.5s（玩家进门动画时间）进入下一场景
                print("woc, 开门！");
                LevelManager.Instance.Players[0].GetComponent<Character>()._animator.SetTrigger("inDoor");
                Invoke(nameof(GTNL),0.5f);
            }
        }
    }

    /// <summary>
    /// 去下一关1
    /// </summary>
    public void GTNL()
    {
        FinishLevel fl = this.gameObject.GetComponent<FinishLevel>();
        LevelChooseManager.Instance.GoToNextLevel(fl.doorID);
        fl.GoToNextLevelButNotIE();
    }
}
