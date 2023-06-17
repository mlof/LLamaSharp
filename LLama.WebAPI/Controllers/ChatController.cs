using LLama.WebAPI.Models;
using LLama.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace LLama.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly ChatService _service;

        public ChatController(ILogger<ChatController> logger,
            ChatService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost("Send")]
        public string SendMessage([FromBody] SendMessageInput input)
        {
            return _service.Send(input);
        }
    }
}
