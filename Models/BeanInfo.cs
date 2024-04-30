using System;
using System.Collections.Generic;

namespace EasyInject.Models
{
    /// <summary>
    /// author: spyn
    /// description: Bean信息（作为IoC容器的Key）
    /// </summary>
    public class BeanInfo
    {
        public readonly string Name;
        public readonly Type Type;
        // 会出现的场景，不作为比较的条件
        public readonly List<string> Scenes = new();

        public BeanInfo(string name, Type type)
        {
            Name = name;
            Type = type;
        }
        
        public BeanInfo(string name, Type type, string scene)
        {
            Name = name;
            Type = type;
            Scenes.Add(scene);
        }

        /// <summary>
        /// 重写Equals方法，用于比较两个BeanInfo是否相等
        /// </summary>
        /// <param name="obj">另一个BeanInfo</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            if (obj is BeanInfo beanInfo)
            {
                return Name == beanInfo.Name && Type == beanInfo.Type;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Type.GetHashCode();
        }

        public override string ToString()
        {
            return $"BeanInfo: {Name} - {Type}";
        }
    }
}