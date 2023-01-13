using API.Dtos;
using API.Entities;

namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhoto();
        Task<Photo> GetPhotoById(int id);
        void RemovePhoto(Photo photo);
    }
}