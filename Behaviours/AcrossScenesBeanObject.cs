using EasyInject.Attributes;

namespace EasyInject.Behaviours
{
    /// <summary>
    /// author: spyn
    /// description: 跨场景Bean组件（用于想要作为Bean但没有自己特殊逻辑组件脚本的MonoBehavior）
    /// </summary>
    [PersistAcrossScenes]
    public class AcrossScenesBeanObject : BeanObject
    {
        private void Awake()
        {
            // 保证不被销毁
            DontDestroyOnLoad(gameObject);
        }
    }
}