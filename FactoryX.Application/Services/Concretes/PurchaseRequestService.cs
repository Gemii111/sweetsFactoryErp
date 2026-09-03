using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class PurchaseRequestService : IPurchaseRequestService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;

    public PurchaseRequestService(IRepositoryManager repositoryManager, IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PurchaseRequestDto>> GetAllRequestsAsync(
        PurchaseRequestStatus? status = null,
        int? departmentId = null,
        int? requestedById = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var requests = await _repositoryManager.PurchaseRequestRepository.GetAllRequestsAsync(
            status, departmentId, requestedById, fromDate, toDate, searchTerm);
        return _mapper.Map<IEnumerable<PurchaseRequestDto>>(requests);
    }

    public async Task<PurchaseRequestDto?> GetRequestByIdAsync(int id)
    {
        var request = await _repositoryManager.PurchaseRequestRepository.GetByIdWithDetailsAsync(id);
        return _mapper.Map<PurchaseRequestDto>(request);
    }

    public async Task<PurchaseRequestDto> CreateRequestAsync(CreatePurchaseRequest request, int userId)
    {
        if (request.Items == null || !request.Items.Any())
        {
            throw new InvalidOperationException("يجب إضافة بند واحد على الأقل في طلب الشراء.");
        }

        // Validate items and materials
        foreach (var item in request.Items)
        {
            if (item.RequestedQuantity <= 0)
            {
                throw new InvalidOperationException("كمية البند المطلوب يجب أن تكون أكبر من الصفر.");
            }

            var material = await _repositoryManager.MaterialRepository.GetByIdWithDetailsAsync(item.MaterialId);
            if (material == null || !material.IsActive)
            {
                throw new InvalidOperationException($"المادة الخام بالمعرف #{item.MaterialId} غير موجودة أو غير نشطة.");
            }
        }

        // Generate deterministic Request Number: PR-YYYYMMDD-XXXX
        var date = request.RequestDate.Date;
        var count = await _repositoryManager.PurchaseRequestRepository.GetCountForDateAsync(date);
        var requestNumber = $"PR-{date:yyyyMMdd}-{(count + 1):D4}";

        // Ensure uniqueness
        int suffix = 1;
        while (!await _repositoryManager.PurchaseRequestRepository.IsRequestNumberUniqueAsync(requestNumber))
        {
            requestNumber = $"PR-{date:yyyyMMdd}-{(count + 1 + suffix):D4}";
            suffix++;
        }

        var pr = new PurchaseRequest
        {
            RequestNumber = requestNumber,
            RequestDate = request.RequestDate,
            RequiredDate = request.RequiredDate,
            DepartmentId = request.DepartmentId,
            Priority = request.Priority,
            Status = PurchaseRequestStatus.Draft,
            RequestedByUserId = userId,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = request.Items.Select(i => new PurchaseRequestItem
            {
                MaterialId = i.MaterialId,
                RequestedQuantity = i.RequestedQuantity,
                Unit = i.Unit,
                EstimatedUnitPrice = i.EstimatedUnitPrice,
                RequiredDate = i.RequiredDate ?? request.RequiredDate,
                Notes = i.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList()
        };

        _repositoryManager.PurchaseRequestRepository.Create(pr);
        await _repositoryManager.SaveAsync();

        return (await GetRequestByIdAsync(pr.Id))!;
    }

    public async Task<PurchaseRequestDto> SubmitRequestAsync(int id, int userId)
    {
        var pr = await _repositoryManager.PurchaseRequestRepository.GetByIdWithDetailsAsync(id, trackChanges: true);
        if (pr == null)
        {
            throw new KeyNotFoundException($"طلب الشراء برقم #{id} غير موجود.");
        }

        if (pr.Status != PurchaseRequestStatus.Draft)
        {
            throw new InvalidOperationException($"لا يمكن تقديم طلب الشراء إلا إذا كان في حالة مسودة (Draft). الحالة الحالية: {pr.Status}.");
        }

        pr.Status = PurchaseRequestStatus.Submitted;
        pr.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.PurchaseRequestRepository.Update(pr);
        await _repositoryManager.SaveAsync();

        return (await GetRequestByIdAsync(pr.Id))!;
    }

    public async Task<PurchaseRequestDto> ApproveRequestAsync(int id, int userId)
    {
        var pr = await _repositoryManager.PurchaseRequestRepository.GetByIdWithDetailsAsync(id, trackChanges: true);
        if (pr == null)
        {
            throw new KeyNotFoundException($"طلب الشراء برقم #{id} غير موجود.");
        }

        if (pr.Status != PurchaseRequestStatus.Submitted && pr.Status != PurchaseRequestStatus.Draft)
        {
            throw new InvalidOperationException($"لا يمكن اعتماد طلب الشراء إلا إذا كان مقدماً (Submitted) أو مسودة. الحالة الحالية: {pr.Status}.");
        }

        pr.Status = PurchaseRequestStatus.Approved;
        pr.ApprovedByUserId = userId;
        pr.ApprovedAt = DateTime.UtcNow;
        pr.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.PurchaseRequestRepository.Update(pr);
        await _repositoryManager.SaveAsync();

        return (await GetRequestByIdAsync(pr.Id))!;
    }

    public async Task<PurchaseRequestDto> RejectRequestAsync(int id, int userId, string? reason)
    {
        var pr = await _repositoryManager.PurchaseRequestRepository.GetByIdWithDetailsAsync(id, trackChanges: true);
        if (pr == null)
        {
            throw new KeyNotFoundException($"طلب الشراء برقم #{id} غير موجود.");
        }

        if (pr.Status == PurchaseRequestStatus.Approved || pr.Status == PurchaseRequestStatus.Cancelled)
        {
            throw new InvalidOperationException($"لا يمكن رفض طلب شراء معتمد أو ملغي مسبقاً.");
        }

        pr.Status = PurchaseRequestStatus.Rejected;
        pr.Notes = string.IsNullOrWhiteSpace(reason) ? pr.Notes : $"{pr.Notes} [سبب الرفض: {reason}]".Trim();
        pr.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.PurchaseRequestRepository.Update(pr);
        await _repositoryManager.SaveAsync();

        return (await GetRequestByIdAsync(pr.Id))!;
    }

    public async Task<PurchaseRequestDto> CancelRequestAsync(int id, int userId, string? reason)
    {
        var pr = await _repositoryManager.PurchaseRequestRepository.GetByIdWithDetailsAsync(id, trackChanges: true);
        if (pr == null)
        {
            throw new KeyNotFoundException($"طلب الشراء برقم #{id} غير موجود.");
        }

        if (pr.PurchaseOrders != null && pr.PurchaseOrders.Any(po => po.Status != PurchaseOrderStatus.Cancelled))
        {
            throw new InvalidOperationException("لا يمكن إلغاء طلب الشراء لأنه مرتبط بأمر شراء جارٍ تنفيذه.");
        }

        pr.Status = PurchaseRequestStatus.Cancelled;
        pr.Notes = string.IsNullOrWhiteSpace(reason) ? pr.Notes : $"{pr.Notes} [سبب الإلغاء: {reason}]".Trim();
        pr.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.PurchaseRequestRepository.Update(pr);
        await _repositoryManager.SaveAsync();

        return (await GetRequestByIdAsync(pr.Id))!;
    }
}
