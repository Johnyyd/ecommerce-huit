namespace ECommerce.Huit.Domain.Enums
{
    public enum MovementType
    {
        PURCHASE,        // Nhập hàng từ NCC
        SALE_RESERVED,   // Giữ chỗ khi khách đặt
        SALE_PICKED,     // Lấy hàng đóng gói
        SALE_SHIPPED,    // Giao hàng thành công
        RETURN_RESTOCK,  // Khách trả hàng hoàn kho
        ADJUSTMENT,      // Kiểm kê điều chỉnh
        TRANSFER         // Chuyển kho
    }
}
