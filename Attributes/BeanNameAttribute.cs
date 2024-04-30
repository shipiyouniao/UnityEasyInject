using System;

namespace EasyInject.Attributes
{
    /// <summary>
    /// author: spyn
    /// description: Bean名字属性，打在脚本当中标记名字的字段上，Bean会以这个名字注册到IoC容器中
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class BeanNameAttribute : Attribute
    {
    }
}