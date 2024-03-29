namespace SlothfulCrud.Domain
{
    public interface ISlothfulEntity
    {
        Guid Id { get; }
        public string DisplayName { get; }
    }
}