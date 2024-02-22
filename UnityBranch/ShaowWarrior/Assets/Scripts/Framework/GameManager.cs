using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class GameManager : MonoBehaviour,IManager
    {
        private struct ManagerStruct
        {
            public BasicManager manager;
            public bool needUpdate;
            public bool needFixedUpdate;
            public bool needLateUpdate;
        }

        private struct ManagerCtorStruct
        {
            public ManagerConstructorDelegate ctor;
            public bool needUpdate;
            public bool needFixedUpdate;
            public bool needLateUpdate;
        }

        private static GameManager _instance;

        private Dictionary<string, ManagerCtorStruct> _managerName2CtorDic;
        private Dictionary<string, BasicManager> _managerName2ManagerDic;

        private List<string> _newTempModules;
        private List<ManagerStruct> _frameworkManagers;
        private List<ManagerStruct> _moduleManagers;
        private List<ManagerStruct> _tempModuleManagers;
        private List<int> _removedTempModuleIndexList;

        #region Life Cycle

        private void Awake()
        {
            if (GameManager._instance != null)
            {
                Destroy(this);
                return;
            } 
            GameManager._instance = this;
            DontDestroyOnLoad(this.gameObject);
            this.OnInitialize();
        }

        // Update is called once per frame
        void Update()
        {
            this.OnUpdate(Time.deltaTime);
        }

        void LateUpdate()
        {
            this.OnLateUpdate(Time.deltaTime);
        }
    
        public void OnInitialize()
        {
            this._managerName2ManagerDic = new Dictionary<string, BasicManager>();
            this._managerName2CtorDic = new Dictionary<string, ManagerCtorStruct>();
            this._frameworkManagers = new List<ManagerStruct>();
            this._moduleManagers = new List<ManagerStruct>();
            this._tempModuleManagers = new List<ManagerStruct>();
            this._newTempModules = new List<string>();
            this._removedTempModuleIndexList = new List<int>();
            
            ManagerRegister registerTable = new ManagerRegister();
            this.RegisterFrameWork(registerTable.GetFrameworkManagers());
            
            Debug.Log("正在初始化框架层");
            foreach (var frameworkManager in this._frameworkManagers)
            {
                frameworkManager.manager.OnInitialize();
            }
            
            Debug.Log("正在初始化常驻模块层");
            this.RegisterModule(registerTable.GetMainModuleManagers());
            foreach (var moduleManager in this._moduleManagers)
            {
                moduleManager.manager.OnInitialize();
            }
            this.RegisterTempModule(registerTable.GetTempModuleManagers());
        }

        public void Start()
        {
        }

        public void Destroy()
        {
            for (int i = this._tempModuleManagers.Count - 1; i >= 0; i--)
            {
                this._tempModuleManagers[i].manager.Destroy();
            }

            for (int i = this._moduleManagers.Count - 1; i >= 0; i--)
            {
                this._tempModuleManagers[i].manager.Destroy();                
            }

            for (int i = this._frameworkManagers.Count - 1; i >= 0; i--)
            {
                this._frameworkManagers[i].manager.Destroy();
            }
        }

        /** 主要更新逻辑在Update中，同时也负责一些Manager的初始化，删除 */
        public void OnUpdate(float deltaTime)
        {
            
            for (int i = 0; i < this._frameworkManagers.Count; i++)
            {
                var mgr = this._frameworkManagers[i];
                if (mgr.manager.managerState == ManagerState.Ready)
                {
                    mgr.manager.Start();
                }

                if (mgr.needUpdate && mgr.manager.managerState == ManagerState.Started)
                    mgr.manager.OnUpdate(deltaTime);
            }
            for (int i = 0; i < this._moduleManagers.Count; i++)
            {
                var mgr = this._moduleManagers[i];
                if (mgr.manager.managerState == ManagerState.Ready)
                {
                    mgr.manager.Start();
                }
                if (mgr.needUpdate && mgr.manager.managerState == ManagerState.Started)
                    mgr.manager.OnUpdate(deltaTime);
            }

            // 增加一些非长期Module的Manager
            for (int i = 0; i < this._newTempModules.Count; i++)
            {
                var ctorStruct = this._managerName2CtorDic[this._newTempModules[i]];
                var mgr = ctorStruct.ctor();
                this._managerName2ManagerDic.Add(this._newTempModules[i], mgr);
                this._tempModuleManagers.Add(new ManagerStruct
                {
                    manager = mgr,
                    needUpdate = ctorStruct.needUpdate,
                    needFixedUpdate = ctorStruct.needFixedUpdate,
                    needLateUpdate = ctorStruct.needLateUpdate,
                });
            }
            this._newTempModules.Clear();
            
            for (int i = 0; i < this._tempModuleManagers.Count; i++)
            {
                var mgr = this._tempModuleManagers[i];
                if (mgr.manager.managerState == ManagerState.Ready)
                {
                    mgr.manager.Start();
                }
                if (mgr.needUpdate && mgr.manager.managerState == ManagerState.Started)
                    mgr.manager.OnUpdate(deltaTime);
                if (mgr.manager.managerState == ManagerState.Destroy)
                {
                    this._managerName2ManagerDic.Remove(mgr.manager.GetType().Name);
                    this._removedTempModuleIndexList.Add(i);
                }
            }

            // 从循环列表中删除被成功卸载的Manager
            for (int i = this._removedTempModuleIndexList.Count - 1; i >= 0; i--)
            {
                this._tempModuleManagers.RemoveAt(this._removedTempModuleIndexList[i]);
            }
            this._removedTempModuleIndexList.Clear();

            
        }

        public void OnLateUpdate(float deltaTime)
        {
            for (int i = 0; i < _frameworkManagers.Count; i++)
            {
                var mgr = this._frameworkManagers[i];
                if (mgr.needLateUpdate && mgr.manager.managerState == ManagerState.Started)
                    mgr.manager.OnLateUpdate(deltaTime);
            }
            for (int i = 0; i < _moduleManagers.Count; i++)
            {
                var mgr = this._moduleManagers[i];
                if (mgr.needLateUpdate && mgr.manager.managerState == ManagerState.Started)
                    mgr.manager.OnLateUpdate(deltaTime);
            }
            for (int i = 0; i < _tempModuleManagers.Count; i++)
            {
                var mgr = this._tempModuleManagers[i];
                if (mgr.needLateUpdate && mgr.manager.managerState == ManagerState.Started)
                    mgr.manager.OnLateUpdate(deltaTime);
            }
        }

        public void OnFixedUpdate(float fixedDeltaTime)
        {
            for (int i = 0; i < _frameworkManagers.Count; i++)
            {
                var mgr = this._frameworkManagers[i];
                if (mgr.needFixedUpdate && mgr.manager.managerState == ManagerState.Started)
                    mgr.manager.OnFixedUpdate(fixedDeltaTime);
            }
            for (int i = 0; i < _moduleManagers.Count; i++)
            {
                var mgr = this._moduleManagers[i];
                if (mgr.needFixedUpdate && mgr.manager.managerState == ManagerState.Started)
                    mgr.manager.OnFixedUpdate(fixedDeltaTime);
            }
            for (int i = 0; i < _tempModuleManagers.Count; i++)
            {
                var mgr = this._tempModuleManagers[i];
                if (mgr.needFixedUpdate && mgr.manager.managerState == ManagerState.Started)
                    mgr.manager.OnFixedUpdate(fixedDeltaTime);
            }
        }

        #endregion

        #region Register

        private void RegisterFrameWork(ManagerInfo[][] frameworks)
        {
            for (int layer = 0; layer < frameworks.Length; ++layer)
            {
                ManagerInfo[] infos = frameworks[layer];
                int len = infos.Length;
                
                for (int index = 0;  index< len; index++)
                {
                    ManagerInfo info = infos[index];
                    if (this._managerName2ManagerDic.ContainsKey(info.ManagerName))
                    {
                        Debug.LogWarningFormat("有多余的框架Manager {0} 被加载，请检查ManagerRegister", info.ManagerName);
                    }
                    else
                    {
                        BasicManager frameManager = info.managerCtor();
                        this._managerName2ManagerDic.Add(info.ManagerName, frameManager);
                        this._frameworkManagers.Add(new ManagerStruct
                        {
                            manager = frameManager,
                            needUpdate = info.needUpdate,
                            needFixedUpdate = info.needFixedUpdate,
                            needLateUpdate = info.needLateUpdate,
                        });
                    }
                }
            }
        }

        private void RegisterModule(ManagerInfo[] modules)
        {
            for (int i = 0; i < modules.Length; i++)
            {
                ManagerInfo info = modules[i];
                if (this._managerName2ManagerDic.ContainsKey(info.ManagerName))
                {
                    Debug.LogWarningFormat("有多余的框架Manager {0} 被加载，请检查ManagerRegister", info.ManagerName);
                }
                else
                {
                    BasicManager moduleManager = info.managerCtor();
                    this._managerName2ManagerDic.Add(info.ManagerName, moduleManager);
                    this._moduleManagers.Add(new ManagerStruct
                    {
                        manager = moduleManager,
                        needUpdate = info.needUpdate,
                        needFixedUpdate = info.needFixedUpdate,
                        needLateUpdate = info.needLateUpdate,
                    });
                }
            }
        }

        private void RegisterTempModule(ManagerInfo[] modules)
        {
            for (int i = 0; i < modules.Length; i++)
            {
                ManagerInfo info = modules[i];
                if (this._managerName2CtorDic.ContainsKey(info.ManagerName))
                {
                    Debug.LogWarningFormat("有多余的框架Manager {0} 被加载，请检查ManagerRegister", info.ManagerName);
                }
                else
                {
                    this._managerName2CtorDic.Add(info.ManagerName, new ManagerCtorStruct
                    {
                        ctor = info.managerCtor,
                        needUpdate = info.needUpdate,
                        needFixedUpdate = info.needFixedUpdate,
                        needLateUpdate = info.needLateUpdate,
                    });
                }
            }
        }

        #endregion

        #region public methods
        /// <summary>
        /// 用于获得特定名字的manager，manager需要在ManagerRegister里面注册。
        /// 注册在TempModuleManagers中的Manager需要先确保调用CreateManager后才能够使用
        /// </summary>
        /// <param name="managerName">被加载的Manager名称</param>
        /// <returns></returns>
        public static BasicManager GetManager(string managerName)
        {
            BasicManager mgr;
            if (_instance._managerName2ManagerDic.TryGetValue(managerName, out mgr))
            {
                return mgr;
            } 
            if (_instance._managerName2CtorDic.ContainsKey(managerName))
            {
                #if UNITY_EDITOR
                Debug.LogWarningFormat("Manager: {0} 尚未初始化，请先调用GameManager.CreateManager来初始化", managerName);
                #endif
            }
            else
            {
                Debug.LogErrorFormat("Manager: {0} 不在注册表", managerName);
            }
            return null;

        }
        
        /// <summary>
        /// 用于获得特定名字的manager，manager需要在ManagerRegister里面注册。
        /// 注册在TempModuleManagers中的Manager需要先确保调用CreateManager后才能够使用
        /// </summary>
        /// <returns></returns>
        public static BasicManager GetManager<T>() where T: BasicManager
        {
            return GetManager(typeof(T).Name);
        }
        
        /// <summary>
        /// 创建特定名字的manager，manager需要在ManagerRegister里面注册。
        /// </summary>
        /// <param name="managerName">创建的Manager的名字</param>
        /// <param name="awakeImmediately">为true,这个manager会立即执行awake。为false,这个manager会在下一个update中执行awake</param>
        public static void CreateManager(string managerName, bool awakeImmediately = false)
        {
            if (_instance._managerName2ManagerDic.ContainsKey(managerName))
                return;
            if (awakeImmediately)
            {
                if (_instance._managerName2CtorDic.ContainsKey(managerName))
                {
                    _instance._newTempModules.Add(managerName);
                }
                else
                {
                    Debug.LogErrorFormat("Manager {0} 未注册", managerName);
                }
            }
            else
            {
                ManagerCtorStruct ctorStruct;
                if (_instance._managerName2CtorDic.TryGetValue(managerName, out ctorStruct))
                {
                    var mgr = ctorStruct.ctor();
                    _instance._tempModuleManagers.Add(new ManagerStruct
                    {
                        manager = mgr,
                        needUpdate = ctorStruct.needUpdate,
                        needFixedUpdate = ctorStruct.needFixedUpdate,
                        needLateUpdate = ctorStruct.needLateUpdate,
                    });
                    mgr.OnInitialize();
                }
                else
                {
                    Debug.LogErrorFormat("Manager {0} 未注册", managerName);
                }
            }
        }

        /// <summary>
        /// 创建特定类型的manager，manager需要在ManagerRegister里面注册。
        /// </summary>
        /// <param name="awakeImmediately">为true,这个manager会立即执行awake。为false,这个manager会在下一个update中执行awake</param>
        public static void CreateManager<T>(bool awakeImmediately = false) where T : BasicManager
        {
            CreateManager(typeof(T).Name, awakeImmediately);
        }

        /// <summary>
        /// 请求卸载特定的Manager，只有属于TempModule的Manager可以被卸载
        /// </summary>
        /// <param name="managerName">被卸载的Manager名</param>
        public static void DestroyManager(string managerName)
        {
            if (!_instance._managerName2CtorDic.ContainsKey(managerName))
            {
                Debug.LogErrorFormat("Manager {0} 未注册或者不是TempModule的Manager");
            }
            else
            {
                BasicManager mgr;
                if (_instance._managerName2ManagerDic.TryGetValue(managerName, out mgr))
                {
                    mgr.Destroy();
                }
            }
        }
        
        /// <summary>
        /// 请求卸载特定的Manager，只有属于TempModule的Manager可以被卸载
        /// </summary>
        public static void DestroyManager<T>() where T: BasicManager
        {
            DestroyManager(typeof(T).Name);
        }

        #endregion
        
    }
}
