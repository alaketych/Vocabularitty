using Microsoft.AspNetCore.Identity;
using Vocabularity.Service;
using Vocabularity.Service.User.Entities;

namespace Vocabularity.Infrastructure.Authentication;

public sealed class PasswordService(IPasswordHasher<User> hasher) : IPasswordService
{
    public string Hash(User user, string password) => hasher.HashPassword(user, password);
    public bool Verify(User user, string password, out bool needsRehash)
    {
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        needsRehash = result == PasswordVerificationResult.SuccessRehashNeeded;
        return result != PasswordVerificationResult.Failed;
    }
}

