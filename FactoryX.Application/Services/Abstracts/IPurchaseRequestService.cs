using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface IPurchaseRequestService
{
    Task<IEnumerable<PurchaseRequestDto>> GetAllRequestsAsync(
        PurchaseRequestStatus? status = null,
        int? departmentId = null,
        int? requestedById = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);

    Task<PurchaseRequestDto?> GetRequestByIdAsync(int id);
    Task<PurchaseRequestDto> CreateRequestAsync(CreatePurchaseRequest request, int userId);
    Task<PurchaseRequestDto> SubmitRequestAsync(int id, int userId);
    Task<PurchaseRequestDto> ApproveRequestAsync(int id, int userId);
    Task<PurchaseRequestDto> RejectRequestAsync(int id, int userId, string? reason);
    Task<PurchaseRequestDto> CancelRequestAsync(int id, int userId, string? reason);
}
