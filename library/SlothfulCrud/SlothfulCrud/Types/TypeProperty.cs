namespace SlothfulCrud.Types
{
    public class TypeProperty
    {
        public string Name { get; private set; }
        public Type Type { get; private set; }
        
        public TypeProperty(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}