namespace SlothfulCrud.Types.Configurations
{
    public class GlobalConfiguration
    {
        public string SortProperty { get; private set; }
        public bool ExposeAllNestedProperties { get; private set; }
        public bool IsAuthorizationEnable { get; private set; }
        public string[] PolicyNames { get; private set; }

        public GlobalConfiguration()
        {
            SortProperty = "Name";
            ExposeAllNestedProperties = false;
            IsAuthorizationEnable = false;
            PolicyNames = Array.Empty<string>();
        }
        
        public GlobalConfiguration(
            string sortProperty,
            bool exposeAllNestedProperties,
            bool isAuthorizationEnable,
            string[] policyNames)
        {
            SortProperty = sortProperty;
            ExposeAllNestedProperties = exposeAllNestedProperties;
            IsAuthorizationEnable = isAuthorizationEnable;
            PolicyNames = policyNames;
        }

        public void SetSortProperty(string sortProperty)
        {
            SortProperty = sortProperty;
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