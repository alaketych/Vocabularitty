using Microsoft.EntityFrameworkCore;
using Vocabularity.Service.Dictionary.Entities;
using Vocabularity.Service.User.Entities;
using DictionaryEntity = Vocabularity.Service.Dictionary.Entities.Dictionary;

namespace Vocabularity.Service;

public interface ITokenService
{
    Auth.Models.AuthResponse Create(User.Entities.User user);
}

