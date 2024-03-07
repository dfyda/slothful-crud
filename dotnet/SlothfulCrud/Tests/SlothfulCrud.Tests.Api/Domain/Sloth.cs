using SlothfulCrud.Domain;

namespace SlothfulCrud.Tests.Api.Domain
{
    public class Sloth : ISlothfulEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public Sloth()
        {
            Id = Guid.NewGuid();
            Name = "DapperSloth";
        }
        
        public Sloth(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }
    }
}