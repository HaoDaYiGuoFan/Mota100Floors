# recolor_yellow_door.ps1
# 把 Assets/Tiles/door_yellow.png（经典素材原色为青绿色铁门）按色度着色为黄钥匙同色调的金黄色，
# 与 door_red/door_blue 的生成工艺一致（import_classic_assets.ps1 的 Colorize-Rgb）。
# 黄色目标取 generate_tiles.ps1 中黄门的配色 (232,172,34)，与黄钥匙为同一金黄色调。
# 用法：pwsh -File Assets/recolor_yellow_door.ps1
Add-Type -AssemblyName System.Drawing
$ErrorActionPreference = 'Stop'

$path = Join-Path $PSScriptRoot 'Tiles\door_yellow.png'
$tr = 232; $tg = 172; $tb = 34   # 金黄色（与黄钥匙同色调，generate_tiles.ps1 黄门配色）

$img = [System.Drawing.Bitmap]::new($path)
$bmp = [System.Drawing.Bitmap]::new($img.Width, $img.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
for ($y = 0; $y -lt $img.Height; $y++) {
    for ($x = 0; $x -lt $img.Width; $x++) {
        $c = $img.GetPixel($x, $y)
        $r = $c.R; $g = $c.G; $b = $c.B
        $max = [math]::Max($r, [math]::Max($g, $b))
        $min = [math]::Min($r, [math]::Min($g, $b))
        $sat = if ($max -gt 0) { ($max - $min) / $max } else { 0 }
        if ($sat -ge 0.16) {
            # 彩色像素按亮度映射为目标色（灰/白/黑与透明像素保留）；
            # 原青绿铁门整体偏暗，黄色低亮度会读作棕色，先提升亮度再映射
            $lum = (0.30 * $r + 0.59 * $g + 0.11 * $b) / 255.0
            $lum = [math]::Min(1.0, $lum * 1.9)
            $r = [int][math]::Round($tr * $lum)
            $g = [int][math]::Round($tg * $lum)
            $b = [int][math]::Round($tb * $lum)
        }
        $bmp.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($c.A, $r, $g, $b))
    }
}
$img.Dispose()
$bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Host "door_yellow.png 已着色为金黄色调 -> $path"
