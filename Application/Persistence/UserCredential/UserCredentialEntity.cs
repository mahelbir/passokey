using Application.Persistence.User;

namespace Application.Persistence.UserCredential;

public class UserCredentialEntity : Entity
{
    public Guid UserId { get; set; }
    public byte[] CredentialId { get; set; }
    public byte[] PublicKey { get; set; }
    public uint SignCount { get; set; }

    public UserEntity? User { get; set; }
}