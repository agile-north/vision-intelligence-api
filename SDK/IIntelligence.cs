namespace SDK
{
    public abstract class Intelligence<TConfiguration> where TConfiguration : class, new()
    {
        protected TConfiguration Configuration { get; }
        public Intelligence(TConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}