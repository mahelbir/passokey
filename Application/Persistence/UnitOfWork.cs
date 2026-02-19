namespace Application.Persistence;

public class UnitOfWork(AppDbContext context)
{
    public Task<int> SaveChangesAsync()
    {
        return context.SaveChangesAsync();
    }
}