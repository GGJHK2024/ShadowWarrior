using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
    public class LevelChooseManager : MMSingleton<LevelChooseManager>,
        MMEventListener<CorgiEngineEvent>
    {
        [System.Serializable]public struct LevelStruct
        {
            public string name;
            public int id;
            public int[] childId;
            public LevelStruct(string n, int i, int[] p)
            {
                name = n;
                id = i;
                childId = p;
            }
        }
    
        [SerializeField] public List<LevelStruct> levels = new List<LevelStruct>();

        public int curID;
        public List<int> nextLevel;
        public FinishLevel[] gates;
        // Start is called before the first frame update
        void Start()
        {
            MMEventManager.AddListener<CorgiEngineEvent>(this);
            // 第一次打开游戏
            curID = 0;
            InitLevel(curID);
            InitNextLevelGate();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void InitLevel(int id)
        {
            // child level number(0/1/2)
            int cn = levels[id].childId.Length;
            while (cn > 0)
            {
                nextLevel.Add(levels[id].childId[cn - 1]);
                cn--;
            }
        }

        /// <summary>
        /// 在这里我想的是也通过i定位第0或者第1个孩子关卡
        /// </summary>
        /// <param name="i"></param>
        public void GoToNextLevel(int i)
        {
            curID = levels[curID].childId[i];
            print("curID in GOTONEXTLEVEL: " + curID);
        }
        
        /// <summary>
        /// 初始化门，理论上在加载新关卡（LM）中调用
        /// </summary>
        public void InitNextLevelGate()
        {
            print("---init next level---");
            gates= FindObjectsOfType(typeof(FinishLevel)) as FinishLevel[];
            int cn = levels[curID].childId.Length;
            foreach(var g in gates)
            {
                print("cur id: " + curID);
                g.LevelName = levels[levels[curID].childId[cn - 1]].name;
                cn--;
            }
        }
        
        public void OnMMEvent(CorgiEngineEvent eventType)
        {
            switch (eventType.EventType) {
                case CorgiEngineEventTypes.LevelStart:
                {
                    InitNextLevelGate();
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
}
