namespace SlothfulCrud.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        public string Code { get; }

        public NotFoundException(string code, string message) : base(message)
        {
            Code = code;
        }
    }
}