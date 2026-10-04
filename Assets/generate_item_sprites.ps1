# generate_item_sprites.ps1
# 生成 12 种道具贴图 item_1~12.png（32x32，透明底），样式对齐原版魔塔：
#   1~4   药水系：红药水 / 蓝药水 / 大血瓶 / 圣血瓶 —— 圆瓶身+瓶颈+软木塞，体积逐档变大
#   5~8   红宝石系：菱形宝石，四档大小递增，神之档带金色放射光线
#   9~12  蓝宝石系：同上（蓝）
# 用法：pwsh -ExecutionPolicy Bypass -File Assets/generate_item_sprites.ps1
Add-Type -AssemblyName System.Drawing

$ErrorActionPreference = 'Stop'
$OutDir = Join-Path $PSScriptRoot 'Tiles'
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$C = [System.Drawing.Color]
$White = $C::FromArgb(255, 255, 255)
$Black = $C::FromArgb(28, 24, 24)
$WoodDark = $C::FromArgb(96, 66, 42)
$WoodLight = $C::FromArgb(148, 106, 68)
$Gold = $C::FromArgb(255, 202, 40)
$GoldDark = $C::FromArgb(196, 142, 22)
$Glass = $C::FromArgb(226, 234, 240)
$GlassDark = $C::FromArgb(120, 132, 148)

