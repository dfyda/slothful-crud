namespace SlothfulCrud.Builders.Abstract
{
    internal interface IAbstractFunctionalBuilder<TSubject, TSelf>
        where TSelf : IAbstractFunctionalBuilder<TSubject, TSelf>
        where TSubject : class
    {
        TSelf Do(Action<TSubject> action);
    }
}