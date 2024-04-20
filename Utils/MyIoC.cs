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

        // 默认需要注入的MonoBehaviour实例
        private readonly List<object> _behaviours = new();

        // 尚未注入字段的实例
        private readonly Dictionary<object, HashSet<string>> _shelvedInstances = new();

        // 某场景下需要一开始就注入的MonoBehaviour实例数
        private int _instancesCount;

        // 用于全部注入完成后的回调
        private event Action OnAwakeCalled;
        
        // 默认需要注入的MonoBehavior是否已经全部注入
        public bool IsInjected { get; private set; }

        /// <summary>
        /// 增加一个待注入的实例和完成后的回调
        /// </summary>
        /// <param name="instance">待注入的实例</param>
        /// <param name="action">完成后的回调</param>
        /// <param name="isBean">是否是Bean</param>
        public void AddDefaultInstance(object instance, Action action, bool isBean)
        {
            _behaviours.Add(instance);
            // 如果是Bean，就放入IoC容器
            if (isBean)
            {
                var type = instance.GetType();
                var beanName = type.GetCustomAttribute<BeanNameAttribute>();
                AddBean(beanName != null ? beanName.Name : string.Empty, instance, false);
            }

            OnAwakeCalled += action;

            if (_behaviours.Count == _instancesCount)
            {
                InjectDefaultInstances();
            }
        }

        /// <summary>
        /// 注入默认的MonoBehaviour实例
        /// </summary>
        private void InjectDefaultInstances()
        {
            for (var i = 0; i < _behaviours.Count; i++)
            {
                var instance = _behaviours[i];
                Inject(instance);
            }
            
            // 检查实例是否注入完成，如果没有，证明默认的不满足，抛出异常
            if (_shelvedInstances.Count > 0)
            {
                // 异常说明显示的是第一个没有注入完成的实例
                var ins = _shelvedInstances.First();
                throw new Exception($"No service of type {ins.Key.GetType()} found for autowiring");
            }
            
            IsInjected = true;
            OnAwakeCalled?.Invoke();
        }

        /// <summary>
        /// 初始化IoC容器
        /// </summary>
        public void Init()
        {
            // 清空IoC容器
            _beans.Clear();
            _shelvedInstances.Clear();
            _behaviours.Clear();
            IsInjected = false;
            _instancesCount = 0;
            OnAwakeCalled = null;
            
            InitNormalBean();
            GetDefaultMonoBehaviourInfo();
        }

        /// <summary>
        /// 获取场景中需要注入的MonoBehaviour实例数
        /// </summary>
        private void GetDefaultMonoBehaviourInfo()
        {
            // 获得场景上所有被隐藏的物体，如果是BeanMonoBehaviour就激活一下，这样才会调用Awake方法
            var hiddenBeans = Resources.FindObjectsOfTypeAll<BeanMonoBehaviour>().Where(bean => !bean.gameObject.activeSelf).ToList();
            foreach (var bean in hiddenBeans)
            {
                bean.gameObject.SetActive(true);
                bean.gameObject.SetActive(false);
            }
            // 获取场景名称，计算出场景中需要注入的MonoBehaviour实例数
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var behaviours = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttributes(typeof(DefaultInjectAttribute), true).Length > 0).ToList();
            foreach (var dummy in behaviours
                         .Select(behaviour => behaviour.GetCustomAttribute<DefaultInjectAttribute>())
                         .Where(attribute => attribute.SceneNameList.Contains(sceneName)))
            {
                _instancesCount++;
            }
            
            // 获得场景上所有挂载了BeanObject且isDefault为true的物体
            var beanObjects = Resources.FindObjectsOfTypeAll<BeanObject>().Where(bean => bean.isDefault).ToList();
            foreach (var beanObject in beanObjects)
            {
                AddBean(beanObject.name, beanObject, false);
                _behaviours.Add(beanObject);
                // 这里也需要把默认实例数加1
                _instancesCount++;
            }
            // 如果为0，把IsInjected设为true
            if (_instancesCount == 0)
            {
                IsInjected = true;
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
                    BeanInfo beanInfo;
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
                            beanInfo = new BeanInfo(name, parameterType);

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
                    // 注册进IoC容器
                    beanInfo = new BeanInfo(type.GetCustomAttribute<ComponentAttribute>().Name, type);
                    _beans[beanInfo] = instance;
                    // 观察这个类是否实现了接口，如果有也要把接口作为key注册进IoC容器
                    var interfaces = type.GetInterfaces();
                    foreach (var @interface in interfaces)
                    {
                        beanInfo = @interface.GetCustomAttribute<ComponentAttribute>()?.Name == null
                            ? new BeanInfo(type.GetCustomAttribute<ComponentAttribute>().Name, @interface)
                            : new BeanInfo(@interface.GetCustomAttribute<ComponentAttribute>().Name, @interface);
                        _beans[beanInfo] = instance;
                    }

                    // 观察这个类是否继承了父类，如果有也要把父类作为key注册进IoC容器（不能是Object）
                    var baseType = type.BaseType;
                    if (baseType != null && baseType != typeof(object))
                    {
                        beanInfo = baseType.GetCustomAttribute<ComponentAttribute>()?.Name == null
                            ? new BeanInfo(type.GetCustomAttribute<ComponentAttribute>().Name, baseType)
                            : new BeanInfo(baseType.GetCustomAttribute<ComponentAttribute>().Name, baseType);
                        _beans[beanInfo] = instance;
                    }

                    // 从待注册列表中移除
                    types.RemoveAt(i);
                    i--;
                }
            }

            // 开始进行字段的依赖注入
            foreach (var type in _beans.Keys.ToList())
            {
                var instance = _beans[type];
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
        /// <param name="instance">MonoBehaviour实例</param>
        /// <exception cref="Exception">没有找到对应的实例</exception>
        public void Inject(object instance)
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
                    // 暂时放入_shelvedInstances中，等到有实例的时候再注入
                    _shelvedInstances.Add(instance, injectedFields);
                    break;
                }
            }

            CheckShelvedInstances();
        }

        /// <summary>
        /// 新增一个Bean（一般是BeanMonoBehaviour）
        /// </summary>
        /// <remarks>
        /// 要注意如果暂时无法完成注入，会被搁置，如果一直被搁置证明没有生成对应的Bean，在外部调用时肯定会报错
        /// </remarks>
        /// <param name="name">Bean的名字</param>
        /// <param name="instance">Bean的实例</param>
        /// <param name="startInject">是否立即注入</param>
        public void AddBean(string name, object instance, bool startInject = true)
        {
            var beanInfo = new BeanInfo(name, instance.GetType());
            _beans[beanInfo] = instance;
            
            // 如果有实现接口，且接口不是Unity相关的接口，也要注册进IoC容器
            var interfaces = instance.GetType().GetInterfaces();
            foreach (var @interface in interfaces)
            {
                if (@interface.Namespace == null || @interface.Namespace.Contains("UnityEngine")) continue;
                beanInfo = new BeanInfo(name, @interface);
                _beans[beanInfo] = instance;
            }
            

            if (startInject)
            {
                Inject(instance);
            }
        }
        
        /// <summary>
        /// 检查_shelvedInstances中的实例是否可以注入
        /// </summary>
        private void CheckShelvedInstances()
        {
            foreach (var ins in _shelvedInstances.ToList())
            {
                var insFields = ins.Key.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => f.GetCustomAttributes(typeof(AutowiredAttribute), true).Length > 0).ToList();
                var injectedFields = ins.Value;
                var count = insFields.Count;

                foreach (var field in insFields)
                {
                    // 如果Autowired传了名字参数，名字和类型都要匹配
                    var name = field.GetCustomAttribute<AutowiredAttribute>()?.Name;
                    var beanInfo = new BeanInfo(name, field.FieldType);
                    // 如果IoC容器中有这个类型的实例，就注入
                    if (!_beans.TryGetValue(beanInfo, out var value))
                    {
                        continue;
                    }

                    field.SetValue(ins.Key, value);
                    injectedFields.Add(field.Name);
                    count--;
                }

                if (count == 0)
                {
                    _shelvedInstances.Remove(ins);
                }
            }
        }
    }
}