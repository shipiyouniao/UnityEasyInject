using System;

namespace EasyInject.Attributes
{
    /// <summary>
    /// author: spyn
    /// description: 标记在场景一开始就存在的MonoBehaviour上的特性，标明是一个Bean
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class GameObjectBeanAttribute : Attribute
    {
        public string Name { get; }
        
        public ENameType NameType { get; }

        public GameObjectBeanAttribute(string name)
        {
            Name = name;
            NameType = ENameType.Custom;
        }
        
        public GameObjectBeanAttribute()
        {
            Name = string.Empty;
            NameType = ENameType.Custom;
        }
        
        public GameObjectBeanAttribute(ENameType nameType)
        {
            NameType = nameType;
        }
    }
    
    public enum ENameType
    {
        /// <summary>
        /// 通过自定义名字作为Bean名
        /// </summary>
        Custom,
        
        /// <summary>
        /// 通过类名作为Bean名
        /// </summary>
        ClassName,
        
        /// <summary>
        /// 通过物体名作为Bean名
        /// </summary>
        GameObjectName,
        
        /// <summary>
        /// 通过字段的值作为Bean名
        /// </summary>
        FieldValue
    }
}