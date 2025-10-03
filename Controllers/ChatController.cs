using backend.Dtos.Chat;
using backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("ask")]
        public async Task<ActionResult<ChatResponseDto>> Ask([FromBody] ChatRequestDto req, CancellationToken ct)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Question))
            {
                return BadRequest(new ChatResponseDto { Answer = "Please provide a non-empty question about cryptography." });
            }

            var res = await _chatService.AskAsync(req, ct);
            return Ok(res);
        }

        [HttpPost("rewrite")]
        public async Task<ActionResult<TextRewriteResponseDto>> Rewrite([FromBody] TextRewriteRequestDto req, CancellationToken ct)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Text))
            {
                return BadRequest(new TextRewriteResponseDto { Rewritten = string.Empty, FromModel = false });
            }

            var res = await _chatService.RewriteAsync(req, ct);
            return Ok(res);
        }
    }
}
