using Azure.AI.OpenAI;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace ChatGPT.Services
{
    public class ChatService
    {
        private readonly OpenAIClient _client;
        private readonly string _modelName;
        private readonly ConcurrentDictionary<string, List<ChatMessage>> _conversations = new();
        private readonly IConfiguration _configuration;

        public ChatService(OpenAIClient client, IConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
            _modelName = configuration["GitHubAI:Model"] ?? "gpt-4o";
        }

        public async Task<string> GetResponseAsync(string sessionId, string message)
        {
            // Get or create conversation history for this session
            var messages = _conversations.GetOrAdd(sessionId, _ => new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, "You are a helpful assistant powered by GitHub's AI model. Answer user questions concisely and accurately.")
            });

            // Add the user's message
            messages.Add(new ChatMessage(ChatRole.User, message));

            var options = new ChatCompletionsOptions
            {
                DeploymentName = _modelName,
                Temperature = 0.7f,
                MaxTokens = 800
            };

            // Add all messages from the conversation history
            foreach (var msg in messages)
            {
                options.Messages.Add(msg);
            }

            var response = await _client.GetChatCompletionsAsync(options);
            var responseMessage = response.Value.Choices[0].Message.Content;

            // Add the assistant's response to history
            messages.Add(new ChatMessage(ChatRole.Assistant, responseMessage));

            // Keep conversation history at a reasonable size
            if (messages.Count > 20)
            {
                // Remove oldest messages but keep system message
                messages.RemoveRange(1, 2);
            }

            return responseMessage;
        }

        public async Task<(string ResponseContent, string StreamId)> GetStreamingResponseAsync(string sessionId, string message)
        {
            // Get or create conversation history for this session
            var messages = _conversations.GetOrAdd(sessionId, _ => new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, "You are a helpful assistant powered by GitHub's AI model. Answer user questions concisely and accurately.")
            });

            // Add the user's message
            messages.Add(new ChatMessage(ChatRole.User, message));

            var options = new ChatCompletionsOptions
            {
                DeploymentName = _modelName,
                Temperature = 0.7f,
                MaxTokens = 800
            };

            // Add all messages from the conversation history
            foreach (var msg in messages)
            {
                options.Messages.Add(msg);
            }

            var streamId = Guid.NewGuid().ToString();

            // For implementation simplicity, we're capturing the full response here
            // In a real app, you'd stream this to the client
            var streamingResponse = await _client.GetChatCompletionsStreamingAsync(options);
            var fullResponse = new System.Text.StringBuilder();

            await foreach (var update in streamingResponse)
            {
                if (update.ContentUpdate != null)
                {
                    fullResponse.Append(update.ContentUpdate);
                }
            }

            string responseContent = fullResponse.ToString();

            // Add the assistant's response to history
            messages.Add(new ChatMessage(ChatRole.Assistant, responseContent));

            // Keep conversation history at a reasonable size
            if (messages.Count > 20)
            {
                // Remove oldest messages but keep system message
                messages.RemoveRange(1, 2);
            }

            return (responseContent, streamId);
        }

        public async Task GetStreamingResponseAsync(string sessionId, string message, Func<string, Task> onTokenReceived)
        {
            // Get or create conversation history for this session
            var messages = _conversations.GetOrAdd(sessionId, _ => new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, "You are a helpful assistant powered by GitHub's AI model. Answer user questions concisely and accurately.")
            });

            // Add the user's message
            messages.Add(new ChatMessage(ChatRole.User, message));

            var options = new ChatCompletionsOptions
            {
                DeploymentName = _modelName,
                Temperature = 0.7f,
                MaxTokens = 800
            };

            // Add all messages from the conversation history
            foreach (var msg in messages)
            {
                options.Messages.Add(msg);
            }

            // Stream the response and call the callback for each token
            var streamingResponse = await _client.GetChatCompletionsStreamingAsync(options);
            var fullResponse = new System.Text.StringBuilder();

            await foreach (var update in streamingResponse)
            {
                if (update.ContentUpdate != null)
                {
                    await onTokenReceived(update.ContentUpdate);
                    fullResponse.Append(update.ContentUpdate);
                }
            }

            string responseContent = fullResponse.ToString();

            // Add the assistant's response to history
            messages.Add(new ChatMessage(ChatRole.Assistant, responseContent));

            // Keep conversation history at a reasonable size
            if (messages.Count > 20)
            {
                // Remove oldest messages but keep system message
                messages.RemoveRange(1, 2);
            }
        }

        public void ClearConversation(string sessionId)
        {
            if (_conversations.TryRemove(sessionId, out _))
            {
                // Successfully cleared
            }
        }
    }
}