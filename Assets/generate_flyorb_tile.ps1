# generate_flyorb_tile.ps1
# 生成飞行器（楼层穿梭机）32x32 像素贴图：
#   item_flyorb.png —— 地图 / 背包道具图标
#   icon_flyorb.png —— 面板小图标（带金色描边）
# 用法：pwsh -ExecutionPolicy Bypass -File Assets/generate_flyorb_tile.ps1
Add-Type -AssemblyName System.Drawing

$ErrorActionPreference = 'Stop'
$OutDir = Join-Path $PSScriptRoot 'Tiles'
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$C = [System.Drawing.Color]
$Body      = $C::FromArgb(226, 210, 175)   # 金属米色（机壳）
$BodyDark  = $C::FromArgb(150, 130, 95)    # 机壳暗部
$Cockpit   = $C::FromArgb(120, 200, 235)   # 座舱蓝
$CockpitHi = $C::FromArgb(210, 242, 255)   # 座舱高光
$Jet       = $C::FromArgb(70, 205, 170)    # 喷气青
$JetHi     = $C::FromArgb(215, 255, 235)   # 喷气高光
$Gold      = $C::FromArgb(255, 202, 40)    # 金色描边

function PT([object[]]$pts) {
    $out = @()
    foreach ($p in $pts) { $out += [System.Drawing.Point]::new([int]($p[0]), [int]($p[1])) }
    $out
}
function BR($g, $x, $y, $w, $h, $color) {
    $b = [System.Drawing.SolidBrush]::new($color)
    $g.FillRectangle($b, [single]$x, [single]$y, [single]$w, [single]$h)
    $b.Dispose()
}
function BE($g, $x, $y, $w, $h, $color) {
    $b = [System.Drawing.SolidBrush]::new($color)
    $g.FillEllipse($b, [single]$x, [single]$y, [single]$w, [single]$h)
    $b.Dispose()
}
function BP($g, $pts, $color) {
    $b = [System.Drawing.SolidBrush]::new($color)
    $g.FillPolygon($b, $pts)
    $b.Dispose()
}

function New-Sprite([string]$name, [scriptblock]$draw) {
    $bmp = [System.Drawing.Bitmap]::new(32, 32, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)
    & $draw $g
    $g.Dispose()
    $bmp.Save((Join-Path $OutDir $name), [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host ("  + " + $name)
}

# 复古 8bit 圆盘飞行器（俯视 + 底部喷气）
function Draw-FlyOrb($g, [bool]$isIcon) {
    $s = 1.0
    # 底部喷气
    BE $g (11 * $s) (20 * $s) (10 * $s) (9 * $s) $Jet
    BE $g (13 * $s) (23 * $s) (6 * $s) (5 * $s) $JetHi
    # 机身边缘暗部（下缘阴影）
    BE $g (4 * $s) (9 * $s) (24 * $s) (13 * $s) $BodyDark
    # 主体圆盘
    BE $g (5 * $s) (7 * $s) (22 * $s) (12 * $s) $Body
    # 顶部座舱
    BE $g (11 * $s) (5 * $s) (10 * $s) (9 * $s) $Cockpit
    BE $g (13 * $s) (6 * $s) (5 * $s) (4 * $s) $CockpitHi
    # 铆钉
    BE $g (7 * $s) (11 * $s) 2 2 $BodyDark
    BE $g (23 * $s) (11 * $s) 2 2 $BodyDark
    if ($isIcon) {
        $p = [System.Drawing.Pen]::new($Gold, [single]1.4)
        $g.DrawEllipse($p, [single](5 * $s), [single](7 * $s), [single](22 * $s), [single](12 * $s))
        $p.Dispose()
    }
}

New-Sprite 'item_flyorb.png' { param($g) Draw-FlyOrb $g $false }
New-Sprite 'icon_flyorb.png' { param($g) Draw-FlyOrb $g $true }
Write-Host "飞行器贴图生成完毕 -> $OutDir"
