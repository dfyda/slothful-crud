using SlothfulCrud.Domain;

namespace SlothfulCrud.Tests.Api.Domain
{
    public class Sloth : ISlothfulEntity
    {
        public string Name { get; private set; }

        public Sloth()
        {
            Name = "DapperSloth";
        }
    }
}