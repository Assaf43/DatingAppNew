using API.Dtos;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly IUnitOfWork _uow;

        public MessageHub(IUnitOfWork uow, IMapper mapper,
            IHubContext<PresenceHub> presenceHub)
        {
            _uow = uow;
            _presenceHub = presenceHub;
            _mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUserName(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var message = await _uow.messageRepository.GetMessageThread(
                Context.User.GetUserName(), otherUser);

            if (_uow.HasChanges())
            {
                await _uow.Complete();
            }

            await Clients.Caller.SendAsync("ReceiveMessageThread", message);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var userName = Context.User.GetUserName();

            if (userName == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("You cannot send message to yourself");

            var sender = await _uow.userRepository.GetUserByUsernameAsync(userName);
            var recipient = await _uow.userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) throw new HubException("No recipient found");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            var gropuName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await _uow.messageRepository.GetMessageGroup(gropuName);

            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker.GetConnenctionsForUser(recipient.UserName);
                if (connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync(
                        "NewMessageReceived", new
                        {
                            username = sender.UserName,
                            knownAs = sender.KnownAs
                        }
                    );
                }
            }

            _uow.messageRepository.AddMessage(message);

            if (await _uow.Complete())
            {
                await Clients.Groups(gropuName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }


        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _uow.messageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());

            if (group == null)
            {
                group = new Group(groupName);
                _uow.messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await _uow.Complete()) return group;

            throw new HubException("Failed To Add To Group");

        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _uow.messageRepository.GetGroupForConnention(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _uow.messageRepository.RemoveConnection(connection);
            if (await _uow.Complete()) return group;

            throw new HubException("Failed To Removed From Group");
        }
    }
}