using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;

public class EnemyManager : MMSingleton<EnemyManager>,		
    MMEventListener<MMGameEvent>,
    MMEventListener<CorgiEngineEvent>
{
    public int enemyCount = 0;
    private int _enemyCount;
    
    /// <summary>
    /// Statics initialization to support enter play modes
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    protected static void InitializeStatics()
    {
        _instance = null;
    }

    private void Start()
    {
        Reset();
        MMEventManager.AddListener<CorgiEngineEvent>(this);
        MMEventManager.AddListener<MMGameEvent>(this);
    }

    public void BeKilled()
    {
        _enemyCount--;
        print("current reaming: " + _enemyCount);
    }

    public int RemainingEnemies()
    {
        return _enemyCount;
    }

    public void Reset()
    {
        print("重置");
        _enemyCount = enemyCount;
    }
    
    public void OnMMEvent(MMGameEvent eventType)
    {
        switch (eventType.EventName) {
            case GameEventType.Dead: {
                Reset();
                return;
            }
        }
    }
    public void OnMMEvent(CorgiEngineEvent eventType)
    {
        switch (eventType.EventType) {
            case CorgiEngineEventTypes.LevelStart: {
                Reset();
                return;
            }
            case CorgiEngineEventTypes.LevelComplete: {
                return;
            }
            case CorgiEngineEventTypes.LevelEnd: {
                return;
            }
        }
    }
}
