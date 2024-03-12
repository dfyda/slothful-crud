using SlothfulCrud.Domain;

namespace SlothfulCrud.Tests.Api.Domain
{
    public class Koala : ISlothfulEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int Age { get; }
        public Sloth Cuisine { get; private set; }

        public Koala()
        {
            Id = Guid.NewGuid();
            Name = "SpeedyKoala";
        }
        
        public Koala(Guid id, string name, int age)
        {
            Id = id;
            Name = name;
            Age = age;
        }
    }
}