# 多边形顶点：PT @((x,y), (x,y), ...) -> System.Drawing.Point[]（强类型，DrawPolygon 需要）
function PT([object[]]$pts) {
    $out = [System.Drawing.Point[]]::new($pts.Count)
    for ($i = 0; $i -lt $pts.Count; $i++) {
        $out[$i] = [System.Drawing.Point]::new([int]($pts[$i][0]), [int]($pts[$i][1]))
    }
    return $out
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
function DL($g, $x1, $y1, $x2, $y2, $color, $w) {
    $p = [System.Drawing.Pen]::new($color, [single]$w)
    $g.DrawLine($p, [single]$x1, [single]$y1, [single]$x2, [single]$y2)
    $p.Dispose()
}
function OE($g, $x, $y, $w, $h, $color, $width) {
    $p = [System.Drawing.Pen]::new($color, [single]$width)
    $g.DrawEllipse($p, [single]$x, [single]$y, [single]$w, [single]$h)
    $p.Dispose()
}
function OP($g, $pts, $color, $width) {
    $p = [System.Drawing.Pen]::new($color, [single]$width)
    $g.DrawPolygon($p, [System.Drawing.Point[]]$pts)
    $p.Dispose()
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

# ---------- 药水：圆瓶身 + 瓶颈 + 软木塞 ----------
# $bodyW/$bodyH 瓶身尺寸，$bodyY 瓶身顶部 y，$liquid/$liquidDark 药液色，$capColor 瓶塞色，$crossColor 瓶身十字（圣血瓶）
function Draw-Flask($g, $bodyW, $bodyH, $bodyY, $liquid, $liquidDark, $capColor, $crossColor) {
    $bx = 16 - $bodyW / 2
    $neckW = [math]::Max(4, [int]($bodyW * 0.3))
    $neckH = [math]::Max(3, [int]($bodyH * 0.22))
    $capW = $neckW + 2

    # 瓶塞 + 瓶颈（玻璃）
    BR $g (16 - $capW / 2) ($bodyY - 3) $capW 3 $capColor
    BR $g (16 - $capW / 2) ($bodyY - 3) $capW 1 $White
    BR $g (16 - $neckW / 2) $bodyY $neckW $neckH $Glass
    DL $g (16 - $neckW / 2) $bodyY (16 - $neckW / 2) ($bodyY + $neckH) $GlassDark 1
    DL $g (16 + $neckW / 2) $bodyY (16 + $neckW / 2) ($bodyY + $neckH) $GlassDark 1

    # 瓶身（玻璃轮廓 + 药液 + 高光）
    BE $g $bx $bodyY $bodyW $bodyH $Glass
    OE $g $bx $bodyY $bodyW $bodyH $GlassDark 1
    $liqY = $bodyY + [int]($bodyH * 0.38)
    $liqH = $bodyY + $bodyH - $liqY - 2
    BE $g ($bx + 2) $liqY ($bodyW - 4) $liqH $liquid
    BE $g ($bx + 2) ($bodyY + $bodyH - [int]($bodyH * 0.3)) ($bodyW - 4) ([int]($bodyH * 0.28)) $liquidDark
    BE $g ($bx + [int]($bodyW * 0.2)) ($bodyY + [int]($bodyH * 0.14)) ([int]($bodyW * 0.3)) ([int]($bodyH * 0.22)) $White

    # 圣血瓶的金色十字
    if ($crossColor) {
        $cw = [math]::Max(2, [int]($bodyW * 0.16))
        $cl = [int]($bodyH * 0.5)
        $cx = 16 - [int]($cw / 2)
        $cy = $bodyY + [int]($bodyH * 0.42)
        BR $g $cx $cy $cw $cl $crossColor
        BR $g ($cx - [int]($cl / 2) + $cw) ($cy + [int]($cl / 2) - [int]($cw / 2)) $cl $cw $crossColor
    }
}

# ---------- 宝石：菱形，四档大小，神之档金色光线 ----------
# $s 菱形半对角线长；$holy 为真时背后画金色斜十字光线
function Draw-Gem($g, $s, $outline, $main, $light, $dark, [bool]$holy) {
    $cx = 16; $cy = 16
    if ($holy) {
        DL $g ($cx - 13) ($cy - 13) ($cx + 13) ($cy + 13) $Gold 2
        DL $g ($cx + 13) ($cy - 13) ($cx - 13) ($cy + 13) $Gold 2
        BE $g ($cx - $s - 2) ($cy - $s - 2) (2 * $s + 4) (2 * $s + 4) $C::FromArgb(255, 244, 200)
    }

    # 菱形主体：上半亮面、下半暗面
    $top = PT @(($cx, ($cy - $s)), (($cx + $s), $cy), ($cx, ($cy + $s)), (($cx - $s), $cy))
    BP $g $top $main
    BP $g (PT @(($cx, ($cy - $s)), (($cx + $s), $cy), ($cx, ($cy + $s)))) $dark
    BP $g (PT @(($cx, ($cy - $s)), (($cx - $s), $cy), ($cx, $cy))) $light
    OP $g $top $outline 1

    # 星光点
    $sx = $cx - [int]($s * 0.4); $sy = $cy - [int]($s * 0.4)
    BP $g (PT @(($sx, ($sy - 2)), (($sx + 2), $sy), ($sx, ($sy + 2)), (($sx - 2), $sy))) $White
    BE $g ($cx + [int]($s * 0.45)) ($cy - 1) 2 2 $White
}

$RedOutline = $C::FromArgb(146, 22, 34)
$RedMain = $C::FromArgb(226, 56, 64)
$RedLight = $C::FromArgb(250, 132, 132)
$RedDark = $C::FromArgb(178, 34, 46)
$BlueOutline = $C::FromArgb(22, 50, 140)
$BlueMain = $C::FromArgb(62, 110, 232)
$BlueLight = $C::FromArgb(134, 180, 250)
$BlueDark = $C::FromArgb(42, 78, 180)

# ---------- 生成 ----------
# 1~4 药水（体积递增；圣血瓶 = 红液 + 金塞 + 金十字）
New-Sprite 'item_1.png' { param($g) Draw-Flask $g 12 12 15 $C::FromArgb(222, 56, 64) $C::FromArgb(178, 34, 46) $WoodDark $null }
New-Sprite 'item_2.png' { param($g) Draw-Flask $g 12 12 15 $C::FromArgb(62, 110, 232) $C::FromArgb(42, 78, 180) $WoodDark $null }
New-Sprite 'item_3.png' { param($g) Draw-Flask $g 16 16 13 $C::FromArgb(222, 56, 64) $C::FromArgb(178, 34, 46) $WoodDark $null }
New-Sprite 'item_4.png' { param($g) Draw-Flask $g 20 19 11 $C::FromArgb(222, 56, 64) $C::FromArgb(178, 34, 46) $Gold $Gold }

# 5~12 宝石（半对角线 6/8/10/12 递增；神之档 holy）
New-Sprite 'item_5.png' { param($g) Draw-Gem $g 6 $RedOutline $RedMain $RedLight $RedDark $false }
New-Sprite 'item_6.png' { param($g) Draw-Gem $g 8 $RedOutline $RedMain $RedLight $RedDark $false }
New-Sprite 'item_7.png' { param($g) Draw-Gem $g 10 $RedOutline $RedMain $RedLight $RedDark $false }
New-Sprite 'item_8.png' { param($g) Draw-Gem $g 12 $RedOutline $RedMain $RedLight $RedDark $true }
New-Sprite 'item_9.png' { param($g) Draw-Gem $g 6 $BlueOutline $BlueMain $BlueLight $BlueDark $false }
New-Sprite 'item_10.png' { param($g) Draw-Gem $g 8 $BlueOutline $BlueMain $BlueLight $BlueDark $false }
New-Sprite 'item_11.png' { param($g) Draw-Gem $g 10 $BlueOutline $BlueMain $BlueLight $BlueDark $false }
New-Sprite 'item_12.png' { param($g) Draw-Gem $g 12 $BlueOutline $BlueMain $BlueLight $BlueDark $true }

Write-Host "道具贴图生成完毕 -> $OutDir"
