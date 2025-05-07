using lab09.Model;

namespace lab09.Services;

public interface IDbService
{
    public Task<VisitDto> GetVisitForId(int visitId);
    
    public Task CreateVisit(AddVisitDto addVisitDto);
}