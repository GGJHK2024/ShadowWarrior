using System;
using UnityEngine;

namespace Framework
{
    public class Driver : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.name = "GameManager";
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<GameManager>();
        }
    }
}
