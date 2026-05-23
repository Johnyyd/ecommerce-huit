$categories = @("smartphones", "laptops", "tablets", "mobile-accessories")
$products = @()

foreach ($cat in $categories) {
    $url = "https://dummyjson.com/products/category/$cat"
    $response = Invoke-RestMethod -Uri $url
    $products += $response.products
}

# Take exactly 20 products
$products = $products | Select-Object -First 20

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
    $safeTitle = $prod.title -replace '[^a-zA-Z0-9]', '_'
    
    # FORCE JPG EXTENSION
    $fileName = "$safeTitle.jpg"
    $filePath = Join-Path -Path $anhDir -ChildPath $fileName
    
    # Download image
    Invoke-WebRequest -Uri $prod.thumbnail -OutFile $filePath
    
    $thumbnailUrl = "/Content/Anh/$fileName"
    
    $desc = $prod.description -replace "'", "''"
    $title = $prod.title -replace "'", "''"
    $slug = $prod.title.ToLower() -replace '[^a-z0-9]+', '-'
    
    $pVal = "($startProductId, N'$title', '$slug', 1, 1, N'<p>$desc</p>', N'{}', 'ACTIVE', 1)"
    $prodValues += $pVal
    
    $sku = "SKU-ELEC-" + $startProductId
    $price = [math]::Round($prod.price * 25000)
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

$fullSql = "`n-- =====================================================`n-- MORE ELECTRONICS`n-- =====================================================`n$sqlProducts`n$sqlVariants`n$sqlInventories"

$seedFile = "C:\Users\MinhTri\Documents\GitHub\ecommerce-huit\DATABASE\seed.sql"
Add-Content -Path $seedFile -Value $fullSql -Encoding UTF8
Write-Host "Done downloading and updating seed.sql"
