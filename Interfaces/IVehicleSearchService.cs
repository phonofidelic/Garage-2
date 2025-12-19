
namespace Garage_2.Interfaces;

public interface IVehicleSearchService
{

    IQueryable<T> Search<T>(IQueryable<T> query, string? searchString) where T : class;
}
