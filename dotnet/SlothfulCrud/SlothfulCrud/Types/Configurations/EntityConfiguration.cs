namespace SlothfulCrud.Types.Configurations
{
    public class EntityConfiguration : Configuration
    {
        public string SortProperty { get; private set; }
        public string FilterProperty { get; private set; }
        public string UpdateMethod { get; private set; }
        public bool HasValidation { get; private set; }

        public EntityConfiguration()
        {
            SetSortProperty("Name");
            SetFilterProperty("Name");
            SetUpdateMethod("Update");
            SetHasValidation(true);
        }
        
        public EntityConfiguration(
            string sortProperty,
            string filterProperty,
            bool exposeAllNestedProperties,
            bool isAuthorizationEnable,
            string[] policyNames) : base(exposeAllNestedProperties, isAuthorizationEnable, policyNames)
        {
            SetSortProperty(sortProperty);
            SetFilterProperty(filterProperty);
        }

        public void SetSortProperty(string sortProperty)
        {
            SortProperty = sortProperty;
        }

        public void SetFilterProperty(string filterProperty)
        {
            FilterProperty = filterProperty;
        }

        public void SetUpdateMethod(string updateMethod)
        {
            UpdateMethod = updateMethod;
        }

        public void SetHasValidation(bool hasValidation)
        {
            HasValidation = hasValidation;
        }
    }
}