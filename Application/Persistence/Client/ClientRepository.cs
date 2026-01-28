namespace Application.Persistence.Client;

public class ClientRepository(AppDbContext context) : GenericRepository<ClientEntity>(context)
{
}
