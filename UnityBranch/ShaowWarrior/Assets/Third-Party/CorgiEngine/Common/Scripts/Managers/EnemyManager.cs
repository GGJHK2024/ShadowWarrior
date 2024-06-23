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
    public int enemyCount = KillsManager.Instance.DeathThreshold;
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
        enemyCount = KillsManager.Instance.DeathThreshold;
        _enemyCount = enemyCount;
    }
    
    public void OnMMEvent(MMGameEvent eventType)
    {
        switch (eventType.EventName) {
            case GameEventType.Dead: {
                KillsManager.Instance.ComputeKillThresholdBasedOnTargetLayerMask();
                KillsManager.Instance.RefreshRemainingDeaths();
                Reset();
                return;
            }
        }
    }
    public void OnMMEvent(CorgiEngineEvent eventType)
    {
        switch (eventType.EventType) {
            case CorgiEngineEventTypes.LevelStart: {
                KillsManager.Instance.ComputeKillThresholdBasedOnTargetLayerMask();
                KillsManager.Instance.RefreshRemainingDeaths();
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
