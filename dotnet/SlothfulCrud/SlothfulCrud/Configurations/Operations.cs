namespace SlothfulCrud.Configurations
{
    public class Operations
    {
        public bool Read { get; private set; }
        public bool Create { get; private set; }
        public bool Update { get; private set; }
        public bool Delete { get; private set; }
        
        public Operations()
            : this(true, true, true, true)
        {
        }
        
        public Operations(bool read, bool create, bool update, bool delete)
        {
            Read = read;
            Create = create;
            Update = update;
            Delete = delete;
        }
    }
}