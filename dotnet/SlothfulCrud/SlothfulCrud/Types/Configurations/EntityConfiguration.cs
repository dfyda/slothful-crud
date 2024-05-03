namespace SlothfulCrud.Types.Configurations
{
    public class EntityConfiguration : Configuration
    {
        public string SortProperty { get; private set; }
        public string FilterProperty { get; private set; }
        public string KeyProperty { get; private set; }
        public Type KeyPropertyType { get; private set; }
        public string UpdateMethod { get; private set; }
        public bool HasValidation { get; private set; }

        public EntityConfiguration()
        {
            SetSortProperty("Name");
            SetFilterProperty("Name");
            SetUpdateMethod("Update");
            SetKeyProperty("Id");
            SetKeyPropertyType(typeof(Guid));
            SetHasValidation(true);
        }
        
        public EntityConfiguration(
            string sortProperty,
            string filterProperty,
            string keyProperty,
            Type keyPropertyType,
            bool exposeAllNestedProperties,
            bool isAuthorizationEnable,
            string[] policyNames) : base(exposeAllNestedProperties, isAuthorizationEnable, policyNames)
        {
            SetSortProperty(sortProperty);
            SetFilterProperty(filterProperty);
            SetKeyProperty(keyProperty);
            SetKeyPropertyType(keyPropertyType);
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
        
        public void SetKeyProperty(string keyProperty)
        {
            KeyProperty = keyProperty;
        }
        
        public void SetKeyPropertyType(Type keyPropertyType)
        {
            KeyPropertyType = keyPropertyType;
        }

        public void SetHasValidation(bool hasValidation)
        {
            HasValidation = hasValidation;
        }
    }
}