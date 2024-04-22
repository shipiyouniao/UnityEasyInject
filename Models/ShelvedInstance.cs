namespace EasyInject.Models
{
    public class ShelvedInstance
    {
        public readonly object Instance;
        public readonly string BeanName;

        public ShelvedInstance(string beanName, object instance)
        {
            Instance = instance;
            BeanName = beanName;
        }

        /// <summary>
        /// 重写Equals方法，用于比较两个ShelvedInstance是否相等
        /// </summary>
        /// <param name="obj">另一个ShelvedInstance</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            if (obj is ShelvedInstance shelvedInstance)
            {
                return Instance == shelvedInstance.Instance && BeanName == shelvedInstance.BeanName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Instance.GetHashCode() ^ BeanName.GetHashCode();
        }
    }
}