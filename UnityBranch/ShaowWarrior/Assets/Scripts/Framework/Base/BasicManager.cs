namespace Framework
{
    
public interface IManager
{
    public void Start();
    public void Destroy();
    public void OnUpdate(float deltaTime);
    public void OnLateUpdate(float deltaTime);
    public void OnFixedUpdate(float fixedDeltaTime);
}

public enum ManagerState : int
{
    /** 初始化Manager，不能保证所有任务都已加载好 */ 
    StartUp = 0,
    /** 在此阶段Manager所有内容已完全加载好，可以直接使用 */
    Ready = 1,
    /** 已经初始化完成 */
    Started = 2,
    /** 正在卸载Manager的相关资源 */
    Unloading = 3,
    /** Manager相关资源已经摧毁，可以直接清除 */
    Destroy = 4,
}

/**
 * 基础的游戏manager，在这里只负责游戏实体之间的逻辑不负责任何表现
 */
public abstract class BasicManager: IManager
{

    private ManagerState _managerState = ManagerState.StartUp;
    public ManagerState managerState => this._managerState;

    #region life cycle

    /** 当一个管理器被创建好后，在Awake对需要的数据进行加载 */
    public void OnInitialize()
    {
        this._managerState = ManagerState.StartUp;
        this.OnAwake();
        if (this._managerState == ManagerState.StartUp) // 为了防止被创建后立即删除无法执行删除逻辑
            this._managerState = ManagerState.Ready;
    }

    /** 此时的Manager已经加载好了必要配置，可以在这里对数据进行预处理 */
    public void Start()
    {
        this.OnStart();
        this._managerState = ManagerState.Started;
    }
    
    /** 当一个管理器被彻底销毁前，需要释放占有的资源 */
    public virtual void Destroy()
    {
        this._managerState = ManagerState.Unloading;
        this.OnDestroy();
        this._managerState = ManagerState.Destroy;
    }

    /** 在Start阶段完成后，Unity每个Update调用 */
    public virtual void OnUpdate(float deltaTime)
    {
    }

    /** 在Start阶段完成后，Unity每个LateUpdate调用 */
    public virtual void OnLateUpdate(float deltaTime)
    {
    }

    /** 在Start阶段完成后，Unity每个FixedUpdate调用 */

    public virtual void OnFixedUpdate(float fixedDeltaTime)
    {
    }

    protected abstract void OnDestroy();
    /** 在这里加载初始化必要数据 */
    protected abstract void OnAwake();
    /** 必要的数据以及依赖模块以加载好，在这里编写预处理逻辑 */
    protected abstract void OnStart();

    #endregion

}

public delegate BasicManager ManagerConstructorDelegate();
/** 注册Manager使用的结构体 */
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

}