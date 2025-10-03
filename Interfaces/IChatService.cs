using backend.Dtos.Chat;

namespace backend.Interfaces
{
    public interface IChatService
    {
        Task<ChatResponseDto> AskAsync(ChatRequestDto request, CancellationToken ct = default);
        Task<TextRewriteResponseDto> RewriteAsync(TextRewriteRequestDto request, CancellationToken ct = default);
    }
}
