using System;
using System.Collections.Generic;

namespace EasyInject.Attributes
{
    /// <summary>
    /// author: spyn
    /// description: 加上这个特性并传入场景名字后，会在场景加载（IoC容器初始化）时被自动注入然后再调用OnStart方法
    /// </summary>
    /// <remarks>只允许单例使用，否则会导致Bean过多不会被注入</remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultInjectAttribute : Attribute
    {
        public List<string> SceneNameList { get; }

        public DefaultInjectAttribute(params string[] sceneName)
        {
            SceneNameList = new List<string>(sceneName);
        }
    }
}