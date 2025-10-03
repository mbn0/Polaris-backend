 Cryptography Chatbot

This backend exposes a simple cryptography-only chatbot endpoint powered by a free Groq model.

## Endpoint
- POST `/api/chat/ask` (authentication required)

Request body:
```
{
  "question": "What is the difference between AES-CBC and AES-GCM?"
}
```

Response:
```
{
  "answer": "...",
  "fromModel": true
}
```

## Configuration
- Set environment variable `GROQ_API_KEY` with your Groq API key.
- Alternatively, add it to configuration as `LLM:GROQ_API_KEY`.

If no API key is configured, the service returns a helpful fallback message and does not call any external API.

## Behavior
- The chatbot strictly answers cryptography topics; non-crypto questions are politely declined.
- A local topic guard and a model system prompt enforce this policy.

## Frontend
- Visit `/chat` in the Angular app for a simple chat UI.
