namespace backend.Dtos.Chat
{
    public class ChatRequestDto
    {
        public string Question { get; set; } = string.Empty;
    }

    public class ChatResponseDto
    {
        public string Answer { get; set; } = string.Empty;
        public bool FromModel { get; set; } = true;
    }
}
