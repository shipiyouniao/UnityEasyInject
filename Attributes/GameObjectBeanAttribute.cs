using System;

namespace EasyInject.Attributes
{
    /// <summary>
    /// author: spyn
    /// description: 打在MonoBehaviour上的特性，标明是一个Bean
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class GameObjectBeanAttribute : Attribute
    {
        public string Name { get; }

        public GameObjectBeanAttribute(string name)
        {
            Name = name;
        }
        
        public GameObjectBeanAttribute()
        {
            Name = string.Empty;
        }
    }
}