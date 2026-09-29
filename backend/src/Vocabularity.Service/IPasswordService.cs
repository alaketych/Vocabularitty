namespace Vocabularity.Service;

public interface IPasswordService
{
    string Hash(User.Entities.User user, string password);
    bool Verify(User.Entities.User user, string password, out bool needsRehash);
}

