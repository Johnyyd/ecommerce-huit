using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HuitShopDB.Models;
using HuitShopDB.Models.DTOs.Admin;
using HuitShopDB.Services.Interfaces;

namespace HuitShopDB.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly HuitShopDBDataContext _context;

        public InventoryService()
        {
            _context = new HuitShopDBDataContext();
        }

        public async Task<IEnumerable<InventoryDto>> GetStockLevelByWarehouseAsync(int warehouseId)
        {
            var query = from inv in _context.inventories
                        where warehouseId == 0 || inv.warehouse_id == warehouseId
                        select new InventoryDto
                        {
                            WarehouseId = inv.warehouse_id,
                            WarehouseName = inv.warehouse.name,
                            WarehouseCode = inv.warehouse.code,
                            VariantId = inv.variant_id,
                            Sku = inv.product_variant.sku,
                            ProductName = inv.product_variant.product.name,
                            VariantName = inv.product_variant.variant_name,
                            QuantityOnHand = inv.quantity_on_hand,
                            QuantityReserved = inv.quantity_reserved,
                            AvailableQuantity = inv.quantity_on_hand - inv.quantity_reserved,
                            ReorderPoint = inv.reorder_point
                        };

            return await Task.FromResult(query.ToList());
        }

        public async Task<IEnumerable<LowStockDto>> GetLowStockVariantsAsync(int? warehouseId)
        {
            var query = from inv in _context.inventories
                        where (!warehouseId.HasValue || inv.warehouse_id == warehouseId.Value)
                              && inv.quantity_on_hand <= inv.reorder_point
                        select new LowStockDto
                        {
                            WarehouseId = inv.warehouse_id,
                            WarehouseName = inv.warehouse.name,
                            WarehouseCode = inv.warehouse.code,
                            ProductId = inv.product_variant.product_id,
                            ProductName = inv.product_variant.product.name,
                            VariantId = inv.variant_id,
                            Sku = inv.product_variant.sku,
                            VariantName = inv.product_variant.variant_name,
                            QuantityOnHand = inv.quantity_on_hand,
                            QuantityReserved = inv.quantity_reserved,
                            AvailableQuantity = inv.quantity_on_hand - inv.quantity_reserved,
                            ReorderPoint = inv.reorder_point
                        };

            return await Task.FromResult(query.ToList());
        }

        public async Task<bool> AdjustStockAsync(AdjustStockRequest request)
        {
            var inv = _context.inventories.FirstOrDefault(i => i.warehouse_id == request.WarehouseId && i.variant_id == request.VariantId);
            if (inv == null) return false;

            int oldQty = inv.quantity_on_hand;
            inv.quantity_on_hand += request.QuantityChange;
            inv.last_updated = DateTime.Now;

            var movement = new stock_movement
            {
                warehouse_id = request.WarehouseId,
                variant_id = request.VariantId,
                quantity = Math.Abs(request.QuantityChange),
                movement_type = request.QuantityChange > 0 ? "ADJUST_IN" : "ADJUST_OUT",
                note = request.Note,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            _context.stock_movements.InsertOnSubmit(movement);
            _context.SubmitChanges();

            return await Task.FromResult(true);
        }

        public async Task<bool> ImportStockAsync(ImportStockRequest request)
        {
            var inv = _context.inventories.FirstOrDefault(i => i.warehouse_id == request.WarehouseId && i.variant_id == request.VariantId);
            int quantity = request.Serials.Any() ? request.Serials.Count : 1; // Simplification if no serials provided

            if (inv == null)
            {
                inv = new inventory
                {
                    warehouse_id = request.WarehouseId,
                    variant_id = request.VariantId,
                    quantity_on_hand = quantity,
                    quantity_reserved = 0,
                    reorder_point = 10,
                    last_updated = DateTime.Now
                };
                _context.inventories.InsertOnSubmit(inv);
            }
            else
            {
                inv.quantity_on_hand += quantity;
                inv.last_updated = DateTime.Now;
            }

            var movement = new stock_movement
            {
                warehouse_id = request.WarehouseId,
                variant_id = request.VariantId,
                quantity = quantity,
                movement_type = "IMPORT",
                supplier_id = request.SupplierId,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            _context.stock_movements.InsertOnSubmit(movement);
            
            // Handle Serials if any
            foreach (var s in request.Serials)
            {
                var serial = new product_serial
                {
                    variant_id = request.VariantId,
                    warehouse_id = request.WarehouseId,
                    serial_number = s,
                    status = "AVAILABLE",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };
                _context.product_serials.InsertOnSubmit(serial);
            }

            _context.SubmitChanges();
            return await Task.FromResult(true);
        }

        public async Task<bool> TransferStockAsync(TransferStockRequest request)
        {
            var fromInv = _context.inventories.FirstOrDefault(i => i.warehouse_id == request.FromWarehouseId && i.variant_id == request.VariantId);
            if (fromInv == null || fromInv.quantity_on_hand < request.Quantity) return false;

            var toInv = _context.inventories.FirstOrDefault(i => i.warehouse_id == request.ToWarehouseId && i.variant_id == request.VariantId);

            // Subtract from source
            fromInv.quantity_on_hand -= request.Quantity;
            fromInv.last_updated = DateTime.Now;

            // Add to destination
            if (toInv == null)
            {
                toInv = new inventory
                {
                    warehouse_id = request.ToWarehouseId,
                    variant_id = request.VariantId,
                    quantity_on_hand = request.Quantity,
                    quantity_reserved = 0,
                    reorder_point = 10,
                    last_updated = DateTime.Now
                };
                _context.inventories.InsertOnSubmit(toInv);
            }
            else
            {
                toInv.quantity_on_hand += request.Quantity;
                toInv.last_updated = DateTime.Now;
            }

            // Record movements
            var moveOut = new stock_movement
            {
                warehouse_id = request.FromWarehouseId,
                variant_id = request.VariantId,
                quantity = request.Quantity,
                movement_type = "TRANSFER_OUT",
                note = "Transfer to " + request.ToWarehouseId + ". " + request.Note,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            var moveIn = new stock_movement
            {
                warehouse_id = request.ToWarehouseId,
                variant_id = request.VariantId,
                quantity = request.Quantity,
                movement_type = "TRANSFER_IN",
                note = "Transfer from " + request.FromWarehouseId + ". " + request.Note,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            _context.stock_movements.InsertOnSubmit(moveOut);
            _context.stock_movements.InsertOnSubmit(moveIn);

            _context.SubmitChanges();
            return await Task.FromResult(true);
        }

        public async Task<IEnumerable<StockMovementDto>> GetStockMovementsAsync(int warehouseId = 0, int? variantId = null)
        {
            var query = from m in _context.stock_movements
                        where (warehouseId == 0 || m.warehouse_id == warehouseId)
                              && (!variantId.HasValue || m.variant_id == variantId.Value)
                        orderby m.created_at descending
                        select new StockMovementDto
                        {
                            WarehouseId = m.warehouse_id,
                            WarehouseName = m.warehouse.name,
                            VariantId = m.variant_id,
                            Sku = m.product_variant.sku,
                            ProductName = m.product_variant.product.name,
                            VariantName = m.product_variant.variant_name,
                            Quantity = m.quantity,
                            MovementType = m.movement_type,
                            Note = m.note,
                            CreatedAt = m.created_at
                        };

            return await Task.FromResult(query.ToList());
        }

        // Additional helper for UI
        public async Task<IEnumerable<warehouse>> GetWarehousesAsync()
        {
            return await Task.FromResult(_context.warehouses.ToList());
        }

        public async Task<WarehouseAnalyticsDto> GetWarehouseAnalyticsAsync()
        {
            var analytics = new WarehouseAnalyticsDto();

            // Get warehouse count
            var warehouses = _context.warehouses.ToList();
            analytics.TotalWarehouses = warehouses.Count;

            // Get inventory stats
            var inventories = _context.inventories.ToList();
            analytics.TotalSKUs = inventories.Select(i => i.product_variant.sku).Distinct().Count();
            analytics.TotalItemsInStock = inventories.Sum(i => i.quantity_on_hand);
            analytics.TotalItemsReserved = inventories.Sum(i => i.quantity_reserved);
            analytics.LowStockItemsCount = inventories.Count(i => i.quantity_on_hand <= i.reorder_point);

            // Get warehouse stats
            foreach (var warehouse in warehouses)
            {
                var warehouseInventories = inventories.Where(i => i.warehouse_id == warehouse.id).ToList();
                var stats = new WarehouseStatsDto
                {
                    WarehouseId = warehouse.id,
                    WarehouseName = warehouse.name,
                    WarehouseCode = warehouse.code,
                    TotalItems = warehouseInventories.Sum(i => i.quantity_on_hand),
                    ReservedItems = warehouseInventories.Sum(i => i.quantity_reserved),
                    AvailableItems = warehouseInventories.Sum(i => i.quantity_on_hand - i.quantity_reserved),
                    SKUCount = warehouseInventories.Select(i => i.product_variant.sku).Distinct().Count(),
                    LowStockCount = warehouseInventories.Count(i => i.quantity_on_hand <= i.reorder_point)
                };
                analytics.WarehouseStats.Add(stats);
            }

            return await Task.FromResult(analytics);
        }

        public async Task<IEnumerable<InventoryReorderReportDto>> GetReorderReportAsync()
        {
            var report = new List<InventoryReorderReportDto>();
            
            var lowStockItems = _context.inventories
                .Where(i => i.quantity_on_hand <= i.reorder_point)
                .GroupBy(i => i.variant_id)
                .ToList();

            foreach (var group in lowStockItems)
            {
                var variant = group.First().product_variant;
                var reportItem = new InventoryReorderReportDto
                {
                    ProductId = variant.product_id,
                    ProductName = variant.product.name,
                    Sku = variant.sku,
                    VariantId = variant.id,
                    VariantName = variant.variant_name,
                    TotalQuantityAcrossWarehouses = group.Sum(i => i.quantity_on_hand),
                    ReorderPoint = group.First().reorder_point
                };

                // Determine status
                if (reportItem.TotalQuantityAcrossWarehouses <= reportItem.ReorderPoint / 2)
                    reportItem.ReorderStatus = "URGENT";
                else if (reportItem.TotalQuantityAcrossWarehouses <= reportItem.ReorderPoint)
                    reportItem.ReorderStatus = "WARNING";
                else
                    reportItem.ReorderStatus = "OK";

                foreach (var inv in group)
                {
                    reportItem.StockByWarehouse.Add(new WarehouseStockDto
                    {
                        WarehouseId = inv.warehouse_id,
                        WarehouseName = inv.warehouse.name,
                        Quantity = inv.quantity_on_hand,
                        Reserved = inv.quantity_reserved
                    });
                }

                report.Add(reportItem);
            }

            return await Task.FromResult(report);
        }

        public async Task<IEnumerable<StockMovementDto>> GetStockMovementsFilteredAsync(StockMovementFilterRequest filter)
        {
            var query = _context.stock_movements.AsQueryable();

            if (filter.WarehouseId.HasValue)
                query = query.Where(m => m.warehouse_id == filter.WarehouseId.Value);

            if (filter.VariantId.HasValue)
                query = query.Where(m => m.variant_id == filter.VariantId.Value);

            if (!string.IsNullOrEmpty(filter.MovementType))
                query = query.Where(m => m.movement_type == filter.MovementType);

            if (filter.FromDate.HasValue)
                query = query.Where(m => m.created_at >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(m => m.created_at <= filter.ToDate.Value.AddDays(1));

            var movements = query
                .OrderByDescending(m => m.created_at)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var result = movements.Select(m => new StockMovementDto
            {
                WarehouseId = m.warehouse_id,
                WarehouseName = m.warehouse.name,
                VariantId = m.variant_id,
                Sku = m.product_variant.sku,
                ProductName = m.product_variant.product.name,
                VariantName = m.product_variant.variant_name,
                Quantity = m.quantity,
                MovementType = m.movement_type,
                Note = m.note,
                CreatedAt = m.created_at
            }).ToList();

            return await Task.FromResult(result);
        }
    }
}
