using Gifter.Models;

namespace Gifter.Repositories
{
    public interface IUserProfileRepository
    {
        List<UserProfile> GetAll();
        UserProfile GetById(int id);
        UserProfile GetByIdWithPosts(int id);
        void Add(UserProfile profile);
        void Update(UserProfile profile);
        void Delete(int id);
    }
}