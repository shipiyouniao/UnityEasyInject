using EasyInject.Controllers;
using UnityEngine;

namespace EasyInject.Behaviours
{
    /// <summary>
    /// author: spyn
    /// description: 普通Bean组件（用于想要作为Bean但没有自己特殊逻辑组件脚本的MonoBehavior）
    /// </summary>
    public class BeanObject : MonoBehaviour
    {
        public bool isDefault;
        
        private void Awake()
        {
            // 这里只给isDefault为false的Bean实现依赖注入
            if (!isDefault)
            {
                GlobalInitializer.Instance.AddBean(name, this);
            }
        }
    }
}