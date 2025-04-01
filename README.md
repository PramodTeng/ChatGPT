# GitHub AI Model Integration Example

This project demonstrates integration with the GitHub AI model inference endpoint using the Azure.AI.OpenAI SDK.

## Prerequisites

1. .NET 8.0 SDK
2. GitHub Personal Access Token with `models:read` permissions

## Setup

1. Set your GitHub token as an environment variable:

   **PowerShell:**

   ```powershell
   $Env:GITHUB_TOKEN="<your-github-token-goes-here>"
   ```

   **Command Prompt:**

   ```cmd
   set GITHUB_TOKEN=<your-github-token-goes-here>
   ```

   **Bash:**

   ```bash
   export GITHUB_TOKEN="<your-github-token-goes-here>"
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

## Running the Examples

Run the project using:

```bash
dotnet run
```

The program will demonstrate several examples:

1. Basic chat completion
2. Multi-turn conversation
3. Streaming output
4. Tool usage with a flight information example

Each example will be clearly marked in the console output.

## Features

- Basic chat completion with the GPT-4o model
- Multi-turn conversation support
- Streaming responses for better user experience
- Function calling with a flight information example

## Notes

- Make sure your GitHub token has the required `models:read` permissions, or the requests will return unauthorized errors.
- This code uses the Azure.AI.OpenAI SDK version 1.0.0-beta.9, not the OpenAI SDK.
- The endpoint used is `https://models.inference.ai.azure.com` for accessing GitHub AI models.
