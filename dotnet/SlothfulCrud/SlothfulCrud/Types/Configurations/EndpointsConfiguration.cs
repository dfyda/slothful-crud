namespace SlothfulCrud.Types.Configurations
{
    public class EndpointsConfiguration
    {
        public EndpointConfiguration Get { get; private set; }
        public EndpointConfiguration Browse { get; private set; }
        public EndpointConfiguration Create { get; private set; }
        public EndpointConfiguration Update { get; private set; }
        public EndpointConfiguration Delete { get; private set; }
        public GlobalConfiguration Global { get; private set; }
        
        public EndpointsConfiguration()
        {
            Get = new EndpointConfiguration();
            Browse = new EndpointConfiguration();
            Create = new EndpointConfiguration();
            Update = new EndpointConfiguration();
            Delete = new EndpointConfiguration();
            Global = new GlobalConfiguration();
        }
        
        public EndpointsConfiguration(
            EndpointConfiguration get,
            EndpointConfiguration browse,
            EndpointConfiguration create,
            EndpointConfiguration update,
            EndpointConfiguration delete,
            GlobalConfiguration global)
        {
            Get = get;
            Browse = browse;
            Create = create;
            Update = update;
            Delete = delete;
            Global = global;
        }
    }
}