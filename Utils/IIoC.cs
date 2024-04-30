using UnityEngine;

namespace EasyInject.Utils
{
    public interface IIoC
    {
        /// <summary>
        /// 创建一个GameObject作为Bean
        /// </summary>
        /// <param name="original">原型</param>
        /// <param name="beanName">名字</param>
        /// <typeparam name="T">Bean类型</typeparam>
        /// <returns>Bean实例</returns>
        T CreateGameObjectAsBean<T>(GameObject original, string beanName) where T : MonoBehaviour;

        /// <summary>
        /// 创建一个GameObject作为Bean
        /// </summary>
        /// <param name="original">原型</param>
        /// <param name="beanName">名字</param>
        /// <param name="parent">父物体</param>
        /// <typeparam name="T">Bean类型</typeparam>
        /// <returns>Bean实例</returns>
        T CreateGameObjectAsBean<T>(GameObject original, string beanName, Transform parent) where T : MonoBehaviour;

        /// <summary>
        /// 创建一个GameObject作为Bean
        /// </summary>
        /// <param name="original">原型</param>
        /// <param name="beanName">名字</param>
        /// <param name="parent">父物体</param>
        /// <param name="instantiateInWorldSpace">是否在世界空间中实例化</param>
        /// <typeparam name="T">Bean类型</typeparam>
        /// <returns>Bean实例</returns>
        T CreateGameObjectAsBean<T>(GameObject original, string beanName, Transform parent,
            bool instantiateInWorldSpace) where T : MonoBehaviour;

        /// <summary>
        /// 创建一个GameObject作为Bean
        /// </summary>
        /// <param name="original">原型</param>
        /// <param name="beanName">名字</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <typeparam name="T">Bean类型</typeparam>
        /// <returns>Bean实例</returns>
        T CreateGameObjectAsBean<T>(GameObject original, string beanName, Vector3 position, Quaternion rotation)
            where T : MonoBehaviour;

        /// <summary>
        /// 创建一个GameObject作为Bean
        /// </summary>
        /// <param name="original">原型</param>
        /// <param name="beanName">名字</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="parent">父物体</param>
        /// <typeparam name="T">Bean类型</typeparam>
        /// <returns>Bean实例</returns>
        T CreateGameObjectAsBean<T>(GameObject original, string beanName, Vector3 position, Quaternion rotation,
            Transform parent) where T : MonoBehaviour;

        /// <summary>
        /// 删除一个游戏物体Bean
        /// </summary>
        /// <param name="bean">Bean实例</param>
        /// <param name="beanName">Bean名字</param>
        /// <param name="deleteGameObj">是否删除游戏物体</param>
        /// <param name="t">延迟时间</param>
        /// <typeparam name="T">Bean类型</typeparam>
        /// <returns>是否删除成功</returns>
        bool DeleteGameObjBean<T>(T bean, string beanName = "", bool deleteGameObj = false, float t = 0.0F)
            where T : MonoBehaviour;
        
        /// <summary>
        /// 立即删除一个游戏物体Bean
        /// </summary>
        /// <param name="bean">Bean实例</param>
        /// <param name="beanName">Bean名字</param>
        /// <param name="deleteGameObj">是否删除游戏物体</param>
        /// <typeparam name="T">Bean类型</typeparam>
        /// <returns>是否删除成功</returns>
        bool DeleteGameObjBeanImmediate<T>(T bean, string beanName = "", bool deleteGameObj = false)
            where T : MonoBehaviour;

        /// <summary>
        /// 获取一个Bean
        /// </summary>
        /// <param name="name">Bean的名字</param>
        /// <typeparam name="T">Bean的类型</typeparam>
        /// <returns>Bean实例</returns>
        T GetBean<T>(string name = "") where T : class;

        /// <summary>
        /// 针对场景初始化容器
        /// </summary>
        void Init();
        
        /// <summary>
        /// 清空该场景的Bean
        /// </summary>
        /// <param name="scene">场景名称</param>
        /// <param name="clearAcrossScenesBeans">是否清空跨场景的Bean</param>
        void ClearBeans(string scene = null, bool clearAcrossScenesBeans = false);
        
        /// <summary>
        /// 清空该场景的Bean
        /// </summary>
        /// <param name="clearAcrossScenesBeans">是否清空跨场景的Bean</param>
        void ClearBeans(bool clearAcrossScenesBeans);
    }
}