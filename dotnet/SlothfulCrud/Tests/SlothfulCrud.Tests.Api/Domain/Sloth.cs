using SlothfulCrud.Domain;

namespace SlothfulCrud.Tests.Api.Domain
{
    public class Sloth : ISlothfulEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int Age { get; private set; }

        public Sloth()
        {
            Id = Guid.NewGuid();
            Name = "DapperSloth";
        }
        
        public Sloth(Guid id, string name, int age)
        {
            Id = id;
            Name = name;
            Age = age;
        }
        
        public void Update(string name, int age)
        {
            Name = name;
            Age = age;
        }
    }
}