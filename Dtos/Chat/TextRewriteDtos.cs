namespace backend.Dtos.Chat
{
    public class TextRewriteRequestDto
    {
        public string Text { get; set; } = string.Empty;
        public string? Mode { get; set; } // e.g., simplify, eli5, deepen, concise, analogies
        public string? Instruction { get; set; } // custom instruction overrides Mode
        public bool PreserveStructure { get; set; } = true; // advise model to keep headings/lists
    }

    public class TextRewriteResponseDto
    {
        public string Rewritten { get; set; } = string.Empty;
        public bool FromModel { get; set; } = true;
    }
}
