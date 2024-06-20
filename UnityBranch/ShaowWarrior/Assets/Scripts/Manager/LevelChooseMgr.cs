using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

public class LevelChooseMgr : MMSingleton<LevelChooseMgr>
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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*public void InitLevelList(List<LevelStruct> list)
    {
        list.Add(new LevelStruct("CG",0,-1));
        // CG --> Tutorial
        list.Add(new LevelStruct("Tutorial",1,0));
        // Tutorial --> Normal1
        list.Add(new LevelStruct("Normal1",2,1));
        // Normal1 --> Normal2
        //         --> Toilet
        list.Add(new LevelStruct("Normal2",3,2));
        list.Add(new LevelStruct("Toilet",4,2));
        // Normal2 --> RestRoom
        //         --> Normal2
        list.Add(new LevelStruct("RestRoom",5,3));
        list.Add(new LevelStruct("Normal2",6,3));
        // Toilet --> Normal2
        //        --> RestRoom
        list.Add(new LevelStruct("Normal2",7,4));
        list.Add(new LevelStruct("RestRoom",8,4));
        // RestRoom --> Toilet
        //         --> Normal3
        list.Add(new LevelStruct("Toilet",9,5));
        list.Add(new LevelStruct("Normal3",10,5));
        // Normal2 --> Normal3
        //         --> Hard1
        list.Add(new LevelStruct("Normal2",11,6));
        list.Add(new LevelStruct("RestRoom",12,6));
        // RestRoom --> Hard1
        //          --> Normal2
        list.Add(new LevelStruct("Normal2",13,7));
        list.Add(new LevelStruct("RestRoom",14,7));
        // Normal2 --> RestRoom
        //         --> Normal2
        list.Add(new LevelStruct("Normal2",15,8));
        list.Add(new LevelStruct("RestRoom",16,8));
        
        list.Add(new LevelStruct("Normal2",17,9));
        list.Add(new LevelStruct("RestRoom",18,9));
    }*/
}
