namespace SlothfulCrud.Types.Configurations
{
    public class EndpointConfiguration : Configuration
    {
        public bool IsEnable { get; private set; }

        public EndpointConfiguration()
        {
            SetIsEnable(true);
        }
        
        public EndpointConfiguration(
            bool exposeAllNestedProperties,
            bool isAuthorizationEnable,
            string[] policyNames,
            bool isEnable) : base(exposeAllNestedProperties, isAuthorizationEnable, policyNames)
        {
            SetIsEnable(isEnable);
        }
        
        public void SetIsEnable(bool isEnable)
        {
            IsEnable = isEnable;
        }
    }
}