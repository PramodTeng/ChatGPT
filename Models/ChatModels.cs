namespace ChatGPT.Models
{
    public class ChatMessageViewModel
    {
        public string Content { get; set; } = string.Empty;
        public string Role { get; set; } = "user";
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class ChatHistoryViewModel
    {
        public List<ChatMessageViewModel> Messages { get; set; } = new List<ChatMessageViewModel>();
    }

    public class ChatRequestModel
    {
        public string Message { get; set; } = string.Empty;
    }

    public class ChatResponseModel
    {
        public string Content { get; set; } = string.Empty;
        public bool IsError { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class StreamResponseModel
    {
        public string StreamId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsComplete { get; set; } = false;
    }
}