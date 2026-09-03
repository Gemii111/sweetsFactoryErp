namespace FactoryX.Domain.Entities;

public enum RecipeStatus
{
    Draft = 1,    // مسودة - قيد الإعداد وقابلة للتعديل
    Active = 2,   // نشطة ومعتمدة للإنتاج
    Inactive = 3  // معطلة / مؤرشفة
}
