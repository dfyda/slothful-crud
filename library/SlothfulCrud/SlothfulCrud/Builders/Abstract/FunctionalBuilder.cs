namespace SlothfulCrud.Builders.Abstract
{
    internal class FunctionalBuilder<TSubject, TSelf> : AbstractFunctionalBuilder<TSubject, TSelf>
        where TSelf : FunctionalBuilder<TSubject, TSelf>
        where TSubject : class, new()
    {
        public TSubject Build()
        {
            return Actions.Aggregate(new TSubject(), (subject, action) => action(subject));
        }
    }
}