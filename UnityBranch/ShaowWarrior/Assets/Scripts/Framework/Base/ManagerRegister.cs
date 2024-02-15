// manager的注册表

using Modules.Input;

namespace Framework
{
    public class ManagerRegister
    {
        /** 底层框架注册表 */
        public ManagerInfo[][]  GetFrameworkManagers()
        {
            ManagerInfo[][] managerInfos =
            {
                // 第一层框架，不依赖任何其他内容
                new ManagerInfo[]{
                    new ManagerInfo{
                        ManagerName = "EventManager",
                        managerCtor = () => new EventManager(),
                        needUpdate = false,
                        needLateUpdate = false,
                        needFixedUpdate = false,
                    },
                    new ManagerInfo{
                        ManagerName = "ResourceManager",
                        managerCtor = () => new ResourceManager(),
                        needUpdate = false,
                        needLateUpdate = false,
                        needFixedUpdate = false,
                    },
                    new ManagerInfo
                    {
                        ManagerName = "ConfigManager",
                        managerCtor = () => new ConfigManager(),
                        needUpdate = false,
                        needLateUpdate = false,
                        needFixedUpdate = false,
                    },
                },
                // 第二层框架，依赖第一层框架中的内容
                new ManagerInfo[]{
                    new ManagerInfo{
                        ManagerName = "GameSceneManager",
                        managerCtor = () => new GameSceneManager(),
                        needUpdate = false,
                        needLateUpdate = false,
                        needFixedUpdate = false,
                    },
                },
            };
            return managerInfos;
        }
    
        /// <summary>
        /// 用于注册非框架层的manager，会在所有的框架层manager注册完成后调用。
        /// 在这里注册的Manager都会在框架加载完后被创建，它们的生命周期贯穿整个游戏
        /// 请注意，如果不同业务层之间存在依赖关系，则需要确保被依赖的业务层的注册顺序在此之前
        /// </summary>
        /// <returns></returns>
        public ManagerInfo[] GetMainModuleManagers()
        {
            ManagerInfo[] managerInfos = new ManagerInfo[]
            {
                new ManagerInfo{
                    ManagerName = "InputManager",
                    managerCtor = () => new InputManager(),
                    needUpdate = true,
                    needLateUpdate = false,
                    needFixedUpdate = false,
                },
            };
            return managerInfos;
        }

        public ManagerInfo[] GetTempModuleManagers()
        {
            ManagerInfo[] managerInfos = new ManagerInfo[]
            {
            };
            return managerInfos;
        }
    }
}
