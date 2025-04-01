using Microsoft.AspNetCore.Mvc;
using ChatGPT.Models;
using ChatGPT.Services;
using System.Diagnostics;
using System.Text;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Http;

namespace ChatGPT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ChatService _chatService;

        public HomeController(ILogger<HomeController> logger, ChatService chatService)
        {
            _logger = logger;
            _chatService = chatService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ChatResponseModel
                {
                    IsError = true,
                    ErrorMessage = "Message cannot be empty"
                });
            }

            try
            {
                string sessionId = HttpContext.Session.Id;
                string response = await _chatService.GetResponseAsync(sessionId, request.Message);

                return Ok(new ChatResponseModel
                {
                    Content = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message");
                return StatusCode(500, new ChatResponseModel
                {
                    IsError = true,
                    ErrorMessage = "An error occurred while processing your request"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> StreamMessage([FromBody] ChatRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ChatResponseModel
                {
                    IsError = true,
                    ErrorMessage = "Message cannot be empty"
                });
            }

            try
            {
                string sessionId = HttpContext.Session.Id;
                var (responseContent, streamId) = await _chatService.GetStreamingResponseAsync(sessionId, request.Message);

                return Ok(new StreamResponseModel
                {
                    StreamId = streamId,
                    Content = responseContent,
                    IsComplete = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing streaming chat message");
                return StatusCode(500, new ChatResponseModel
                {
                    IsError = true,
                    ErrorMessage = "An error occurred while processing your request"
                });
            }
        }

        [HttpGet]
        public async Task StreamMessageSSE([FromQuery] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                Response.StatusCode = 400;
                return;
            }

            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            try
            {
                string sessionId = HttpContext.Session.Id;

                // Tell the client we're starting
                await Response.WriteAsync("event: start\ndata: Processing message\n\n");
                await Response.Body.FlushAsync();

                // Get the messages for this session and add the new message
                await _chatService.GetStreamingResponseAsync(sessionId, message,
                    async (string token) =>
                    {
                        // Send each token as it arrives
                        if (!string.IsNullOrEmpty(token))
                        {
                            // Filter any backslash-n sequences that might appear in the actual content
                            var sanitizedToken = token.Replace("\\n", " ");

                            // Send the token without any special escaping
                            await Response.WriteAsync($"event: message\ndata: {sanitizedToken}\n\n");
                            await Response.Body.FlushAsync();
                            // Add a small delay to simulate more natural typing
                            await Task.Delay(10);
                        }
                    }
                );

                // Tell the client we're done
                await Response.WriteAsync("event: end\ndata: Message complete\n\n");
                await Response.Body.FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error streaming chat message");
                await Response.WriteAsync($"event: error\ndata: {ex.Message}\n\n");
                await Response.Body.FlushAsync();
            }
        }

        [HttpPost]
        public IActionResult ClearConversation()
        {
            try
            {
                string sessionId = HttpContext.Session.Id;
                _chatService.ClearConversation(sessionId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing conversation");
                return StatusCode(500, new { error = "An error occurred while clearing the conversation" });
            }
        }

        [HttpPost]
        public IActionResult ClearChat()
        {
            // Use the session ID from cookie or create a new one
            string sessionId = HttpContext.Request.Cookies["SessionId"];
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                HttpContext.Response.Cookies.Append("SessionId", sessionId, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddDays(7)
                });
            }

            _chatService.ClearConversation(sessionId);
            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}