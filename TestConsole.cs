using Azure;
using Azure.AI.OpenAI;
using System;
using System.Threading.Tasks;

class TestConsole
{
    private static readonly string endpoint = "https://models.inference.ai.azure.com";
    private static readonly string model = "gpt-4o";

    public static async Task RunTest()
    {
        Console.WriteLine("\n=== GitHub AI Model Test Console ===");
        Console.WriteLine("Type 'exit' to quit\n");

        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("GitHub token not found! Please set the GITHUB_TOKEN environment variable.");
            Console.WriteLine("PowerShell: $Env:GITHUB_TOKEN=\"<your-github-token>\"");
            Console.WriteLine("Command Prompt: set GITHUB_TOKEN=<your-github-token>");
            return;
        }

        try
        {
            var options = new OpenAIClientOptions();
            var client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(token), options);

            Console.WriteLine("Connected to GitHub AI Model. You can start chatting now.");
            Console.WriteLine("---------------------------------------------");

            var chatOptions = new ChatCompletionsOptions
            {
                DeploymentName = model,
                Temperature = 0.7f,
                MaxTokens = 800
            };

            // Add system message
            chatOptions.Messages.Add(new ChatMessage(ChatRole.System,
                "You are a helpful assistant powered by GitHub's AI model. Answer user questions concisely and accurately."));

            while (true)
            {
                Console.Write("\nYou: ");
                var userInput = Console.ReadLine();

                if (string.IsNullOrEmpty(userInput) || userInput.ToLower() == "exit")
                {
                    break;
                }

                // Add user message
                chatOptions.Messages.Add(new ChatMessage(ChatRole.User, userInput));

                Console.Write("\nAI: ");

                try
                {
                    // Use streaming for better UX
                    var streamingResponse = await client.GetChatCompletionsStreamingAsync(chatOptions);
                    string fullResponse = "";

                    await foreach (var update in streamingResponse)
                    {
                        if (update.ContentUpdate != null)
                        {
                            Console.Write(update.ContentUpdate);
                            fullResponse += update.ContentUpdate;
                        }
                    }

                    // Add assistant response to conversation history
                    chatOptions.Messages.Add(new ChatMessage(ChatRole.Assistant, fullResponse));

                    // Keep conversation history manageable (optional)
                    if (chatOptions.Messages.Count > 10)
                    {
                        // Remove oldest messages but keep system message
                        chatOptions.Messages.RemoveAt(1);
                        chatOptions.Messages.RemoveAt(1);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize client: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }
}