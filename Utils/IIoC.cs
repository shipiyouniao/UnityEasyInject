using UnityEngine;

namespace EasyInject.Utils
{
    public interface IIoC
    {
        /// <summary>
        /// 创建一个GameObject作为Bean
        /// </summary>
        /// <param name="original">原型</param>
        /// <param name="parent">父物体</param>
        /// <param name="beanName">名字</param>
        /// <typeparam name="T">Bean类型</typeparam>
        /// <returns>Bean实例</returns>
        T CreateGameObjectAsBean<T>(GameObject original, Transform parent, string beanName) where T : MonoBehaviour;

        /// <summary>
        /// 获取一个Bean
        /// </summary>
        /// <param name="name">Bean的名字</param>
        /// <typeparam name="T">Bean的类型</typeparam>
        /// <returns>Bean实例</returns>
        T GetBean<T>(string name = "") where T : class;
        
        /// <summary>
        /// 初始化IoC容器
        /// </summary>
        void Init();
    }
}