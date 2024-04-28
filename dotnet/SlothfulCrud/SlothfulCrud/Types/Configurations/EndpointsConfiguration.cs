namespace SlothfulCrud.Types.Configurations
{
    public class EndpointsConfiguration
    {
        public EndpointConfiguration Get { get; private set; }
        public EndpointConfiguration Browse { get; private set; }
        public EndpointConfiguration BrowseSelectable { get; private set; }
        public EndpointConfiguration Create { get; private set; }
        public EndpointConfiguration Update { get; private set; }
        public EndpointConfiguration Delete { get; private set; }
        public EntityConfiguration Entity { get; private set; }
        
        public EndpointsConfiguration()
        {
            Get = new EndpointConfiguration();
            Browse = new EndpointConfiguration();
            BrowseSelectable = new EndpointConfiguration();
            Create = new EndpointConfiguration();
            Update = new EndpointConfiguration();
            Delete = new EndpointConfiguration();
            Entity = new EntityConfiguration();
        }
        
        public EndpointsConfiguration(
            EndpointConfiguration get,
            EndpointConfiguration browse,
            EndpointConfiguration browseSelectable,
            EndpointConfiguration create,
            EndpointConfiguration update,
            EndpointConfiguration delete,
            EntityConfiguration entity)
        {
            Get = get;
            Browse = browse;
            BrowseSelectable = browseSelectable;
            Create = create;
            Update = update;
            Delete = delete;
            Entity = entity;
        }
    }
}