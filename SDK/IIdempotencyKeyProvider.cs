using System.Threading.Tasks;

namespace SDK;

public interface IIdempotencyKeyProvider
{
    Task<string?> ResolveAsync();
}