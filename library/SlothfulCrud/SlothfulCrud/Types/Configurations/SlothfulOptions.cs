namespace SlothfulCrud.Types.Configurations
{
    public class SlothfulOptions
    {
        public bool UseSlothfulProblemHandling { get; set; }
        
        /// <summary>
        /// Optional hook to customize every EF Core query before execution.
        /// Receives and returns a non-generic <see cref="IQueryable"/>.
        /// Useful for applying provider-specific optimizations such as AsSplitQuery().
        /// </summary>
        public Func<IQueryable, IQueryable> QueryCustomizer { get; set; }
    }
}