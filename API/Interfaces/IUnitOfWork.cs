using System.Reflection.Metadata;

namespace API.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository userRepository { get; }
        IMessageRepository messageRepository { get; }
        ILikesRepository likesRepository { get; }
        IPhotoRepository photoRepository { get; }
        Task<bool> Complete();
        bool HasChanges();
    }
}