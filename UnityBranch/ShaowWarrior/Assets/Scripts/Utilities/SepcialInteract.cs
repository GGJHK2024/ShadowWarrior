using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;

public class SepcialInteract : MonoBehaviour
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
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // 检查离开的物体是否是玩家
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false; // 玩家离开触发区域
        }
    }
    void Update()
    {
        // 如果玩家在触发区域内并且按下了E键，则执行FunctionA
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            GUIManager.Instance.OpenSpecialEvent();
        }
    }
}
