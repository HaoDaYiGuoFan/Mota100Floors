# import_classic_assets.ps1
# 从经典 pygame 魔塔参考实现（MagicTowerGame-master）导入美术资源到 Assets/Tiles：
#   · 玩家四方向火柴人       player_up / player_down / player_left / player_right.png
#   · 6 种经典怪物           monster_classic_1..6.png
#   · 三色门 / 三色钥匙      door_yellow/red/blue.png、key_yellow/red/blue.png
#                           （红、蓝版本由黄色原图按色度着色生成）
#   · 商店 / 楼梯 / 血瓶 / NPC / 胜利星
# 原版素材普遍带 (230,230,230) 灰底，脚本会把该底色转换为透明，便于与现地图瓦片混排。
# 用法：
#   pwsh -File Assets/import_classic_assets.ps1 [-SourceDir <img目录>] [-OutDir <输出目录>]
param(
    [string]$SourceDir = 'E:\PrivateProjects\Private\MagicTowerGame-master\img',
    [string]$OutDir = (Join-Path $PSScriptRoot 'Tiles')
)

Add-Type -AssemblyName System.Drawing

$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

# 是否属于“经典灰底”(230,230,230)（±容差）——这些像素应转透明
function Is-GreyBackdrop([int]$r, [int]$g, [int]$b, [int]$tolerance = 16) {
    return [math]::Abs($r - 230) -le $tolerance -and `
           [math]::Abs($g - 230) -le $tolerance -and `
           [math]::Abs($b - 230) -le $tolerance
}

# 彩色像素着色：把原图的彩色区（饱和度超过阈值）按亮度映射为目标色，灰/白/黑保留
function Colorize-Rgb([int]$r, [int]$g, [int]$b, [int]$tr, [int]$tg, [int]$tb) {
    $max = [math]::Max($r, [math]::Max($g, $b))
    $min = [math]::Min($r, [math]::Min($g, $b))
    $sat = if ($max -gt 0) { ($max - $min) / $max } else { 0 }
    if ($sat -lt 0.16) { return @($r, $g, $b) }
    $lum = (0.30 * $r + 0.59 * $g + 0.11 * $b) / 255.0
    return @(
        [int][math]::Round($tr * $lum),
        [int][math]::Round($tg * $lum),
        [int][math]::Round($tb * $lum))
}

# 处理单张素材：可选透明化 + 可选着色，另存为 PNG
function Import-Sprite(
    [string]$src,
    [string]$name,
    [switch]$clr,
    [int]$tr = 0, [int]$tg = 0, [int]$tb = 0) {

    $img = [System.Drawing.Bitmap]::new((Join-Path $SourceDir $src))
    $bmp = [System.Drawing.Bitmap]::new(
        $img.Width, $img.Height,
        [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    for ($y = 0; $y -lt $img.Height; $y++) {
        for ($x = 0; $x -lt $img.Width; $x++) {
            $c = $img.GetPixel($x, $y)
            $a = 255
            if (Is-GreyBackdrop $c.R $c.G $c.B) { $a = 0 }
            $r = $c.R; $g = $c.G; $b = $c.B
            if ($clr) {
                $rgb = Colorize-Rgb $r $g $b $tr $tg $tb
                $r = $rgb[0]; $g = $rgb[1]; $b = $rgb[2]
            }
            $bmp.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($a, $r, $g, $b))
        }
    }
    $img.Dispose()
    $bmp.Save((Join-Path $OutDir $name), [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host ("  + " + $name)
}

# ---------- 玩家四方向 ----------
Import-Sprite 'up.jpg' 'player_up.png'
Import-Sprite 'down.jpg' 'player_down.png'
Import-Sprite 'left.jpg' 'player_left.png'
Import-Sprite 'right.jpg' 'player_right.png'

# ---------- 6 种经典怪物 ----------
Import-Sprite 'monster1.jpg' 'monster_classic_1.png'
Import-Sprite 'monster2.jpg' 'monster_classic_2.png'
Import-Sprite 'monster3.jpg' 'monster_classic_3.png'
Import-Sprite 'monster4.jpg' 'monster_classic_4.png'
Import-Sprite 'monster5.jpg' 'monster_classic_5.png'
Import-Sprite 'monster6.jpg' 'monster_classic_6.png'

# ---------- 三色门 / 三色钥匙（红、蓝为着色版本） ----------
Import-Sprite 'door.jpg' 'door_yellow.png'
Import-Sprite 'door.jpg' 'door_red.png' -clr -tr 206 -tg 46 -tb 58
Import-Sprite 'door.jpg' 'door_blue.png' -clr -tr 58 -tg 112 -tb 232
Import-Sprite 'key.jpg' 'key_yellow.png'
Import-Sprite 'key.jpg' 'key_red.png' -clr -tr 212 -tg 52 -tb 62
Import-Sprite 'key.jpg' 'key_blue.png' -clr -tr 62 -tg 112 -tb 232

# ---------- 场景 / 道具 / NPC ----------
Import-Sprite 'shop.jpg' 'shop.png'
Import-Sprite 'upstairs.jpg' 'stairup.png'
Import-Sprite 'downstairs.jpg' 'stairdown.png'
Import-Sprite 'blood.jpg' 'item_hp.png'
Import-Sprite 'smile.jpg' 'npc.png'
Import-Sprite 'star.jpg' 'star.png'

Write-Host "经典资源导入完成 -> $OutDir"