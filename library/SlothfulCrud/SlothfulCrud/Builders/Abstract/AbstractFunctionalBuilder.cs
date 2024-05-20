namespace SlothfulCrud.Builders.Abstract
{
    public class AbstractFunctionalBuilder<TSubject, TSelf> : IAbstractFunctionalBuilder<TSubject, TSelf>
        where TSelf : AbstractFunctionalBuilder<TSubject, TSelf>
        where TSubject : class
    {
        protected readonly ICollection<Func<TSubject, TSubject>> Actions = new List<Func<TSubject, TSubject>>();
        
        public TSelf Do(Action<TSubject> action)
        {
            return AddAction(action);
        }
        
        private TSelf AddAction(Action<TSubject> action)
        {
            Actions.Add(subject =>
            {
                action(subject);
                return subject;
            });
            return (TSelf)this;
        }
    }
}