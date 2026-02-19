namespace Application.Persistence.UserCredential;

public class UserCredentialRepository(AppDbContext context) : GenericRepository<UserCredentialEntity>(context)
{
}