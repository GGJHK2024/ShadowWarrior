using System;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.CorgiEngine
{
    [System.Serializable]
    public class LevelData
    {
        public List<LevelChooseManager.LevelStruct> levels;
    }
    
    public class LevelChooseManager : MMSingleton<LevelChooseManager>,
        MMEventListener<CorgiEngineEvent>
    {
        [System.Serializable]
        public struct LevelStruct
        {
            public string name;
            public int id;
            public int[] childId;
        }
        
    
        public List<LevelStruct> levels = new List<LevelStruct>();

        public int curID;
        public int stage;
        public List<int> nextLevel;
        public FinishLevel[] gates;

        void Awake()
        {
            LoadLevelData();
        }
        
        // Start is called before the first frame update
        void Start()
        {
            MMEventManager.AddListener<CorgiEngineEvent>(this);
            // 第一次打开游戏
            curID = 0;
            stage = -1;
            nextLevel = new List<int>();
            InitLevel(curID);
            InitNextLevelGate();
        }
        
        /// <summary>
        /// 从JSON中读取关卡信息
        /// </summary>
        void LoadLevelData()
        {
            // 读取文件
            TextAsset jsonFile = Resources.Load<TextAsset>("Configs/Levels");
            if(jsonFile != null)
            {
                LevelData levelData = JsonUtility.FromJson<LevelData>(jsonFile.text);
                levels = levelData.levels;
                Debug.Log("成功加载关卡数据！");
                print(jsonFile.text);
            }
            else
            {
                Debug.LogError("未找到 Levels.json 文件！");
            }
        }

        public void InitLevel(int id)
        {
            // child level number(0/1/2)
            print("cur first child: " + levels[id].childId[0]);
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
            print("i: " + i);
            print("curID: " + levels[curID]);
            curID = levels[curID].childId[i];
            stage++;
            print("curID in GOTONEXTLEVEL: " + curID);
        }
        
        /// <summary>
        /// 初始化门，理论上在加载新关卡（LM）中调用
        /// </summary>
        public void InitNextLevelGate()
        {
            if(SceneManager.GetActiveScene().name.Contains("Driver"))   return;
            if (SceneManager.GetActiveScene().name.Contains("Tuto"))
            {
                curID = 1;
                stage = 0;
            }
            print("---init next level---");
            gates= FindObjectsOfType(typeof(FinishLevel)) as FinishLevel[];
            int cn = levels[curID].childId.Length;
            foreach(var g in gates)
            {
                print("cur id: " + curID);
                if (cn == 0)
                {
                    g.gameObject.SetActive(false);  // 最后只有一个门的情况，我们只保留一扇门而隐藏另一扇
                    break;
                }
                g.LevelName = levels[levels[curID].childId[cn - 1]].name;
                g.gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite =
                    Resources.Load<Sprite>("DoorIcons/" + g.LevelName);
                cn--;
            }
        }

        /// <summary>
        /// 重置关卡进度
        /// </summary>
        public void ResetLevelProgress()
        {
            curID = 0;
            stage = -1;
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
