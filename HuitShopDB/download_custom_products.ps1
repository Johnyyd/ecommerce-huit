$products = @(
    @{Title="Asus ROG Zephyrus G14"; Cat=1; Brand=4; Price=35000000},
    @{Title="Dell XPS 15"; Cat=2; Brand=3; Price=45000000},
    @{Title="MacBook Pro 16 M3 Max"; Cat=2; Brand=1; Price=85000000},
    @{Title="Lenovo Legion 5"; Cat=1; Brand=4; Price=30000000},
    @{Title="HP Spectre x360"; Cat=2; Brand=3; Price=40000000},
    @{Title="Apple Watch Series 9"; Cat=5; Brand=1; Price=10000000},
    @{Title="Samsung Galaxy Watch 6 Classic"; Cat=5; Brand=2; Price=8000000},
    @{Title="Garmin Fenix 7"; Cat=5; Brand=5; Price=15000000},
    @{Title="Xiaomi Mi Band 8"; Cat=5; Brand=6; Price=1000000},
    @{Title="Huawei Watch GT 4"; Cat=5; Brand=6; Price=6000000},
    @{Title="CPU Intel Core i9-14900K"; Cat=5; Brand=3; Price=16000000},
    @{Title="CPU AMD Ryzen 9 7950X3D"; Cat=5; Brand=3; Price=18000000},
    @{Title="VGA NVIDIA RTX 4090"; Cat=5; Brand=4; Price=45000000},
    @{Title="VGA AMD Radeon RX 7900 XTX"; Cat=5; Brand=4; Price=30000000},
    @{Title="Mainboard Asus ROG Maximus Z790"; Cat=5; Brand=4; Price=15000000},
    @{Title="Mainboard MSI MAG B650 Tomahawk"; Cat=5; Brand=4; Price=6000000},
    @{Title="RAM Corsair Vengeance 32GB DDR5"; Cat=5; Brand=5; Price=3500000},
    @{Title="RAM G.Skill Trident Z5 64GB"; Cat=5; Brand=5; Price=7000000},
    @{Title="SSD Samsung 990 Pro 2TB"; Cat=5; Brand=2; Price=4500000},
    @{Title="SSD WD Black SN850X 1TB"; Cat=5; Brand=5; Price=2500000}
)

$anhDir = "C:\Users\MinhTri\Documents\GitHub\ecommerce-huit\HuitShopDB\HuitShopDB\Content\Anh"
if (-not (Test-Path -Path $anhDir)) {
    New-Item -ItemType Directory -Path $anhDir | Out-Null
}

$sqlProducts = "SET IDENTITY_INSERT products ON;`nINSERT INTO products (id, name, slug, brand_id, category_id, description, specifications, status, created_by) VALUES `n"
$sqlVariants = "SET IDENTITY_INSERT product_variants ON;`nINSERT INTO product_variants (id, product_id, sku, variant_name, price, original_price, cost_price, thumbnail_url, display_order) VALUES `n"
$sqlInventories = "INSERT INTO inventories (warehouse_id, variant_id, quantity_on_hand, quantity_reserved, reorder_point) VALUES `n"

$startProductId = 6
$startVariantId = 9

$prodValues = @()
$varValues = @()
$invValues = @()

foreach ($prod in $products) {
    # File name preparation
    $safeTitle = $prod.Title -replace '[^a-zA-Z0-9]', '_'
    $slug = $prod.Title.ToLower() -replace '[^a-z0-9]+', '-'
    
    $fileName = "$safeTitle.jpg"
    $filePath = Join-Path -Path $anhDir -ChildPath $fileName
    
    # Download image from picsum.photos using slug as seed to get a consistent unique image
    $imgUrl = "https://picsum.photos/seed/$slug/400/400.jpg"
    Invoke-WebRequest -Uri $imgUrl -OutFile $filePath
    
    $thumbnailUrl = "/Content/Anh/$fileName"
    
    $desc = "Sản phẩm chính hãng " + $prod.Title
    $title = $prod.Title
    
    $catId = $prod.Cat
    $brandId = $prod.Brand
    
    $pVal = "($startProductId, N'$title', '$slug', $brandId, $catId, N'<p>$desc</p>', N'{}', 'ACTIVE', 1)"
    $prodValues += $pVal
    
    $sku = "SKU-" + $startProductId
    $price = $prod.Price
    $originalPrice = [math]::Round($price * 1.2)
    $costPrice = [math]::Round($price * 0.8)
    
    $vVal = "($startVariantId, $startProductId, '$sku', N'Mặc định', $price, $originalPrice, $costPrice, '$thumbnailUrl', 1)"
    $varValues += $vVal
    
    $iVal = "(1, $startVariantId, 100, 0, 10)"
    $invValues += $iVal
    
    $startProductId++
    $startVariantId++
}

$sqlProducts += ($prodValues -join ",`n") + ";`nSET IDENTITY_INSERT products OFF;`nGO`n"
$sqlVariants += ($varValues -join ",`n") + ";`nSET IDENTITY_INSERT product_variants OFF;`nGO`n"
$sqlInventories += ($invValues -join ",`n") + ";`nGO`n"

$fullSql = "`n-- =====================================================`n-- MORE ELECTRONICS: LAPTOP, WEARABLES, PC COMPONENTS`n-- =====================================================`n$sqlProducts`n$sqlVariants`n$sqlInventories"

$seedFile = "C:\Users\MinhTri\Documents\GitHub\ecommerce-huit\DATABASE\seed.sql"
Add-Content -Path $seedFile -Value $fullSql -Encoding UTF8
Write-Host "Done"
