namespace SlothfulCrud.Types.Configurations
{
    public class GlobalConfiguration : Configuration
    {
        public string SortProperty { get; private set; }
        public string UpdateMethod { get; private set; }

        public GlobalConfiguration()
        {
            SetSortProperty("Name");
            SetUpdateMethod("Update");
        }
        
        public GlobalConfiguration(
            string sortProperty,
            bool exposeAllNestedProperties,
            bool isAuthorizationEnable,
            string[] policyNames) : base(exposeAllNestedProperties, isAuthorizationEnable, policyNames)
        {
            SetSortProperty(sortProperty);
        }

        public void SetSortProperty(string sortProperty)
        {
            SortProperty = sortProperty;
        }

        public void SetUpdateMethod(string updateMethod)
        {
            UpdateMethod = updateMethod;
        }
    }
}