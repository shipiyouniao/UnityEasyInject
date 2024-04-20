using System;

namespace EasyInject.Attributes
{
    /// <summary>
    /// author: spyn
    /// description: 打在BeanMonoBehaviour上的特性，传入Bean的名字
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BeanNameAttribute : Attribute
    {
        public string Name { get; }

        public BeanNameAttribute(string name)
        {
            Name = name;
        }
        
        public BeanNameAttribute()
        {
            Name = string.Empty;
        }
    }
}