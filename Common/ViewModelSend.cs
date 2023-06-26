namespace Common
{
    public class ViewModelSend
    {
        public string Message { get; set; }
        public Guid Id { get; set; }
        public ViewModelSend(string message, Guid id)
        {
            Message = message;
            Id = id;
        }
    }
}