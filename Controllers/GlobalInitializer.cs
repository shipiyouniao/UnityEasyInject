using EasyInject.Utils;
using UnityEngine;

namespace EasyInject.Controllers
{
    /// <summary>
    /// author: spyn
    /// description: 全局初始化器
    /// </summary>
    /// <remarks>
    /// 这里必须要设置为最高优先级，因为要在场景加载前初始化，不然就会调用其他的Awake方法
    /// </remarks>
    [DefaultExecutionOrder(-1000000)] 
    public class GlobalInitializer : MonoBehaviour
    {
        // 实例化一个IoC容器，存入静态变量中，这样就可以导致整个游戏都只有一个IoC容器
        public static readonly IIoC Instance = new MyIoC();
        
        private void Awake()
        {
            // 每次进入场景都初始化IoC容器
            Instance.Init();
        }
    }
}