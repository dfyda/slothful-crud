namespace SlothfulCrud.Types.Configurations
{
    public class EndpointConfiguration : GlobalConfiguration
    {
        public bool IsEnable { get; private set; }

        public EndpointConfiguration() : base()
        {
            IsEnable = true;
        }
        
        public void SetIsEnable(bool isEnable)
        {
            IsEnable = isEnable;
        }
    }
}