using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EasyInject.Attributes;
using EasyInject.Controllers;
using UnityEngine;

namespace EasyInject.Behaviours
{
    /// <summary>
    /// author: spyn
    /// description: BeanMonoBehavior，用于标注这是个可以被依赖注入的Bean类型的MonoBehaviour
    /// </summary>
    public class BeanMonoBehaviour : MonoBehaviour
    {
        private List<FieldInfo> _fields;
        
        private BeanNameAttribute _attribute;

        /// <summary>
        /// 在Awake方法中注入依赖
        /// </summary>
        private void Awake()
        {
            // 如果打了DefaultInjectAttribute特性，就调用IoC的AddInstance方法
            if (GetType().GetCustomAttribute<DefaultInjectAttribute>() != null)
            {
                GlobalInitializer.Instance.AddDefaultInstance(this, OnAwake, true);
            }
            else
            {
                _fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => f.GetCustomAttributes(typeof(AutowiredAttribute), true).Length > 0).ToList();
                // 检查当前实例是否有应用BeanNameAttribute特性
                _attribute = GetType().GetCustomAttribute<BeanNameAttribute>();
                StartCoroutine(WaitForStart());
                StartCoroutine(WaitForDependenciesInjected());
            }
        }

        /// <summary>
        /// 自己的Awake方法
        /// </summary>
        protected virtual void OnAwake()
        {
        }

        // 这里OnAwake提示性能问题，但实际上就调用一次，不会有性能问题
        // ReSharper disable Unity.PerformanceAnalysis
        private IEnumerator WaitForDependenciesInjected()
        {
            // 检查所有需要的依赖项是否都已经被注入
            foreach (var field in _fields)
            {
                while (field.GetValue(this) == null)
                {
                    // 如果依赖项还没有被注入，就等待下一帧
                    yield return null;
                }
            }

            // 当所有依赖项都被注入后，调用OnAwake方法
            OnAwake();
        }
        
        // 等待默认注入的依赖项被注入
        // ReSharper disable Unity.PerformanceAnalysis
        private IEnumerator WaitForStart()
        {
            // 如果IsInjected为false，说明默认依赖项还没有被注入完成，不可以执行非默认注入的操作
            if (!GlobalInitializer.Instance.IsInjected)
            {
                yield return null;
            }
            GlobalInitializer.Instance.AddBean(_attribute != null ? _attribute.Name : string.Empty, this);
        }
    }
}