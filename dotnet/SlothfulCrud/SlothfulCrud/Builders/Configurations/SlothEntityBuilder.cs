using System.Linq.Expressions;
using SlothfulCrud.Builders.Configurations.Methods;
using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Configurations
{
    public class SlothEntityBuilder<TEntity> where TEntity : class, ISlothfulEntity
    {
        public SlothfulGetEndpointConfigurationBuilder<TEntity> GetEndpoint => new(EndpointsConfiguration.Get);
        public SlothfulBrowseEndpointConfigurationBuilder<TEntity> BrowseEndpoint => new(EndpointsConfiguration.Browse);
        public SlothfulBrowseEndpointConfigurationBuilder<TEntity> BrowseSelectableEndpoint => new(EndpointsConfiguration.BrowseSelectable);
        public SlothfulCreateEndpointConfigurationBuilder<TEntity> CreateEndpoint => new(EndpointsConfiguration.Create);
        public SlothfulUpdateEndpointConfigurationBuilder<TEntity> UpdateEndpoint => new(EndpointsConfiguration.Update);
        public SlothfulDeleteEndpointConfigurationBuilder<TEntity> DeleteEndpoint => new(EndpointsConfiguration.Delete);
        protected EndpointsConfiguration EndpointsConfiguration { get; set; }

        public SlothEntityBuilder()
        {
            EndpointsConfiguration = new EndpointsConfiguration();
        }
        
        public virtual SlothEntityBuilder<TEntity> AllowAnonymous()
        {
            EndpointsConfiguration.Entity.SetIsAuthorizationEnable(false);
            return this;
        }
        
        public virtual SlothEntityBuilder<TEntity> RequireAuthorization(params string[] policyNames)
        {
            EndpointsConfiguration.Entity.SetIsAuthorizationEnable(true);
            EndpointsConfiguration.Entity.SetPolicyNames(policyNames);
            return this;
        }
        
        public SlothEntityBuilder<TEntity> SetSortProperty<TProperty>(Expression<Func<TEntity, TProperty>> sortExpression)
        {
            if (sortExpression.Body is MemberExpression memberExpression)
            {
                var sortProperty = memberExpression.Member.Name;
                EndpointsConfiguration.Entity.SetSortProperty(sortProperty);
            }
            else
            {
                throw new ArgumentException("Argument must be a property", nameof(sortExpression));
            }
            
            return this;
        }
        
        public SlothEntityBuilder<TEntity> SetUpdateMethodName(string updateMethodName)
        {
            EndpointsConfiguration.Entity.SetUpdateMethod(updateMethodName);
            return this;
        }
        
        public virtual SlothEntityBuilder<TEntity> ExposeAllNestedProperties(bool expose = true)
        {
            EndpointsConfiguration.Entity.SetExposeAllNestedProperties(expose);
            return this;
        }
        
        public EndpointsConfiguration Build()
        {
            var item = new EndpointsConfiguration(
                GetEndpoint.Configuration,
                BrowseEndpoint.Configuration,
                BrowseSelectableEndpoint.Configuration,
                CreateEndpoint.Configuration,
                UpdateEndpoint.Configuration,
                DeleteEndpoint.Configuration,
                EndpointsConfiguration.Entity);
            return item;
        }
    }
}