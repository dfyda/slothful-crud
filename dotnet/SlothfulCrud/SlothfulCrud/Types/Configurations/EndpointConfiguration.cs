namespace SlothfulCrud.Types.Configurations
{
    public class EndpointConfiguration : GlobalConfiguration
    {
        public bool IsEnable { get; private set; }

        public EndpointConfiguration() : base()
        {
            IsEnable = true;
        }
        
        public EndpointConfiguration(
            string sortProperty,
            bool exposeAllNestedProperties,
            bool isAuthorizationEnable,
            string[] policyNames,
            bool isEnable) : base(sortProperty, exposeAllNestedProperties, isAuthorizationEnable, policyNames)
        {
            IsEnable = isEnable;
        }
        
        public void SetIsEnable(bool isEnable)
        {
            IsEnable = isEnable;
        }
    }
}