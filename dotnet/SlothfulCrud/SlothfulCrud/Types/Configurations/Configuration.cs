namespace SlothfulCrud.Types.Configurations
{
    public class Configuration
    {
        public bool ExposeAllNestedProperties { get; private set; }
        public bool IsAuthorizationEnable { get; private set; }
        public string[] PolicyNames { get; private set; }

        protected Configuration()
        {
            SetExposeAllNestedProperties(false);
            SetIsAuthorizationEnable(false);
            SetPolicyNames(Array.Empty<string>());
        }

        protected Configuration(
            bool exposeAllNestedProperties,
            bool isAuthorizationEnable,
            string[] policyNames)
        {
            SetExposeAllNestedProperties(exposeAllNestedProperties);
            SetIsAuthorizationEnable(isAuthorizationEnable);
            SetPolicyNames(policyNames);
        }
        
        public void SetExposeAllNestedProperties(bool exposeAllNestedProperties)
        {
            ExposeAllNestedProperties = exposeAllNestedProperties;
        }
        
        public void SetIsAuthorizationEnable(bool isAuthorizationEnable)
        {
            IsAuthorizationEnable = isAuthorizationEnable;
        }
        
        public void SetPolicyNames(string[] policyNames)
        {
            PolicyNames = policyNames;
        }
    }
}