using System;

namespace EasyInject.Attributes
{
    /// <summary>
    /// author: spyn
    /// description: Component特性（用于标注这是个需要被IoC管理的非MonoBehaviour的组件）
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : Attribute
    {
        public string Name { get; }
        
        public ComponentAttribute(string name)
        {
            Name = name;
        }
        
        public ComponentAttribute()
        {
            Name = string.Empty;
        }
    }
}