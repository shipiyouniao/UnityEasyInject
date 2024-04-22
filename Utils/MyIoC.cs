using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EasyInject.Attributes;
using EasyInject.Behaviours;
using EasyInject.Models;
using UnityEngine;

namespace EasyInject.Utils
{
    /// <summary>
    /// author: spyn
    /// description: IoC容器
    /// </summary>
    public class MyIoC
    {
        // IoC容器
        private readonly Dictionary<BeanInfo, object> _beans = new();

        // 尚未注入字段的实例
        private readonly Dictionary<ShelvedInstance, HashSet<string>> _shelvedInstances = new();

        /// <summary>
        /// 创建一个GameObject作为Bean
        /// </summary>
        /// <param name="original">原型</param>
        /// <param name="parent">父物体</param>
        /// <param name="beanName">名字</param>
        /// <typeparam name="T">Bean类型</typeparam>
        /// <returns>Bean实例</returns>
        public T CreateGameObjectAsBean<T>(GameObject original, Transform parent, string beanName)
            where T : MonoBehaviour
        {
            var go = UnityEngine.Object.Instantiate(original, parent);

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

            AddBean(beanName, behaviour);

            return behaviour;
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
        /// 初始化IoC容器
        /// </summary>
        public void Init()
        {
            // 清空IoC容器
            var beansToKeep =
                (from bean in _beans
                    let type = bean.Value.GetType()
                    where type.GetCustomAttribute<PersistAcrossScenesAttribute>() != null
                    select bean).ToDictionary(bean => bean.Key, bean => bean.Value);
            _beans.Clear();
            foreach (var bean in beansToKeep)
            {
                _beans.Add(bean.Key, bean.Value);
            }

            _shelvedInstances.Clear();

            InitNormalBean();
            GetDefaultMonoBehaviourInfo();
        }

        /// <summary>
        /// 获取场景中需要注入的MonoBehaviour实例数
        /// </summary>
        private void GetDefaultMonoBehaviourInfo()
        {
            // 获得场景上所有挂载了BeanObject的物体
            var beanObjects = Resources.FindObjectsOfTypeAll<BeanObject>().ToList();
            foreach (var beanObject in beanObjects.Where(beanObject =>
                         !_beans.ContainsKey(new BeanInfo(beanObject.name, typeof(BeanObject)))))
            {
                // 不需要进行字段依赖注入，因为BeanObject本来就是空的
                AddBean(beanObject.name, beanObject, false);
            }

            // 获得场景上所有挂载了GameObjectBeanAttribute的物体
            var gameObjectBeans = Resources.FindObjectsOfTypeAll<MonoBehaviour>().Where(monoBehaviour =>
                monoBehaviour.GetType().GetCustomAttribute<GameObjectBeanAttribute>() != null).ToList();
            foreach (var gameObjectBean in gameObjectBeans)
            {
                var name = gameObjectBean.GetType().GetCustomAttribute<GameObjectBeanAttribute>().Name;
                // 如果已经存在，就不再添加（一般是因为这是持久化的对象）
                if (_beans.ContainsKey(new BeanInfo(name, gameObjectBean.GetType()))) continue;
                AddBean(name, gameObjectBean);
            }
        }

        /// <summary>
        /// 注入非MonoBehavior实例
        /// </summary>
        /// <exception cref="Exception">没有找到对应的实例</exception>
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

            // 开始进行字段的依赖注入
            foreach (var type in _beans.Keys.ToList())
            {
                var instance = _beans[type];

                // 如果有PersistAcrossScenesAttribute特性，跳过，因为这个是持久化的游戏组件对象
                if (instance.GetType().GetCustomAttribute<PersistAcrossScenesAttribute>() != null)
                {
                    continue;
                }

                var fields = instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => f.GetCustomAttributes(typeof(AutowiredAttribute), true).Length > 0).ToList();

                foreach (var field in fields)
                {
                    // 如果Autowired传了名字参数，名字和类型都要匹配
                    var name = field.GetCustomAttribute<AutowiredAttribute>()?.Name;
                    var beanInfo = new BeanInfo(name, field.FieldType);
                    // 如果IoC容器中有这个类型的实例，就注入
                    if (_beans.TryGetValue(beanInfo, out var value))
                    {
                        field.SetValue(instance, value);
                    }
                    else
                    {
                        throw new Exception($"No service of type {field.FieldType} found for autowiring");
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
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.GetCustomAttributes(typeof(AutowiredAttribute), true).Length > 0);

            foreach (var field in fields)
            {
                // 如果Autowired传了名字参数，名字和类型都要匹配
                var name = field.GetCustomAttribute<AutowiredAttribute>()?.Name;
                var beanInfo = new BeanInfo(name, field.FieldType);
                var injectedFields = new HashSet<string>();
                // 如果IoC容器中有这个类型的实例，就注入
                if (_beans.TryGetValue(beanInfo, out var value))
                {
                    field.SetValue(instance, value);
                    injectedFields.Add(field.Name);
                }
                else
                {
                    var insKey = new ShelvedInstance(beanName, instance);
                    // 暂时放入_shelvedInstances中，等到有实例的时候再注入
                    _shelvedInstances.Add(insKey, injectedFields);
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
        /// <param name="startInject">是否立即注入</param>
        private void AddBean<T>(string name, T instance, bool startInject = true)
            where T : MonoBehaviour
        {
            RegisterTypeAndParentsAndInterfaces(name, instance, instance.GetType());

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
            foreach (var (key, injectedFields) in _shelvedInstances.ToList())
            {
                var insFields = key.Instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => f.GetCustomAttributes(typeof(AutowiredAttribute), true).Length > 0).ToList();
                var count = insFields.Count;

                foreach (var field in insFields)
                {
                    // 如果已经注入过了，就跳过
                    if (injectedFields.Contains(field.Name))
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
                    injectedFields.Add(field.Name);
                    count--;
                }

                if (count == 0)
                {
                    _shelvedInstances.Remove(key);
                }
            }
        }

        /// <summary>
        /// 按照继承链注册自己、父类和接口（包括注册到Object）
        /// </summary>
        /// <param name="name">bean的名称</param>
        /// <param name="instance">bean的实例</param>
        /// <param name="type">bean的类型</param>
        private void RegisterTypeAndParentsAndInterfaces(string name, object instance, Type type)
        {
            name ??= string.Empty;
            // 注册自己
            var beanInfo = new BeanInfo(name, type);
            _beans[beanInfo] = instance;

            // 注册父类
            var baseType = type.BaseType;
            if (baseType != null)
            {
                RegisterTypeAndParentsAndInterfaces(name, instance, baseType);
            }

            // 注册接口
            var interfaces = type.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                if (@interface.Namespace == null || @interface.Namespace.Contains("UnityEngine")) continue;
                RegisterTypeAndParentsAndInterfaces(name, instance, @interface);
            }
        }
    }
}