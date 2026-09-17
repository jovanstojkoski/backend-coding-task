namespace Claims.Application.Abstractions;

public interface IClaimsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
