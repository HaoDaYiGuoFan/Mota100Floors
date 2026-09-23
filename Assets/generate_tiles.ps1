# generate_tiles.ps1
# 生成魔塔 100 层所需的 32x32 瓦片贴图（System.Drawing 程序化绘制）
# 注意：勿使用 "f(1, 2)" 形式调用自定义函数（会整包成单个数组参数），
#       多边形顶点统一用 PT 助手 + 元组对。
Add-Type -AssemblyName System.Drawing

$ErrorActionPreference = 'Stop'
$OutDir = Join-Path $PSScriptRoot 'Tiles'
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$C = [System.Drawing.Color]
$Floor      = $C::FromArgb(216, 207, 192)
$FloorDark  = $C::FromArgb(184, 173, 155)
$FloorSpeck = $C::FromArgb(28, 138, 127, 108)
$WallBase   = $C::FromArgb(88, 86, 94)
$WallLight  = $C::FromArgb(124, 120, 130)
$WallDark   = $C::FromArgb(52, 50, 58)
$WoodDark   = $C::FromArgb(96, 66, 42)
$WoodLight  = $C::FromArgb(148, 106, 68)
$Gold       = $C::FromArgb(255, 202, 40)
$GoldDark   = $C::FromArgb(196, 142, 22)
$White      = $C::FromArgb(255, 255, 255)
$Black      = $C::FromArgb(22, 20, 18)
$Skin       = $C::FromArgb(240, 205, 160)

