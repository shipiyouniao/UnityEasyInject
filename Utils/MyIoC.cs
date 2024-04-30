using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EasyInject.Attributes;
using EasyInject.Behaviours;
using EasyInject.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyInject.Utils
{
    /// <summary>
    /// author: spyn
    /// description: IoC容器
    /// </summary>
    public class MyIoC : IIoC
    {
        // IoC容器
        private readonly Dictionary<BeanInfo, object> _beans = new();

        // 尚未注入字段的实例
        private readonly Dictionary<ShelvedInstance, HashSet<string>> _shelvedInstances = new();
        
        private string _scene;

        public MyIoC()
        {
            // 注册自己
            RegisterTypeAndParentsAndInterfaces(string.Empty, this, GetType());

            // 注册非游戏物体Bean（一般会持续保留到程序结束）
            InitNormalBean();
        }

        /// <summary>
        /// 创建一个GameObject作为Bean
        /// </summary>
        /// <param name="original">原型</param>
        /// <param name="beanName">名字</param>
        /// <typeparam name="T">Bean类型</typeparam>
        /// <returns>Bean实例</returns>
        public T CreateGameObjectAsBean<T>(GameObject original, string beanName) where T : MonoBehaviour
        {
            var go = Object.Instantiate(original);
            return CheckNewGameObj<T>(go, beanName);
        }

        /// <summary>
        /// 创建一个GameObject作为Bean
        /// </summary>
        /// <param name="original">原型</param>
        /// <param name="beanName">名字</param>
        /// <param name="parent">父物体</param>
        /// <typeparam name="T">Bean类型</typeparam>
        /// <returns>Bean实例</returns>
        public T CreateGameObjectAsBean<T>(GameObject original, string beanName, Transform parent)
            where T : MonoBehaviour
        {
            var go = Object.Instantiate(original, parent);
            return CheckNewGameObj<T>(go, beanName);
        }

        /// <summary>
        /// 创建一个GameObject作为Bean
        /// </summary>
        /// <param name="original">原型</param>
        /// <param name="beanName">名字</param>
        /// <param name="parent">父物体</param>
        /// <param name="instantiateInWorldSpace">是否在世界空间中实例化</param>
        /// <typeparam name="T">Bean类型</typeparam>
        /// <returns>Bean实例</returns>
        public T CreateGameObjectAsBean<T>(GameObject original, string beanName, Transform parent,
            bool instantiateInWorldSpace) where T : MonoBehaviour
        {
            var go = Object.Instantiate(original, parent, instantiateInWorldSpace);
            return CheckNewGameObj<T>(go, beanName);
        }

        /// <summary>
        /// 创建一个GameObject作为Bean
        /// </summary>
        /// <param name="original">原型</param>
        /// <param name="beanName">名字</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <typeparam name="T">Bean类型</typeparam>
        /// <returns>Bean实例</returns>
        public T CreateGameObjectAsBean<T>(GameObject original, string beanName, Vector3 position, Quaternion rotation)
            where T : MonoBehaviour
        {
            var go = Object.Instantiate(original, position, rotation);
            return CheckNewGameObj<T>(go, beanName);
        }

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
        public T CreateGameObjectAsBean<T>(GameObject original, string beanName, Vector3 position, Quaternion rotation,
            Transform parent) where T : MonoBehaviour
        {
            var go = Object.Instantiate(original, position, rotation, parent);
            return CheckNewGameObj<T>(go, beanName);
        }

        /// <summary>
        /// 检查新物体是否符合创建Bean的条件
        /// </summary>
        /// <param name="go">游戏物体</param>
        /// <param name="beanName">Bean名字</param>
        /// <typeparam name="T">Bean类型</typeparam>
        /// <returns>Bean实例</returns>
        /// <exception cref="Exception">如果没有找到对应的组件</exception>
        private T CheckNewGameObj<T>(GameObject go, string beanName) where T : MonoBehaviour
        {
            // 获得当前场景的名字
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // 如果T是BeanObject或AcrossScenesBeanObject且没有挂载，就挂载BeanObject或AcrossScenesBeanObject，否则报错
            var behaviour = go.GetComponent<T>();
            if (behaviour == null)
            {
                if (typeof(T) == typeof(AcrossScenesBeanObject))
                {
                    behaviour = go.AddComponent<AcrossScenesBeanObject>() as T;
                }
                else if (typeof(T) == typeof(BeanObject))
                {
                    behaviour = go.AddComponent<BeanObject>() as T;
                }
                else
                {
                    throw new Exception($"Did not find the corresponding component as {typeof(T)}");
                }
            }

            // 如果在IoC容器中已经有这个Bean，就拋出异常
            if (_beans.ContainsKey(new BeanInfo(beanName, typeof(T))))
            {
                throw new Exception($"Bean {beanName} already exists");
            }

            AddBean(beanName, behaviour, scene);

            return behaviour;
        }

        /// <summary>
        /// 删除一个游戏物体Bean
        /// </summary>
        /// <param name="bean">Bean实例</param>
        /// <param name="beanName">Bean名字</param>
        /// <param name="deleteGameObj">是否删除游戏物体</param>
        /// <param name="t">延迟时间</param>
        /// <typeparam name="T">Bean类型</typeparam>
        /// <returns>是否删除成功</returns>
        public bool DeleteGameObjBean<T>(T bean, string beanName = "", bool deleteGameObj = false, float t = 0.0F)
            where T : MonoBehaviour
        {
            if (bean == null) return false;
            var beanInfo = new BeanInfo(beanName, bean.GetType());
            if (!_beans.ContainsKey(beanInfo)) return false;
            _beans.Remove(beanInfo);

            if (!deleteGameObj)
            {
                // 如果不需要删除游戏物体，就只删除Bean
                Object.Destroy(bean, t);
            }
            else
            {
                // 查找游戏物体上有没有其他的Bean组件（包括打了GameObjectBean特性的组件，以及继承了BeanObject的组件）
                var otherBeans = bean.GetComponents<MonoBehaviour>().Where(monoBehaviour =>
                    monoBehaviour.GetType().GetCustomAttribute<GameObjectBeanAttribute>() != null ||
                    monoBehaviour.GetType().IsSubclassOf(typeof(BeanObject))).ToList();
                foreach (var info in otherBeans.Select(beans => new BeanInfo(beans.name, beans.GetType()))
                             .Where(info => _beans.ContainsKey(info)))
                {
                    _beans.Remove(info);
                }

                Object.Destroy(bean.gameObject, t);
            }

            return true;
        }

        /// <summary>
        /// 立即删除一个游戏物体Bean
        /// </summary>
        /// <param name="bean">Bean实例</param>
        /// <param name="beanName">Bean名字</param>
        /// <param name="deleteGameObj">是否删除游戏物体</param>
        /// <typeparam name="T">Bean类型</typeparam>
        /// <returns>是否删除成功</returns>
        public bool DeleteGameObjBeanImmediate<T>(T bean, string beanName = "", bool deleteGameObj = false)
            where T : MonoBehaviour
        {
            if (bean == null) return false;
            var beanInfo = new BeanInfo(beanName, bean.GetType());
            if (!_beans.ContainsKey(beanInfo)) return false;
            _beans.Remove(beanInfo);

            if (!deleteGameObj)
            {
                // 如果不需要删除游戏物体，就只删除Bean
                Object.DestroyImmediate(bean);
            }
            else
            {
                // 查找游戏物体上有没有其他的Bean组件（包括打了GameObjectBean特性的组件，以及继承了BeanObject的组件）
                var otherBeans = bean.GetComponents<MonoBehaviour>().Where(monoBehaviour =>
                    monoBehaviour.GetType().GetCustomAttribute<GameObjectBeanAttribute>() != null ||
                    monoBehaviour.GetType().IsSubclassOf(typeof(BeanObject))).ToList();
                foreach (var info in otherBeans.Select(beans => new BeanInfo(beans.name, beans.GetType()))
                             .Where(info => _beans.ContainsKey(info)))
                {
                    _beans.Remove(info);
                }

                Object.DestroyImmediate(bean.gameObject);
            }

            return true;
        }

        /// <summary>
        /// 获取一个Bean
        /// </summary>
        /// <param name="name">Bean的名字</param>
        /// <typeparam name="T">Bean的类型</typeparam>
        /// <returns>Bean实例</returns>
        public T GetBean<T>(string name = "") where T : class
        {
            var beanInfo = new BeanInfo(name, typeof(T));
            if (_beans.TryGetValue(beanInfo, out var value))
            {
                return (T) value;
            }

            return null;
        }
        
        /// <summary>
        /// 针对场景初始化容器
        /// </summary>
        public void Init()
        {
            // 清空上一个场景的Bean
            if(_scene != null) ClearBeans(_scene);

            // 获取场景中需要注入的MonoBehaviour实例
            InitGameObjectBean();
        }

        /// <summary>
        /// 清空该场景的Bean
        /// </summary>
        /// <param name="scene">场景名称</param>
        /// <param name="clearAcrossScenesBeans">是否清空跨场景的Bean</param>
        public void ClearBeans(string scene = null, bool clearAcrossScenesBeans = false)
        {
            scene ??= UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            foreach (var (beanInfo, value) in _beans.Where(bean => bean.Key.Scenes.Contains(scene)).ToList())
            {
                // 如果要清空跨场景的Bean，要删除跨场景Bean的GameObject
                if (value.GetType().GetCustomAttribute<PersistAcrossScenesAttribute>() != null)
                {
                    if (!clearAcrossScenesBeans)
                        continue;
                    
                    DeleteGameObjBeanImmediate(value as MonoBehaviour, beanInfo.Name, true);
                }
                else
                {
                    _beans.Remove(beanInfo);
                }
            }

            _shelvedInstances.Clear();
        }

        /// <summary>
        /// 清空该场景的Bean
        /// </summary>
        /// <param name="clearAcrossScenesBeans">是否清空跨场景的Bean</param>
        public void ClearBeans(bool clearAcrossScenesBeans)
        {
            ClearBeans(null, clearAcrossScenesBeans);
        }

        /// <summary>
        /// 获取场景中需要注入的MonoBehaviour实例
        /// </summary>
        private void InitGameObjectBean()
        {
            // 获得当前场景的名字
            _scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // 获得场景上所有挂载了BeanObject的物体
            var beanObjects = Resources.FindObjectsOfTypeAll<BeanObject>()
                // 非运行时环境下，如果预制体也在场景中，会把场景中物件和预制体都找到导致重复，因此必须筛选出来
#if UNITY_EDITOR
                .Where(beanObject => !UnityEditor.PrefabUtility.IsPartOfPrefabAsset(beanObject) &&
                                     !UnityEditor.PrefabUtility.IsPartOfPrefabInstance(beanObject))
#endif
                .ToList();

            foreach (var beanObject in beanObjects)
            {
                if (_beans.ContainsKey(new BeanInfo(beanObject.name, typeof(BeanObject))))
                {
                    // 检查是不是PersistAcrossScenesAttribute，如果是就往场景列表中添加场景
                    if (beanObject.GetType().GetCustomAttribute<PersistAcrossScenesAttribute>() != null)
                    {
                        var keys = _beans.Where(bean => bean.Key.Name == beanObject.name && bean.Value.Equals(beanObject))
                            .Select(bean => bean.Key).ToList();
                        foreach (var key in keys.Where(key => !key.Scenes.Contains(_scene)))
                        {
                            key.Scenes.Add(_scene);
                        }
                    }
                    
                    continue;
                }

                // 不需要进行字段依赖注入，因为BeanObject本来就是空的
                AddBean(beanObject.name, beanObject, _scene, false);
            }

            // 获得场景上所有挂载了GameObjectBeanAttribute的物体
            var gameObjectBeans = Resources.FindObjectsOfTypeAll<MonoBehaviour>().Where(monoBehaviour =>
                    monoBehaviour.GetType().GetCustomAttribute<GameObjectBeanAttribute>() != null)
                // 非运行时环境下，如果预制体也在场景中，会把场景中物件和预制体都找到导致重复，因此必须筛选出来
#if UNITY_EDITOR
                .Where(monoBehaviour => !UnityEditor.PrefabUtility.IsPartOfPrefabAsset(monoBehaviour) &&
                                        !UnityEditor.PrefabUtility.IsPartOfPrefabInstance(monoBehaviour))
#endif
                .ToList();
            foreach (var gameObjectBean in gameObjectBeans)
            {
                var attribute = gameObjectBean.GetType().GetCustomAttribute<GameObjectBeanAttribute>();

                // 先获取NameType，再根据NameType获取名字
                var name = attribute.NameType switch
                {
                    ENameType.Custom => attribute.Name,
                    ENameType.ClassName => gameObjectBean.GetType().Name,
                    ENameType.GameObjectName => gameObjectBean.name,
                    _ => string.Empty
                };

                if (_beans.ContainsKey(new BeanInfo(name, gameObjectBean.GetType())))
                {
                    // 检查是不是PersistAcrossScenesAttribute，如果是就往场景列表中添加场景
                    if (gameObjectBean.GetType().GetCustomAttribute<PersistAcrossScenesAttribute>() != null)
                    {
                        var keys = _beans.Where(bean => bean.Key.Name == name && bean.Value.Equals(gameObjectBean))
                            .Select(bean => bean.Key).ToList();
                        foreach (var key in keys.Where(key => !key.Scenes.Contains(_scene)))
                        {
                            key.Scenes.Add(_scene);
                        }
                    }
                    
                    continue;
                }

                AddBean(name, gameObjectBean, _scene);
            }
        }

        /// <summary>
        /// 注入非MonoBehavior实例
        /// </summary>
        private void InitNormalBean()
        {
            // 扫描所有程序集中打了Component特性的类
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttributes(typeof(ComponentAttribute), true).Length > 0).ToList();

            // 先进行实例化（包括构造函数的依赖注入）
            while (types.Count > 0)
            {
                for (var i = 0; i < types.Count; i++)
                {
                    var type = types[i];
                    // 获取构造函数
                    var constructors = type.GetConstructors();
                    // 实例
                    object instance = null;

                    // 遍历构造函数，找到可以实例化的构造函数
                    foreach (var constructor in constructors)
                    {
                        // 获取构造函数的参数
                        var parameters = constructor.GetParameters();
                        // 构造函数的参数实例
                        var parameterInstances = new object[parameters.Length];

                        for (var j = 0; j < parameters.Length; j++)
                        {
                            // 获取参数的类型
                            var parameterType = parameters[j].ParameterType;
                            // 获取参数上Autowired特性，也可以不打，视为Name为Empty
                            var name = parameters[j].GetCustomAttribute<AutowiredAttribute>()?.Name;
                            var beanInfo = new BeanInfo(name, parameterType);

                            // 如果IoC容器中有这个参数的实例，就注入
                            if (_beans.TryGetValue(beanInfo, out var parameterInstance))
                            {
                                parameterInstances[j] = parameterInstance;
                            }
                            else
                            {
                                break;
                            }
                        }

                        // 如果有参数没有实例化，就跳过这个构造函数
                        if (parameterInstances.Contains(null)) continue;
                        instance = constructor.Invoke(parameterInstances);
                        break;
                    }

                    // 如果没有找到可以实例化的构造函数，就找无参构造函数
                    if (instance == null && type.GetConstructor(Type.EmptyTypes) != null)
                    {
                        instance = Activator.CreateInstance(type);
                    }

                    if (instance == null) continue;
                    RegisterTypeAndParentsAndInterfaces(
                        type.GetCustomAttribute<ComponentAttribute>()?.Name ?? string.Empty, instance, type);

                    // 从待注册列表中移除
                    types.RemoveAt(i);
                    i--;
                }
            }

            // 开始进行字段和属性的依赖注入
            foreach (var type in _beans.Keys.ToList())
            {
                var instance = _beans[type];

                // 如果有PersistAcrossScenesAttribute特性，跳过，因为这个是持久化的游戏组件对象
                if (instance.GetType().GetCustomAttribute<PersistAcrossScenesAttribute>() != null)
                {
                    continue;
                }

                // 获得打了Autowired特性的字段
                var fields = instance.GetType()
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => f.GetCustomAttributes(typeof(AutowiredAttribute), true).Length > 0).ToList();

                var injected = new HashSet<string>();

                foreach (var field in fields)
                {
                    // 如果Autowired传了名字参数，名字和类型都要匹配
                    var name = field.GetCustomAttribute<AutowiredAttribute>()?.Name;
                    var beanInfo = new BeanInfo(name, field.FieldType);
                    // 如果IoC容器中有这个类型的实例，就注入
                    if (_beans.TryGetValue(beanInfo, out var value))
                    {
                        field.SetValue(instance, value);
                        injected.Add(field.Name);
                    }
                    else
                    {
                        _shelvedInstances.Add(new ShelvedInstance(type.Name, instance), injected);
                    }
                }

                // 获得打了Autowired特性的属性
                var properties = instance.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => p.GetCustomAttributes(typeof(AutowiredAttribute), true).Length > 0).ToList();

                foreach (var property in properties)
                {
                    // 如果Autowired传了名字参数，名字和类型都要匹配
                    var name = property.GetCustomAttribute<AutowiredAttribute>()?.Name;
                    var beanInfo = new BeanInfo(name, property.PropertyType);
                    // 如果IoC容器中有这个类型的实例，就注入
                    if (_beans.TryGetValue(beanInfo, out var value))
                    {
                        property.SetValue(instance, value);
                        injected.Add(property.Name);
                    }
                    else
                    {
                        _shelvedInstances.Add(new ShelvedInstance(type.Name, instance), injected);
                    }
                }
            }
        }

        /// <summary>
        /// 这个方法一般用于传入MonoBehaviour实例，然后进行字段的依赖注入
        /// </summary>
        /// <remarks>
        /// 要注意如果暂时无法完成注入，会被搁置，如果一直被搁置证明没有生成对应的Bean，在外部调用时肯定会报错
        /// </remarks>
        /// <param name="beanName">Bean的名字</param>
        /// <param name="instance">MonoBehaviour实例</param>
        private void Inject(string beanName, object instance)
        {
            var type = instance.GetType();

            // 字段注入
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.GetCustomAttributes(typeof(AutowiredAttribute), true).Length > 0);

            var injected = new HashSet<string>();

            foreach (var field in fields)
            {
                // 如果Autowired传了名字参数，名字和类型都要匹配
                var name = field.GetCustomAttribute<AutowiredAttribute>()?.Name;
                var beanInfo = new BeanInfo(name, field.FieldType);

                // 如果IoC容器中有这个类型的实例，就注入
                if (_beans.TryGetValue(beanInfo, out var value))
                {
                    field.SetValue(instance, value);
                    injected.Add(field.Name);
                }
                else
                {
                    var insKey = new ShelvedInstance(beanName, instance);
                    // 暂时放入_shelvedInstances中，等到有实例的时候再注入
                    _shelvedInstances.Add(insKey, injected);
                    break;
                }
            }

            // 属性注入
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.GetCustomAttributes(typeof(AutowiredAttribute), true).Length > 0);

            foreach (var property in properties)
            {
                // 如果Autowired传了名字参数，名字和类型都要匹配
                var name = property.GetCustomAttribute<AutowiredAttribute>()?.Name;
                var beanInfo = new BeanInfo(name, property.PropertyType);

                // 如果IoC容器中有这个类型的实例，就注入
                if (_beans.TryGetValue(beanInfo, out var value))
                {
                    property.SetValue(instance, value);
                    injected.Add(property.Name);
                }
                else
                {
                    var insKey = new ShelvedInstance(beanName, instance);
                    // 暂时放入_shelvedInstances中，等到有实例的时候再注入
                    _shelvedInstances.Add(insKey, injected);
                    break;
                }
            }

            CheckShelvedInstances();
        }

        /// <summary>
        /// 新增一个Bean（一般是MonoBehaviour）
        /// </summary>
        /// <remarks>
        /// 要注意如果暂时无法完成注入，会被搁置，如果一直被搁置证明没有生成对应的Bean，在外部调用时肯定会报错
        /// </remarks>
        /// <param name="name">Bean的名字</param>
        /// <param name="instance">Bean的实例</param>
        /// <param name="scene">场景名称</param>
        /// <param name="startInject">是否立即注入</param>
        private void AddBean<T>(string name, T instance, string scene, bool startInject = true)
            where T : MonoBehaviour
        {
            RegisterTypeAndParentsAndInterfaces(name, instance, instance.GetType(), scene);

            if (startInject)
            {
                Inject(name, instance);
            }
        }

        /// <summary>
        /// 检查_shelvedInstances中的实例是否可以注入
        /// </summary>
        private void CheckShelvedInstances()
        {
            foreach (var (key, injected) in _shelvedInstances.ToList())
            {
                var insFields = key.Instance.GetType()
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => f.GetCustomAttributes(typeof(AutowiredAttribute), true).Length > 0).ToList();

                var insProperties = key.Instance.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => p.GetCustomAttributes(typeof(AutowiredAttribute), true).Length > 0).ToList();

                var count = insFields.Count + insProperties.Count;

                foreach (var field in insFields)
                {
                    // 如果已经注入过了，就跳过
                    if (injected.Contains(field.Name))
                    {
                        count--;
                        continue;
                    }

                    // 如果Autowired传了名字参数，名字和类型都要匹配
                    var name = field.GetCustomAttribute<AutowiredAttribute>().Name;
                    var beanInfo = new BeanInfo(name, field.FieldType);
                    // 如果IoC容器中有这个类型的实例，就注入
                    if (!_beans.TryGetValue(beanInfo, out var value))
                    {
                        continue;
                    }

                    field.SetValue(key.Instance, value);
                    injected.Add(field.Name);
                    count--;
                }

                foreach (var property in insProperties)
                {
                    // 如果已经注入过了，就跳过
                    if (injected.Contains(property.Name))
                    {
                        count--;
                        continue;
                    }

                    // 如果Autowired传了名字参数，名字和类型都要匹配
                    var name = property.GetCustomAttribute<AutowiredAttribute>().Name;
                    var beanInfo = new BeanInfo(name, property.PropertyType);
                    // 如果IoC容器中有这个类型的实例，就注入
                    if (!_beans.TryGetValue(beanInfo, out var value))
                    {
                        continue;
                    }

                    property.SetValue(key.Instance, value);
                    injected.Add(property.Name);
                    count--;
                }

                if (count == 0)
                {
                    _shelvedInstances.Remove(key);
                }
            }
        }

        /// <summary>
        /// 按照继承链注册自己、父类和接口
        /// </summary>
        /// <param name="name">bean的名称</param>
        /// <param name="instance">bean的实例</param>
        /// <param name="type">bean的类型</param>
        /// <param name="scene">场景名称</param>
        private void RegisterTypeAndParentsAndInterfaces(string name, object instance, Type type, string scene = "")
        {
            name ??= string.Empty;
            // 注册自己
            var beanInfo = new BeanInfo(name, type, scene);

            if (!_beans.TryAdd(beanInfo, instance))
            {
                throw new Exception($"Bean {name} already exists");
            }

            // 注册父类
            var baseType = type.BaseType;
            if (baseType != null && baseType != typeof(object) && baseType.Namespace != null &&
                !baseType.Namespace.Contains("UnityEngine"))
            {
                RegisterTypeAndParentsAndInterfaces(name, instance, baseType, scene);
            }

            // 注册接口
            var interfaces = type.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                if (@interface.Namespace == null || @interface.Namespace.Contains("UnityEngine")) continue;
                RegisterTypeAndParentsAndInterfaces(name, instance, @interface, scene);
            }
        }
    }
}