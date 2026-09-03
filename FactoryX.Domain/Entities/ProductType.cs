namespace FactoryX.Domain.Entities;

public enum ProductType
{
    FinishedProduct = 1,     // منتج تام الصنع
    SemiFinishedProduct = 2, // منتج نصف مصنع / وسيط
    PackagingItem = 3,       // مادة تغليف جاهزة / علبة مجمعة
    AssortedBox = 4          // علبة مشكلة
}
