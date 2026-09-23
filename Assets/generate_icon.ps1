# 生成魔塔主题多尺寸图标 Assets\icon.ico（16/24/32/48/64/128/256，PNG 内嵌，Vista+ 兼容）
# 用法：pwsh -File Assets\generate_icon.ps1
Add-Type -AssemblyName System.Drawing

$out = Join-Path $PSScriptRoot "icon.ico"
$sizes = 16, 24, 32, 48, 64, 128, 256

function Add-RoundRect([System.Drawing.Drawing2D.GraphicsPath]$path, [single]$x, [single]$y, [single]$w, [single]$h, [single]$r) {
    $d = $r * 2
    $path.AddArc($x, $y, $d, $d, 180, 90)
    $path.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $path.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $path.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $path.CloseFigure()
}

function New-StarPath([single]$cx, [single]$cy, [single]$outer, [single]$inner) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $pts = New-Object 'System.Drawing.PointF[]' 10
    for ($i = 0; $i -lt 10; $i++) {
        $rad = if ($i % 2 -eq 0) { $outer } else { $inner }
        $a = (-90 + $i * 36) * [math]::PI / 180.0
        $pts[$i] = New-Object System.Drawing.PointF (
            [single]($cx + $rad * [math]::Cos($a)), [single]($cy + $rad * [math]::Sin($a)))
    }
    $p.AddPolygon($pts)
    $p.CloseFigure()
    return $p
}

function New-MotaIcon([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $u = $size / 256.0

    # 深蓝圆角背景（夜晚魔塔天空）
    $bg = New-Object System.Drawing.Drawing2D.GraphicsPath
    Add-RoundRect $bg (10 * $u) (10 * $u) (236 * $u) (236 * $u) (52 * $u)
    $bgBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        (New-Object System.Drawing.Point(0, 0)), (New-Object System.Drawing.Point(0, $size)),
        ([System.Drawing.Color]::FromArgb(255, 14, 30, 62)),
        ([System.Drawing.Color]::FromArgb(255, 66, 116, 182)))
    $g.FillPath($bgBrush, $bg)

    # 月 + 星
    $moonBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(215, 247, 224, 139))
    $g.FillEllipse($moonBrush, (178 * $u), (58 * $u), (54 * $u), (54 * $u))
    $star = New-StarPath (74 * $u) (66 * $u) (24 * $u) (10 * $u)
    $starBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(235, 255, 243, 196))
    $g.FillPath($starBrush, $star)

    # 金色渐变（塔体）
    $goldRect = [System.Drawing.RectangleF]::new((70 * $u), (52 * $u), (120 * $u), (160 * $u))
    $gold = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        $goldRect,
        ([System.Drawing.Color]::FromArgb(255, 255, 224, 138)),
        ([System.Drawing.Color]::FromArgb(255, 218, 148, 54)),
        [single]90.0)
    $outline = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255, 92, 66, 20), (3 * $u))
    $outline.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round

    # 屋顶
    $roofPts = New-Object 'System.Drawing.PointF[]' 3
    $roofPts[0] = New-Object System.Drawing.PointF ((128 * $u), (52 * $u))
    $roofPts[1] = New-Object System.Drawing.PointF ((70 * $u), (120 * $u))
    $roofPts[2] = New-Object System.Drawing.PointF ((186 * $u), (120 * $u))
    $g.FillPolygon($gold, $roofPts)
    $g.DrawPolygon($outline, $roofPts)

    # 塔身 + 塔基
    $g.FillRectangle($gold, (78 * $u), (118 * $u), (100 * $u), (80 * $u))
    $g.DrawRectangle($outline, (78 * $u), (118 * $u), (100 * $u), (80 * $u))
    $g.FillRectangle($gold, (70 * $u), (194 * $u), (116 * $u), (18 * $u))
    $g.DrawRectangle($outline, (70 * $u), (194 * $u), (116 * $u), (18 * $u))

    # 圆拱门 + 双窗（镂空夜空色）
    $door = New-Object System.Drawing.Drawing2D.GraphicsPath
    $door.AddArc((108 * $u), (146 * $u), (40 * $u), (40 * $u), 180, 180)
    $door.AddLine((108 * $u), (186 * $u), (108 * $u), (206 * $u))
    $door.AddLine((108 * $u), (206 * $u), (148 * $u), (206 * $u))
    $door.AddLine((148 * $u), (206 * $u), (148 * $u), (186 * $u))
    $door.CloseFigure()
    $winBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 16, 38, 74))
    $g.FillPath($winBrush, $door)
    $g.FillRectangle($winBrush, (92 * $u), (136 * $u), (16 * $u), (24 * $u))
    $g.FillRectangle($winBrush, (148 * $u), (136 * $u), (16 * $u), (24 * $u))

    # 屋顶球 + 红旗
    $ballBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 245, 197, 24))
    $g.FillEllipse($ballBrush, (121 * $u), (38 * $u), (14 * $u), (14 * $u))
    $flagBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 232, 86, 77))
    $flagPts = New-Object 'System.Drawing.PointF[]' 3
    $flagPts[0] = New-Object System.Drawing.PointF ((135 * $u), (44 * $u))
    $flagPts[1] = New-Object System.Drawing.PointF ((135 * $u), (26 * $u))
    $flagPts[2] = New-Object System.Drawing.PointF ((160 * $u), (35 * $u))
    $g.FillPolygon($flagBrush, $flagPts)

    $g.Dispose()
    return $bmp
}

# ---- 组装多尺寸 ICO（PNG 条目） ----
$pngs = @()
foreach ($sz in $sizes) {
    $bmp = New-MotaIcon $sz
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    $pngs += , $ms.ToArray()
}

$count = $sizes.Count
$stream = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter($stream)
$bw.Write([uint16]0); $bw.Write([uint16]1); $bw.Write([uint16]$count)
$offset = 6 + 16 * $count
for ($i = 0; $i -lt $count; $i++) {
    $sz = $sizes[$i]
    $dim = if ($sz -ge 256) { 0 } else { $sz }
    $bw.Write([byte]$dim); $bw.Write([byte]$dim)
    $bw.Write([byte]0); $bw.Write([byte]0)
    $bw.Write([uint16]1); $bw.Write([uint16]32)
    $bw.Write([uint32]$pngs[$i].Length); $bw.Write([uint32]$offset)
    $offset += $pngs[$i].Length
}
foreach ($p in $pngs) { $bw.Write($p) }
$bw.Flush()
[System.IO.File]::WriteAllBytes($out, $stream.ToArray())
$bw.Dispose()

Write-Host ("Generated: " + $out + "  (" + $count + " sizes, " + [math]::Round((Get-Item $out).Length / 1KB, 1) + " KB)")
