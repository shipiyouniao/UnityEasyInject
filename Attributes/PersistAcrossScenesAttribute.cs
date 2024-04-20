using System;

namespace EasyInject.Attributes
{
    /// <summary>
    /// author: spyn
    /// description: 持久化跨场景的Bean
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PersistAcrossScenesAttribute : Attribute
    {
    }
}