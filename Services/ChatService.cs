using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using backend.Dtos.Chat;
using backend.Interfaces;

namespace backend.Services
{
    public class ChatService : IChatService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        private const string Decline = "I can only answer questions about cryptography topics (e.g., ciphers, hash functions, authentication, key exchange, number theory for crypto). Please ask a cryptography-related question.";

        public ChatService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<ChatResponseDto> AskAsync(ChatRequestDto request, CancellationToken ct = default)
        {
            var q = (request.Question ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(q))
            {
                return new ChatResponseDto { Answer = Decline, FromModel = false };
            }

            // Lightweight topic guard: allow crypto-only keywords or chapters/tools names
            if (!IsCryptoTopic(q))
            {
                return new ChatResponseDto { Answer = Decline, FromModel = false };
            }

            var groqKey = _config["LLM:GROQ_API_KEY"] ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");
            if (string.IsNullOrWhiteSpace(groqKey))
            {
                // No key configured — return a graceful local response
                return new ChatResponseDto
                {
                    Answer = "The chat model is not configured on the server. Please set GROQ_API_KEY environment variable. Meanwhile, here are topics I can help with: symmetric/asymmetric crypto, AES/DES/RSA, hash functions, MAC/HMAC, key exchange, authentication, and cryptographic math.",
                    FromModel = false
                };
            }

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://api.groq.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", groqKey);

            var sysPrompt = "You are a helpful cryptography tutor for a course platform. Strictly answer only cryptography-related questions (symmetric/asymmetric crypto, hash functions, MAC/HMAC, digital signatures, key exchange, authentication, number theory for crypto, protocols like CBC/CTR/GCM, DES/AES/RSA/ElGamal/Paillier, proofs of properties). If asked anything unrelated, politely decline.";

            var payload = new
            {
                messages = new object[]
                {
                    new { role = "system", content = sysPrompt },
                    new { role = "user", content = q }
                },
                model = "llama-3.1-8b-instant", // free tier on Groq at time of writing
                temperature = 0.2,
                max_tokens = 512,
                stream = false
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var resp = await client.PostAsync("/openai/v1/chat/completions", content, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var fallback = $"Chat service error: {(int)resp.StatusCode}. Please try again later.";
                return new ChatResponseDto { Answer = fallback, FromModel = false };
            }

            using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var root = doc.RootElement;
            // OpenAI-compatible response: choices[0].message.content
            var answer = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
            answer = answer.Trim();
            if (string.IsNullOrWhiteSpace(answer)) answer = Decline;

            return new ChatResponseDto { Answer = answer, FromModel = true };
        }

        public async Task<TextRewriteResponseDto> RewriteAsync(TextRewriteRequestDto request, CancellationToken ct = default)
        {
            var src = (request.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(src))
            {
                return new TextRewriteResponseDto { Rewritten = "", FromModel = false };
            }

            var groqKey = _config["LLM:GROQ_API_KEY"] ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");
            if (string.IsNullOrWhiteSpace(groqKey))
            {
                // return original if no model configured
                return new TextRewriteResponseDto
                {
                    Rewritten = src,
                    FromModel = false
                };
            }

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://api.groq.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", groqKey);

            string InstrFromMode(string? mode)
            {
                if (string.IsNullOrWhiteSpace(mode)) return string.Empty;
                switch (mode.Trim().ToLowerInvariant())
                {
                    case "simplify": return "Rewrite in simpler language for a beginner.";
                    case "eli5": return "Explain like I am five years old, using short sentences and concrete examples.";
                    case "deepen": return "Expand with more depth, reasoning, and worked examples. Keep structure.";
                    case "concise": return "Make it more concise without losing key information.";
                    case "analogies": return "Add clear, accurate analogies to aid understanding.";
                    default: return string.Empty;
                }
            }

            var userInstruction = string.IsNullOrWhiteSpace(request.Instruction)
                ? InstrFromMode(request.Mode)
                : request.Instruction!.Trim();
            if (string.IsNullOrWhiteSpace(userInstruction)) userInstruction = "Rewrite for clarity and learning.";

            var sys = string.Join(' ', new[]
            {
                "You rewrite provided course text per user instruction.",
                request.PreserveStructure ? "Preserve the original structure (headings, lists) where possible." : "",
                "Do not invent new facts. If something is uncertain, say so.",
                "Return Markdown only with no preamble or trailing commentary."
            }.Where(s => !string.IsNullOrWhiteSpace(s)));

            var payload = new
            {
                messages = new object[]
                {
                    new { role = "system", content = sys },
                    new { role = "user", content = $"Instruction: {userInstruction}\n\nText:\n\"\"\"{src}\"\"\"" }
                },
                model = "llama-3.1-8b-instant",
                temperature = 0.2,
                max_tokens = 800,
                stream = false
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var resp = await client.PostAsync("/openai/v1/chat/completions", content, ct);
            if (!resp.IsSuccessStatusCode)
            {
                return new TextRewriteResponseDto { Rewritten = src, FromModel = false };
            }

            using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var root = doc.RootElement;
            var outText = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? src;
            outText = (outText ?? src).Trim();
            if (string.IsNullOrWhiteSpace(outText)) outText = src;

            return new TextRewriteResponseDto { Rewritten = outText, FromModel = true };
        }

        private static bool IsCryptoTopic(string q)
        {
            q = q.ToLowerInvariant();
            string[] allow = new[]
            {
                "cryptography", "cipher", "ciphertext", "plaintext", "key", "block cipher", "stream cipher",
                "des", "aes", "rsa", "elgamal", "paillier", "rabin", "diffie-hellman", "dh", "mac", "hmac",
                "gcm", "cbc", "ctr", "ofb", "cfb", "s-box", "feistel", "mixcolumns", "subbytes", "shiftrows",
                "hash", "sha", "sha256", "sha512", "md5", "merkle", "authentication", "digital signature",
                "signature", "pkcs", "padding", "oaep", "number theory", "modular arithmetic", "modular exponentiation",
                "euclidean", "totient", "euler", "fermat", "primality", "prime", "factorization", "homomorphic",
                "zero-knowledge", "zkp", "one-time pad", "otp", "nonce", "iv", "kdc", "kerberos", "pki", "certificate",
                "attack", "chosen plaintext", "chosen ciphertext", "cryptanalysis", "security parameter"
            };
            return allow.Any(k => q.Contains(k));
        }
    }
}
