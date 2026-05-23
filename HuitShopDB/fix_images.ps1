$brainDir = "C:\Users\MinhTri\.gemini\antigravity\brain\360624bc-e33e-43d5-ba48-0c6778dbb1a5"
$anhDir = "C:\Users\MinhTri\Documents\GitHub\ecommerce-huit\HuitShopDB\HuitShopDB\Content\Anh"

$map = @{
    "asus_rog_zephyrus_g14" = "Asus_ROG_Zephyrus_G14.jpg"
    "dell_xps_15" = "Dell_XPS_15.jpg"
    "macbook_pro_16_m3_max" = "MacBook_Pro_16_M3_Max.jpg"
    "lenovo_legion_5" = "Lenovo_Legion_5.jpg"
    "hp_spectre_x360" = "HP_Spectre_x360.jpg"
    "apple_watch_series_9" = "Apple_Watch_Series_9.jpg"
    "samsung_galaxy_watch_6" = "Samsung_Galaxy_Watch_6_Classic.jpg"
    "garmin_fenix_7" = "Garmin_Fenix_7.jpg"
    "xiaomi_mi_band_8" = "Xiaomi_Mi_Band_8.jpg"
    "huawei_watch_gt_4" = "Huawei_Watch_GT_4.jpg"
    "cpu_intel_core_i9_14900k" = "CPU_Intel_Core_i9_14900K.jpg"
    "cpu_amd_ryzen_9_7950x3d" = "CPU_AMD_Ryzen_9_7950X3D.jpg"
    "vga_nvidia_rtx_4090" = "VGA_NVIDIA_RTX_4090.jpg"
    "vga_amd_radeon_rx_7900_xtx" = "VGA_AMD_Radeon_RX_7900_XTX.jpg"
    "mainboard_asus_rog_maximus_z790" = "Mainboard_Asus_ROG_Maximus_Z790.jpg"
    "mainboard_msi_mag_b650_tomahawk" = "Mainboard_MSI_MAG_B650_Tomahawk.jpg"
}

foreach ($key in $map.Keys) {
    $srcFiles = Get-ChildItem -Path $brainDir -Filter "$key`_*.png"
    if ($srcFiles.Count -gt 0) {
        $latest = $srcFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        $destPath = Join-Path -Path $anhDir -ChildPath $map[$key]
        Copy-Item -Path $latest.FullName -Destination $destPath -Force
        Write-Host "Copied $key to $($map[$key])"
    }
}

$missing = @{
    "RAM_Corsair_Vengeance_32GB_DDR5.jpg" = "RAM+Corsair+Vengeance+32GB"
    "RAM_G_Skill_Trident_Z5_64GB.jpg" = "RAM+G.Skill+Trident+Z5"
    "SSD_Samsung_990_Pro_2TB.jpg" = "SSD+Samsung+990+Pro"
    "SSD_WD_Black_SN850X_1TB.jpg" = "SSD+WD+Black+SN850X"
}

foreach ($key in $missing.Keys) {
    $text = $missing[$key]
    $url = "https://placehold.co/400x400/1e293b/f8fafc/png?text=$text"
    $destPath = Join-Path -Path $anhDir -ChildPath $key
    Invoke-WebRequest -Uri $url -OutFile $destPath
    Write-Host "Downloaded placeholder for $key"
}
