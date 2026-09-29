using Microsoft.EntityFrameworkCore;
using Vocabularity.Core;
using Vocabularity.Service.Auth.Models;
using UserEntity = Vocabularity.Service.User.Entities.User;

namespace Vocabularity.Service.Auth;

public sealed class AuthService(
    IVocabularityDbContext database,
    IPasswordService passwords,
    ITokenService tokens)
{
    public async Task<UserResponse> GetUserAsync(
        string authenticatedUserId,
        string requestedUserId,
        CancellationToken cancellationToken,
        bool isAdministrator = false)
    {
        if (!isAdministrator && authenticatedUserId != requestedUserId)
        {
            throw new ApiException(404, "User not found.");
        }

        return await database.Users
            .AsNoTracking()
            .Where(user => user.Id == requestedUserId)
            .Select(user => new UserResponse(user.Id, user.Email, user.Icon, user.Role))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new ApiException(404, "User not found.");
    }

    public async Task<IReadOnlyList<UserResponse>> ListUsersAsync(
        bool isAdministrator, CancellationToken cancellationToken, int pageNumber = 1, int pageSize = 12)
    {
        if (!isAdministrator)
        {
            throw new ApiException(403, "Administrator access is required.");
        }

        return await database.Users.AsNoTracking()
            .OrderBy(user => user.Email)
            .ThenBy(user => user.Id)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(user => new UserResponse(user.Id, user.Email, user.Icon, user.Role))
            .ToListAsync(cancellationToken);
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();
        var emailExists = await database.Users.AnyAsync(
            user => user.NormalizedEmail == normalizedEmail,
            cancellationToken);

        if (emailExists)
        {
            throw new ApiException(409, "Email is already registered.");
        }

        var user = new UserEntity
        {
            Email = email,
            NormalizedEmail = normalizedEmail,
            PasswordHash = "",
            Icon = request.Icon
        };

        user.PasswordHash = passwords.Hash(user, request.Password);
        database.Users.Add(user);
        await database.SaveChangesAsync(cancellationToken);

        return tokens.Create(user);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await database.Users.SingleOrDefaultAsync(
            user => user.NormalizedEmail == normalizedEmail && user.IsActive,
            cancellationToken);

        if (user is null || !passwords.Verify(user, request.Password, out var needsRehash))
        {
            throw new ApiException(401, "Invalid email or password.");
        }

        if (needsRehash)
        {
            user.PasswordHash = passwords.Hash(user, request.Password);
            user.UpdatedAt = DateTime.UtcNow;
            await database.SaveChangesAsync(cancellationToken);
        }

        return tokens.Create(user);
    }
}

