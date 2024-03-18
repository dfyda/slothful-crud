using SlothfulCrud.Domain;

namespace SlothfulCrud.Tests.Api.Domain
{
    public class WildKoala : ISlothfulEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int Age { get; private set; }
        // public Sloth Cuisine { get; private set; }

        public WildKoala()
        {
            Id = Guid.NewGuid();
            Name = "SpeedyKoala";
        }
        
        public WildKoala(Guid id, string name, int age)
        {
            Id = id;
            Name = name;
            Age = age;
        }
    }
}