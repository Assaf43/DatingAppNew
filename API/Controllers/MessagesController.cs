using API.Dtos;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public MessagesController(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var userName = User.GetUserName();

            if (userName == createMessageDto.RecipientUsername.ToLower()) return BadRequest("You cannot send message to yourself");

            var sender = await _uow.userRepository.GetUserByUsernameAsync(userName);
            var recipient = await _uow.userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) return NotFound("No recipient found");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _uow.messageRepository.AddMessage(message);

            if (await _uow.Complete()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessageForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUserName();
            var messages = await _uow.messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));

            return messages;
        }

        // [HttpGet("thread/{username}")]
        // public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagThread(string username)
        // {
        //     var currentUsername = User.GetUserName();
        //     return Ok(await _uow.messageRepository.GetMessageThread(currentUsername, username));
        // }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserName();
            var message = await _uow.messageRepository.GetMessage(id);

            if (message.SenderUsername != username && message.RecipientUsername != username)
            {
                return Unauthorized();
            }

            if (message.SenderUsername == username) message.SenderDeleted = true;
            if (message.RecipientUsername == username) message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _uow.messageRepository.DeleteMessage(message);
            }

            if (await _uow.Complete()) return Ok();

            return BadRequest("Problem in deleting tje message");

        }

    }
}