
public interface IManager
{
    public void OnAwake();
    public void OnDestroy();
    public void OnUpdate(float deltaTime);
    public void OnLateUpdate(float deltaTime);
    public void OnFixedUpdate(float fixedDeltaTime);
}

/**
 * 基础的游戏manager，在这里只负责游戏实体之间的逻辑不负责任何表现
 */
public abstract class BasicManager: IManager
{

    /** 当一个管理器被创建好后，在Awake对需要的数据进行加载以及预处理*/
    public abstract void OnAwake();
    /** 当一个管理器被彻底销毁前，需要释放占有的资源 */
    public abstract void OnDestroy();

    public virtual void OnUpdate(float deltaTime)
    {
    }

    public virtual void OnLateUpdate(float deltaTime)
    {
    }

    public virtual void OnFixedUpdate(float fixedDeltaTime)
    {
    }
}

public delegate BasicManager ManagerConstructorDelegate();
public struct ManagerInfo
{
    /** Manager名字 */
    public string ManagerName;
    /** Manager的构造函数*/
    public ManagerConstructorDelegate managerCtor;
    /** 设置为true的话会在Update生命周期调用Manager的OnUpdate函数 */
    public bool needUpdate;
    /** 设置为true的话会在LateUpdate生命周期调用Manager的OnLateUpdate函数 */
    public bool needLateUpdate;
    /** 设置为true的话会在FixedUpdate生命周期调用Manager的OnFixedUpdate函数 */
    public bool needFixedUpdate;
}
