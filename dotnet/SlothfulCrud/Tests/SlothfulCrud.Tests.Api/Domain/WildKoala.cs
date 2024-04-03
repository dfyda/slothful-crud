using SlothfulCrud.Domain;

namespace SlothfulCrud.Tests.Api.Domain
{
    public class WildKoala : ISlothfulEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int Age { get; private set; }
        public int? Age2 { get; private set; }
        public Guid CuisineId { get; private set; }
        public Guid? NeighbourId { get; private set; }
        public Sloth Cuisine { get; private set; }
        public Sloth Neighbour { get; private set; }
        public string DisplayName => Name;

        public WildKoala()
        {
            Id = Guid.NewGuid();
            Name = "SpeedyKoala";
        }
        
        public WildKoala(Guid id, string name, int age, Sloth cuisine, Sloth neighbour)
        {
            Id = id;
            Name = name;
            Age = age;
            CuisineId = cuisine.Id;
            Cuisine = cuisine;
            NeighbourId = neighbour?.Id;
            Neighbour = neighbour;
        }
    }
}