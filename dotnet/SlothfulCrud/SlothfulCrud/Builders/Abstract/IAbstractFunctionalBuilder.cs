namespace SlothfulCrud.Builders.Abstract
{
    public interface IAbstractFunctionalBuilder<TSubject, TSelf>
        where TSelf : IAbstractFunctionalBuilder<TSubject, TSelf>
        where TSubject : class
    {
        TSelf Do(Action<TSubject> action);
    }
}