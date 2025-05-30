namespace Application.Exceptions
{
    public class DeleteFailureException : Exception
    {
        public DeleteFailureException(string name, object key, string reason)
            : base($"Deletion of entity \"{name}\" ({key}) failed. Reason: {reason}")
        {
        }
         public DeleteFailureException(string message) : base(message)
        {
        }
    }
} 