# 多边形顶点：PT @((x,y), (x,y), ...) -> System.Drawing.Point[]
function PT([object[]]$pts) {
    $out = @()
    foreach ($p in $pts) {
        $out += [System.Drawing.Point]::new([int]($p[0]), [int]($p[1]))
    }
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
function DL($g, $x1, $y1, $x2, $y2, $color, $w) {
    $p = [System.Drawing.Pen]::new($color, [single]$w)
    $g.DrawLine($p, [single]$x1, [single]$y1, [single]$x2, [single]$y2)
    $p.Dispose()
}
function DA($g, $x, $y, $w, $h, $start, $sweep, $color, $width) {
    $p = [System.Drawing.Pen]::new($color, [single]$width)
    $g.DrawArc($p, [single]$x, [single]$y, [single]$w, [single]$h, [single]$start, [single]$sweep)
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

function Draw-Floor($g) {
    BR $g 0 0 32 32 $Floor
    DL $g 8 0 8 32 $FloorDark 1
    DL $g 16 0 16 32 $FloorDark 1
    DL $g 24 0 24 32 $FloorDark 1
    DL $g 0 8 32 8 $FloorDark 1
    DL $g 0 16 32 16 $FloorDark 1
    DL $g 0 24 32 24 $FloorDark 1
    $rnd = [System.Random]::new(7)
    $sp = [System.Drawing.SolidBrush]::new($FloorSpeck)
    for ($i = 0; $i -lt 90; $i++) { $g.FillRectangle($sp, $rnd.Next(0, 32), $rnd.Next(0, 32), 1, 1) }
    $sp.Dispose()
}

function Draw-Wall($g) {
    BR $g 0 0 32 32 $WallBase
    for ($row = 0; $row -lt 4; $row++) {
        $y = $row * 8
        $off = if ($row % 2 -eq 0) { 0 } else { 8 }
        DL $g 0 $y 32 $y $WallDark 1
        for ($x = $off; $x -lt 32; $x += 16) { DL $g $x $y $x ($y + 8) $WallDark 1 }
        DL $g 0 ($y + 1) 32 ($y + 1) $WallLight 1
    }
    DL $g 0 31 32 31 $WallDark 1
    BR $g 5 14 3 2 $WallDark
    BR $g 20 26 4 2 $WallDark
    BR $g 26 4 2 2 $WallDark
    BR $g 5 4 2 1 $WallLight
    BR $g 15 17 1 3 $WallLight
}

function Draw-StairUp($g) {
    Draw-Floor $g
    $xs = @(2, 8, 14, 20); $ys = @(24, 18, 12, 6)
    for ($i = 0; $i -lt 4; $i++) {
        $x = $xs[$i]; $y = $ys[$i]; $w = 30 - $x
        BR $g $x $y $w 7 $WoodDark
        BR $g $x $y $w 2 $WoodLight
    }
    BP $g (PT @((5, 10), (9, 6), (13, 10), (11, 10), (11, 16), (7, 16), (7, 10))) $Gold
    BR $g 8 15 2 6 $GoldDark
}

function Draw-StairDown($g) {
    Draw-Floor $g
    $xs = @(2, 8, 14, 20); $ys = @(6, 12, 18, 24)
    for ($i = 0; $i -lt 4; $i++) {
        $x = $xs[$i]; $y = $ys[$i]; $w = 30 - $x
        BR $g $x $y $w 7 $C::FromArgb(70, 108, 130)
        BR $g $x $y $w 2 $C::FromArgb(104, 148, 172)
    }
    BP $g (PT @((5, 6), (13, 6), (9, 10))) $C::FromArgb(0, 180, 200)
    BR $g 8 10 2 10 $C::FromArgb(0, 180, 200)
}

function Draw-Door($g, $panelColor) {
    BR $g 3 3 26 26 $WoodDark
    BR $g 3 3 26 3 $WoodLight
    BR $g 3 3 3 26 $WoodLight
    BR $g 6 6 20 20 $panelColor
    BR $g 6 21 20 5 $Black
    BR $g 6 6 20 2 $C::FromArgb(255, 255, 255, 90)
    DL $g 16 7 16 24 $WoodDark 1
    BE $g 8 8 3 3 $GoldDark
    BE $g 21 8 3 3 $GoldDark
    BE $g 8 21 3 3 $GoldDark
    BR $g 21 15 3 3 $Black
    BE $g 21 13 3 3 $Black
}

function Draw-Monster($g, $base, $belly, $hornColor) {
    BE $g 6 26 20 3 $FloorDark
    BE $g 6 7 20 19 $base
    BE $g 9 13 14 9 $belly
    if ($hornColor) {
        BP $g (PT @((9, 9), (13, 2), (15, 10))) $hornColor
        BP $g (PT @((17, 10), (21, 2), (23, 9))) $hornColor
    }
    BE $g 10 12 5 6 $White
    BE $g 17 12 5 6 $White
    BE $g 12 14 2 3 $Black
    BE $g 19 14 2 3 $Black
    DA $g 13 20 6 4 20 140 $Black 1
}

function Draw-HeartShape($g, $color) {
    BE $g 8 9 10 10 $color
    BE $g 14 9 10 10 $color
    BP $g (PT @((7, 17), (25, 17), (16, 28))) $color
    BE $g 10 10 4 3 $White
}

function Draw-ItemHeart($g, $color) {
    Draw-Floor $g
    Draw-HeartShape $g $color
}

function Draw-IconHp($g) {
    Draw-HeartShape $g $C::FromArgb(212, 52, 72)
}

function Draw-SwordShape($g) {
    $old = $g.Transform
    $g.TranslateTransform(16, 16)
    $g.RotateTransform(-45)
    BP $g (PT @((-2, -13), (2, -13), (0, -17))) $C::FromArgb(215, 219, 226)
    BR $g -2 -13 4 16 $C::FromArgb(215, 219, 226)
    DL $g -1 -13 0 3 $C::FromArgb(110, 115, 125) 1
    BR $g -6 2 12 2 $Gold
    BR $g -6 4 12 1 $GoldDark
    BR $g -2 5 4 5 $WoodDark
    BE $g -3 10 6 4 $Gold
    $g.Transform = $old
}

function Draw-ItemSword($g) {
    Draw-Floor $g
    Draw-SwordShape $g
}

function Draw-IconAtk($g) {
    Draw-SwordShape $g
}

function Draw-ShieldShape($g) {
    BP $g (PT @((8, 6), (24, 6), (24, 15), (16, 27), (8, 15))) $C::FromArgb(60, 110, 200)
    BP $g (PT @((11, 8), (21, 8), (21, 14), (16, 23), (11, 14))) $C::FromArgb(115, 165, 235)
    DL $g 8 6 24 6 $C::FromArgb(200, 212, 230) 1
    DL $g 8 15 16 26 $C::FromArgb(200, 212, 230) 1
    DL $g 24 15 16 26 $C::FromArgb(200, 212, 230) 1
    BR $g 15 10 2 8 $Gold
    BR $g 13 13 6 2 $Gold
}

function Draw-ItemShield($g) {
    Draw-Floor $g
    Draw-ShieldShape $g
}

function Draw-IconDef($g) {
    Draw-ShieldShape $g
}

function Draw-KeyShape($g, $color) {
    BE $g 6 5 10 10 $color
    BE $g 8 7 6 6 $C::FromArgb(216, 207, 192)
    BR $g 13 11 13 3 $color
    BR $g 24 14 3 3 $color
    BR $g 21 16 3 3 $color
    DL $g 7 7 8 6 $White 1
}

function Draw-ItemKey($g, $color) {
    Draw-Floor $g
    Draw-KeyShape $g $color
}

function Draw-IconKey($g, $color) {
    Draw-KeyShape $g $color
}

function Draw-Player($g) {
    BE $g 8 27 16 3 $FloorDark
    BR $g 11 24 4 4 $C::FromArgb(50, 70, 120)
    BR $g 17 24 4 4 $C::FromArgb(50, 70, 120)
    BR $g 10 27 5 2 $C::FromArgb(40, 40, 46)
    BR $g 18 27 5 2 $C::FromArgb(40, 40, 46)
    BR $g 9 16 14 9 $C::FromArgb(70, 130, 215)
    BR $g 9 22 14 2 $C::FromArgb(150, 110, 60)
    BR $g 7 17 4 7 $C::FromArgb(120, 90, 60)
    BE $g 10 6 12 12 $Skin
    BE $g 10 5 12 5 $C::FromArgb(50, 70, 120)
    BE $g 13 10 2 2 $Black
    BE $g 18 10 2 2 $Black
    BR $g 21 17 4 5 $C::FromArgb(70, 130, 215)
    $old = $g.Transform
    $g.TranslateTransform(25, 15)
    $g.RotateTransform(35)
    BP $g (PT @((-2, -8), (1, -8), (0, -12))) $C::FromArgb(215, 219, 226)
    BR $g -2 -8 3 12 $C::FromArgb(215, 219, 226)
    BR $g -4 -8 8 1 $Gold
    $g.Transform = $old
}

function Draw-Npc($g) {
    BE $g 8 27 16 3 $FloorDark
    BP $g (PT @((16, 15), (6, 28), (26, 28))) $C::FromArgb(140, 70, 190)
    DL $g 7 27 25 27 $C::FromArgb(90, 40, 120) 1
    BE $g 11 6 10 10 $Skin
    BE $g 12 13 8 4 $White
    BP $g (PT @((10, 7), (22, 7), (16, 1))) $C::FromArgb(120, 40, 160)
    BE $g 13 9 2 2 $Black
    BE $g 18 9 2 2 $Black
    BR $g 8 17 3 8 $C::FromArgb(140, 70, 190)
    BR $g 21 17 3 8 $C::FromArgb(140, 70, 190)
}

function Draw-Shop($g) {
    Draw-Floor $g
    for ($i = 0; $i -lt 6; $i++) {
        $c = if ($i % 2 -eq 0) { $C::FromArgb(185, 45, 45) } else { $C::FromArgb(240, 235, 225) }
        BR $g (2 + $i * 5) 3 5 10 $c
    }
    BR $g 2 12 30 2 $WoodDark
    BR $g 3 20 26 10 $WoodDark
    BR $g 3 20 26 2 $WoodLight
    BE $g 10 16 6 6 $Gold
    BE $g 15 17 6 6 $Gold
    BE $g 20 15 6 6 $Gold
    BE $g 12 15 6 6 $GoldDark
}

function Draw-Coin($g) {
    BE $g 5 7 22 18 $C::FromArgb(216, 166, 30)
    BE $g 5 7 22 18 $C::FromArgb(216, 166, 30)
    BE $g 8 9 16 14 $Gold
    BR $g 15 11 2 10 $GoldDark
    BR $g 12 15 8 2 $GoldDark
    BE $g 9 10 5 3 $White
}

function Draw-IconFloor($g) {
    BE $g 8 27 16 3 $FloorDark
    BP $g (PT @((16, 3), (4, 10), (28, 10))) $C::FromArgb(70, 130, 215)
    BR $g 9 10 14 12 $C::FromArgb(205, 190, 165)
    DL $g 9 10 23 10 $C::FromArgb(120, 110, 95) 1
    BR $g 13 17 6 5 $WoodDark
}

# ---------- 生成 ----------
New-Sprite 'floor.png' { param($g) Draw-Floor $g }
New-Sprite 'wall.png' { param($g) Draw-Wall $g }
New-Sprite 'stairup.png' { param($g) Draw-StairUp $g }
New-Sprite 'stairdown.png' { param($g) Draw-StairDown $g }
New-Sprite 'door_red.png' { param($g) Draw-Door $g $C::FromArgb(206, 46, 58) }
New-Sprite 'door_blue.png' { param($g) Draw-Door $g $C::FromArgb(58, 112, 232) }
New-Sprite 'door_yellow.png' { param($g) Draw-Door $g $C::FromArgb(232, 172, 34) }
New-Sprite 'monster_0.png' { param($g) Draw-Monster $g $C::FromArgb(92, 132, 62) $C::FromArgb(122, 168, 92) $null }
New-Sprite 'monster_1.png' { param($g) Draw-Monster $g $C::FromArgb(212, 120, 42) $C::FromArgb(238, 168, 94) $null }
New-Sprite 'monster_2.png' { param($g) Draw-Monster $g $C::FromArgb(152, 62, 162) $C::FromArgb(182, 112, 192) $null }
New-Sprite 'monster_3.png' { param($g) Draw-Monster $g $C::FromArgb(194, 42, 54) $C::FromArgb(218, 92, 104) $C::FromArgb(234, 204, 62) }
New-Sprite 'item_hp.png' { param($g) Draw-ItemHeart $g $C::FromArgb(212, 52, 72) }
New-Sprite 'item_attack.png' { param($g) Draw-ItemSword $g }
New-Sprite 'item_defense.png' { param($g) Draw-ItemShield $g }
New-Sprite 'key_red.png' { param($g) Draw-ItemKey $g $C::FromArgb(212, 52, 62) }
New-Sprite 'key_blue.png' { param($g) Draw-ItemKey $g $C::FromArgb(62, 112, 232) }
New-Sprite 'key_yellow.png' { param($g) Draw-ItemKey $g $C::FromArgb(232, 172, 34) }
New-Sprite 'player.png' { param($g) Draw-Player $g }
New-Sprite 'npc.png' { param($g) Draw-Npc $g }
New-Sprite 'shop.png' { param($g) Draw-Shop $g }
New-Sprite 'icon_hp.png' { param($g) Draw-IconHp $g }
New-Sprite 'icon_atk.png' { param($g) Draw-IconAtk $g }
New-Sprite 'icon_def.png' { param($g) Draw-IconDef $g }
New-Sprite 'icon_key_red.png' { param($g) Draw-IconKey $g $C::FromArgb(212, 52, 62) }
New-Sprite 'icon_key_blue.png' { param($g) Draw-IconKey $g $C::FromArgb(62, 112, 232) }
New-Sprite 'icon_key_yellow.png' { param($g) Draw-IconKey $g $C::FromArgb(232, 172, 34) }
New-Sprite 'icon_gold.png' { param($g) Draw-Coin $g }
New-Sprite 'icon_floor.png' { param($g) Draw-IconFloor $g }
Write-Host "全部贴图生成完毕 -> $OutDir"