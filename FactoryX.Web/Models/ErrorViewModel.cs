using System;

namespace FactoryX.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Path { get; set; }
    public string FriendlyMessage { get; set; } = "حدث خطأ غير متوقع أثناء معالجة طلبك. تم تسجيل الحالة برقم المرجع الموضح أدناه للمتابعة مع إدارة النظام.";

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public string ReferenceNumber => !string.IsNullOrEmpty(CorrelationId) ? CorrelationId : (RequestId ?? "N/A");
}
