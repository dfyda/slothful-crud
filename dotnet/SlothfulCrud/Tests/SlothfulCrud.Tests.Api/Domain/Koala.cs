using SlothfulCrud.Domain;

namespace SlothfulCrud.Tests.Api.Domain
{
    public class Koala : ISlothfulEntity
    {
        public string Name { get; private set; }

        public Koala()
        {
            Name = "SpeedyKoala";
        }
    }
}