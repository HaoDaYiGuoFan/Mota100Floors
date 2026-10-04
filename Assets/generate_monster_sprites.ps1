# 生成原版魔塔风格的怪物 / 道具像素贴图（48×48 PNG = 16×16 像素画 ×3）：
#   Assets/Tiles/monster_1..60.png   —— 对应 Monsters.json 模板 ID，每只怪物独立贴图
#   （item_1..12.png 已移至 generate_item_sprites.ps1 生成，本脚本只管怪物，避免覆盖物品贴图）
# 字符画图例：'.'=透明 'o'=描边 'a'=主体 'b'=主体暗部 'c'=次级色(羽饰/护甲饰) 'e'=眼白 'p'=瞳孔
#             'm'=嘴 'w'=金属 'h'=木柄 'g'=金色 'r'=红点缀 'l'=高光
# 用法：powershell -ExecutionPolicy Bypass -File Assets/generate_monster_sprites.ps1
$ErrorActionPreference = 'Stop'
$OutDir = Join-Path $PSScriptRoot 'Tiles'
Add-Type -AssemblyName System.Drawing

# ---------------- 形状库（16×16；'M' 标记 = 左半 8 列自动镜像） ----------------
$shapes = @{}

function Add-Shape([string]$name, [string[]]$rows, [bool]$mirror = $false) {
    $fixed = foreach ($r in $rows) {
        $s = ($r -replace ' ', '.')
        if ($s.Length -gt 16) { $s = $s.Substring(0, 16) }
        elseif ($s.Length -lt 16) { $s = $s.PadRight(16, '.') }
        $s
    }
    if ($mirror) {
        $full = foreach ($r in $fixed) {
            if ($r.Length -ne 16) { throw "镜像形状 $name 需 8 列半行（补齐后为 16 说明传了整行）：'$r'" }
            $half = $r.Substring(0, 8)
            $rev = -join $half[($half.Length - 1)..0]
            $half + $rev
        }
        $shapes[$name] = @($full)
    }
    else {
        $shapes[$name] = @($fixed)
    }
}

# ---- 史莱姆（对称）：经典圆顶大眼 ----
Add-Shape 'SLIME' @(
    '........'
    '........'
    '.....ooo'
    '...ooaaa'
    '..oaaaaa'
    '.oaaaaaa'
    '.oaaeaaa'
    '.oaaapaa'
    '.oaaaaaa'
    '.oaaaaaa'
    '..oaamma'
    '..obbbbb'
    '...ooobb'
    '.....ooo'
    '........'
    '........'
) $true

# ---- 大史莱姆（对称）：更饱满 + 獠牙嘴 ----
Add-Shape 'SLIME_BIG' @(
    '........'
    '....oooo'
    '..ooaaaa'
    '.oaaaaaa'
    '.oaaaaaa'
    'oaaaeaaa'
    'oaaaeaaa'
    'oaaapaaa'
    'oaaaaaaa'
    'oaaamama'
    'oaaaaaaa'
    'oobbbbbb'
    '.obbbbbb'
    '..oooooo'
    '........'
    '........'
) $true

# ---- 蝙蝠（对称）：展翼獠牙 ----
Add-Shape 'BAT' @(
    '........'
    '......o.'
    '.....oo.'
    'o....oo.'
    'oo..ooa.'
    'oooooar.'
    'oooooaaa'
    '.ooo.oaa'
    '..o..oaa'
    '.....oam'
    '......om'
    '.......o'
    '........'
    '........'
    '........'
    '........'
) $true

# ---- 大蝙蝠（对称）：更大更凶 ----
Add-Shape 'BAT_BIG' @(
    '........'
    '......o.'
    '.....oo.'
    'o....oo.'
    'oo..ooa.'
    'oooooar.'
    'oooooaaa'
    'oooo.oam'
    '.oo..oam'
    '.....oaa'
    '......oa'
    '.......o'
    '........'
    '........'
    '........'
    '........'
) $true

# ---- 骷髅（右手持剑） ----
Add-Shape 'SKEL_SWORD' @(
    '.....oooo....o..'
    '....oaaaaoo.owo.'
    '....oapapa.owo..'
    '....oaaaaa.owo..'
    '.....oaam..owo..'
    '......o...owoo..'
    '..oooooo..owo...'
    '.oaaaaaao.owo...'
    '.o.aa.aaooowo...'
    '.o.aa.aa.owo....'
    '....aa...owo....'
    '...aa..oowoo....'
    '..aa...owoo.....'
    '.aa......oo.....'
    '.aa.............'
    '..o.............'
) $false

# ---- 骷髅士兵（左手圆盾 + 头盔） ----
Add-Shape 'SKEL_SHIELD' @(
    '....oooo'
    '...ocaaaoo'
    '...ocapapa'
    '....oaaaaa'
    '.....oaam'
    '..oooao'
    '.occwoooo'
    'ocwwwoaaaoo'
    'ocwwoaaaaao'
    'occwoaa.aao'
    '.ooooaa..aa'
    '...oaaa.aaa'
    '....aa...aa'
    '....aa...aa'
    '...oo...oo'
    '........'
) $false

# ---- 骷髅队长（角盔 + 战斧） ----
Add-Shape 'SKEL_AXE' @(
    '.r..........o...'
    '.rr....oooo.oo..'
    '..oo..oaaaaohw..'
    '..oao.oapapahw..'
    '...o..oaaaaohw..'
    '...o...oaam.hw..'
    '..oo.....o..hw..'
    '.ocoooo...o.hw..'
    'occaaaao..ooow..'
    '.ocaaaaao..hw...'
    '..oaaaaaa..hw...'
    '...oaaaa...hw...'
    '...aa.aa..hw....'
    '..aa...aa.oo....'
    '..aa...aa.......'
    '..oo...oo.......'
) $false

# ---- 骷髅王（金冠 + 披风 + 大剑） ----
Add-Shape 'SKEL_CROWN' @(
    '..g..g..g.......'
    '..gg.gg.gg..o.o.'
    '...ooooooo.owoo.'
    '..oaaaaaaa.owo..'
    '..oapapaaa.owo..'
    '..oaaaaaaa.owo..'
    '...oammma..owo..'
    '..rraoaao..owo..'
    '.rrraaaaaaoowo..'
    '.rraa.aaaaowo...'
    '.rra...aaaowo...'
    '.rr.....oowoo...'
    '.rr....owoo.....'
    '.rr.....oo......'
    '.oo.............'
    '................'
) $false

# ---- 冥灵（幽灵状骷髅，对称） ----
Add-Shape 'GHOST' @(
    '........'
    '...oooo.'
    '..oaaaa.'
    '..oapa.a'
    '..oaaaa.'
    '...oaam.'
    '..oaaaa.'
    '.oaaaaaa'
    '.oaaaaaa'
    '.oaaaaaa'
    'oaaaaaaa'
    'oaa.aaaa'
    'oo..oaa.'
    '.....oo.'
    '........'
    '........'
) $true

# ---- 法师（尖帽长袍 + 法杖，杖顶宝珠） ----
Add-Shape 'WIZ' @(
    '......o.....ww..'
    '......oo....ww..'
    '.....oooo...ww..'
    '....oooooo..ww..'
    '..ooooooooo.hw..'
    '....oaeaea..hw..'
    '....oaaaaa..hw..'
    '...oaaaaaa..hw..'
    '...oaaaaaa.ohwo.'
    '..oaaaaaaa..hw..'
    '..oaaaaaaa..hw..'
    '..oaaaaaaaa.hw..'
    '.oaaaaaaaaa.hw..'
    '.oaabbbbbbb.hw..'
    '.obbbbbbbbb.hw..'
    '..ooooooooooo...'
) $false

# ---- 卫兵（盔甲长矛羽饰；c=羽饰，右手持矛） ----
Add-Shape 'GUARD' @(
    '..........w.....'
    '....oooo..w.....'
    '...ocaaao.w.....'
    '...ocaaao.w.....'
    '....oaaao.w.....'
    '....oaeae.w.....'
    '....oaaaa.w.....'
    '...oaaaaa.w.....'
    '..oaaaaaaow.....'
    '..oaaaaaa.w.....'
    '..oaaoccaa......'
    '..oaaaaaaa......'
    '...oabbbba......'
    '...obbbbbb......'
    '....oooooo......'
    '................'
) $false

# ---- 大卫士（BOSS 级魁梧卫兵，持戟） ----
Add-Shape 'GUARD_BIG' @(
    '..........w.....'
    '...oooo...w.....'
    '..ocaaaao.w.....'
    '..ocapapo.w.....'
    '...oaaaao.w.....'
    '..oaaaaaaow.....'
    '.oaaaaaaao.w....'
    'oaaaaaaaao.w....'
    'oaaaacaaaow.....'
    'oaaagcaaa.w.....'
    'oaaaacaaa.w.....'
    'oaaaaaaa..w.....'
    'oaabbbbb..w.....'
    '.obbbbbb..w.....'
    '..oooooo..w.....'
    '..........o.....'
) $false

# ---- 兽人（绿皮獠牙 + 战斧） ----
Add-Shape 'ORC' @(
    '..........ohw..'
    '..oooo....ohw..'
    '.oaaaaoo..ohw..'
    '.oapapao..ohwoo'
    '.oaaaaao..ohww.'
    '.oamamao..ohw..'
    '..ooooa...ohw..'
    '...ooooa..ohw..'
    '..oaaaaao.ohw..'
    '.oaaaaaaaooow...'
    '.oaa.aaaaa.hw...'
    '.oaa.aabba.hw...'
    '.oa..aabba.hw...'
    '.oa..aabba.oo...'
    '..o...ooo.......'
    '................'
) $false

# ---- 食人魔（魁梧大棒） ----
Add-Shape 'OGRE' @(
    '..........ohw..'
    '..oooo....ohw..'
    '.oaaaao...ohw..'
    '.oapapo...ohwoo'
    '.oaaaaao..ohww.'
    '.oamamoo..ohw..'
    '..ooooo...ohw..'
    '.oaaaaaooo.ow..'
    'oaaaaaaaao.oo..'
    'oaaa.aaaao.hw..'
    'oaaa.aabbb.hw..'
    'oa..aaabbbb.hw.'
    'oa..aabbbb..oo.'
    '.o..aabbbb.....'
    '....oobbbb.....'
    '.....oooo......'
) $false

# ---- 武士（持剑重甲） ----
Add-Shape 'KNIGHT' @(
    '.....oooo...oo..'
    '....ocaaaa.owo..'
    '....ocapap.owo..'
    '.....oaaaa.owo..'
    '......oaam.owo..'
    '...ooooao..owoo.'
    '..ocaaaaao.owo..'
    '.ocaaaaaaaaowo..'
    '.oc.aaaaaa.owo..'
    '.oc.aaaaa..owo..'
    '....aaaa...owo..'
    '...aaa....owoo..'
    '..aaa....owoo...'
    '..aa......oo....'
    '..aa............'
    '..oo............'
) $false

# ---- 大剑士（双手巨剑居中） ----
Add-Shape 'KNIGHT_GREAT' @(
    '.....oooo.......'
    '....ocaaaa......'
    '....ocapap......'
    '.....oaaaa......'
    '......oaam......'
    '...ooooao.......'
    '..ocaaaao..ww...'
    '.ocaaaaao..ww...'
    '.ocaaaaoaowww...'
    '.ocaaaowaoww....'
    '...aaaowaoww....'
    '...aa.owahww....'
    '..aa..oahww.....'
    '..aa...hhh......'
    '..aa...hhh......'
    '..oo....oo......'
) $false

# ---- 僵尸（对称，双臂前伸） ----
Add-Shape 'ZOMBIE' @(
    '........'
    '...oooo.'
    '..oaaaa.'
    '..oaeee.'
    '..oapa.a'
    '..oaaaa.'
    '...oaam.'
    '.oaaoaaa'
    'oaaaaaaa'
    'oaaaaaaa'
    'oaaaoaaa'
    '.oaaobb.'
    '..oaobb.'
    '...oobb.'
    '....ooo.'
    '........'
) $true

# ---- 大乌鸦（对称） ----
Add-Shape 'CROW' @(
    '........'
    '........'
    '......o.'
    '.....ooo'
    '.o...ooa'
    '.oo..oaa'
    '..oooaar'
    '..ooooaa'
    '...ooaar'
    '....ooam'
    '.....oam'
    '......oo'
    '.......o'
    '........'
    '........'
    '........'
) $true

# ---- 石头人（对称，岩石躯干发光眼） ----
Add-Shape 'GOLEM' @(
    '........'
    '...oooo.'
    '..oaaaa.'
    '..oaeae.'
    '..oaaaa.'
    '.ooaaaa.'
    '.oaaaaaa'
    'oaaabbaa'
    'oaaabbaa'
    'oaaabbaa'
    'oaaaaaaa'
    'oaabbaaa'
    '.oabbbba'
    '.oaaaaaa'
    '..oooooo'
    '........'
) $true

# ---- 恶魔（对称，双角） ----
Add-Shape 'DEMON' @(
    '.o......'
    '.ro.....'
    '..ro.oo.'
    '..ooraa.'
    '...oaaaa'
    '...oaraa'
    '...oaaaa'
    '..ooaaaa'
    '.oaaaaaa'
    '.oaaaaaa'
    'oaaaaaaa'
    'oaaaaaaa'
    'oaa.aaaa'
    '.o.obbbo'
    '....oooo'
    '........'
) $true

# ---- 魔王（BOSS 级恶魔，双角+披风，对称） ----
Add-Shape 'DEMON_KING' @(
    '.o......'
    '.go.....'
    '..go.oo.'
    '..ooara.'
    '...oaaaa'
    '...oapa.'
    '...oaaaa'
    '..ooaam.'
    '.oaaaaaa'
    '.oaaaaaa'
    'oaaaaaaa'
    'oaaaaaaa'
    'oaa.aaaa'
    'oa..abba'
    'oo..obbo'
    '.....ooo'
) $true

# ---- 巨龙（侧视，右向，弓形巨翼） ----
Add-Shape 'DRAGON' @(
    '..........oo....'
    '.o........oaao..'
    '.oo......oaaaap.'
    '.ooo....oaraaap.'
    '..oooo..oaaaaaa.'
    '..ooooooaaaaaam.'
    '.oaaaaaaaaaaaa..'
    'oaaaaaaaaaaaa...'
    'oaaaaaaaaaaa....'
    '.oaaaaaaaaaa....'
    '..oaaaaaaaaa....'
    '..oaaa..aaaa....'
    '.oaaa....aaa....'
    '.oaa......oao...'
    '.oo........o....'
    '................'
) $false

# ---- 骨龙（骷髅巨龙，右向） ----
Add-Shape 'DRAGON_BONE' @(
    '..........oo....'
    '.o........oaao..'
    '.oo......oaaaap.'
    '.ooo....oaraaap.'
    '..oooo..oaeaaaa.'
    '..ooooooaaaaaam.'
    '.oaaaaaaaaaaaa..'
    'oaaaaaaaaaaaa...'
    'oaaaaaaaaaaa....'
    '.oaaaaaaaaaa....'
    '..oaaaaaaaaa....'
    '..oaaa..aaaa....'
    '.oaaa....aaa....'
    '.oaa......oao...'
    '.oo........o....'
    '................'
) $false

# ---- 红药水（小瓶，对称） ----
Add-Shape 'FLASK' @(
    '........'
    '........'
    '.....ooo'
    '.....oho'
    '.....ooo'
    '....oaao'
    '...oaaaa'
    '..oaaaaa'
    '..oallaa'
    '..oaaaaa'
    '..oaaaaa'
    '...oaaaa'
    '....oooo'
    '........'
    '........'
    '........'
) $true

# ---- 大血瓶（大瓶，对称） ----
Add-Shape 'FLASK_BIG' @(
    '........'
    '.....ooo'
    '.....oho'
    '.....ooo'
    '....oaao'
    '...oaaaa'
    '..oaaaaa'
    '.oaaaaaa'
    '.oallaaa'
    '.oalaaaa'
    '.oaaaaaa'
    '.oaaaaaa'
    '..oaaaaa'
    '...ooooo'
    '........'
    '........'
) $true

# ---- 宝石（对称菱形） ----
Add-Shape 'GEM' @(
    '........'
    '........'
    '...oooo.'
    '..olaaaa'
    '.olaaaaa'
    '.olaaaaa'
    'oalaaaaa'
    'oaaaaaaa'
    'oaaaaaab'
    '.oaaaaab'
    '.oaaaabb'
    '..oaaabb'
    '...ooobb'
    '....oooo'
    '........'
    '........'
) $true

# ---------------- 调色板 ----------------
function Palette([string]$a, [string]$b, [string]$c,
    [string]$outline = '#FF1A1A1A', [string]$eye = '#FFFFFFFF', [string]$pupil = '#FF101018',
    [string]$mouth = '#FF7A1F1F', [string]$metal = '#FFB8BEC9', [string]$wood = '#FF7A4A22',
    [string]$gold = '#FFD9A420', [string]$red = '#FFD03830', [string]$light = '#FFFFFFFF') {
    @{
        'o' = $outline; 'a' = $a; 'b' = $b; 'c' = $c; 'e' = $eye; 'p' = $pupil
        'm' = $mouth; 'w' = $metal; 'h' = $wood; 'g' = $gold; 'r' = $red; 'l' = $light
    }
}

$sprites = @{
    # ===== 原版经典怪（1~23） =====
    1  = @('SLIME', (Palette '#3FA93F' '#256B25' '#2E852E'))                                      # 绿色史莱姆
    2  = @('SLIME', (Palette '#D84545' '#8E2323' '#B93333'))                                      # 红色史莱姆
    3  = @('BAT', (Palette '#7A55B5' '#4C3178' '#5D3F94'))                                        # 小蝙蝠
    4  = @('SLIME', (Palette '#4A4A5C' '#262632' '#363644' '#FF101014' '#FFE8E8E8'))              # 黑色史莱姆
    5  = @('WIZ', (Palette '#4A6FD4' '#2A4390' '#6C8AE8'))                                        # 初级法师
    6  = @('SKEL_SWORD', (Palette '#F4F2E6' '#8E8876' '#CFC9B5'))                                 # 骷髅人
    7  = @('BAT_BIG', (Palette '#5A3E86' '#382456' '#48306E'))                                    # 大蝙蝠
    8  = @('SLIME_BIG', (Palette '#5FBF5F' '#337A33' '#46A046'))                                  # 大史莱姆
    9  = @('SKEL_SHIELD', (Palette '#F4F2E6' '#8E8876' '#9AA4B2'))                                # 骷髅士兵（银盔盾）
    10 = @('ORC', (Palette '#5C9E3A' '#39641F' '#4A8030'))                                        # 兽人
    11 = @('GUARD', (Palette '#96A4B8' '#4E5868' '#4A6FD4'))                                      # 初级卫兵（银甲蓝羽）
    12 = @('BAT_BIG', (Palette '#B54444' '#7A2626' '#963434'))                                    # 红蝙蝠
    13 = @('SKEL_AXE', (Palette '#F4F2E6' '#8E8876' '#8A6A3A'))                                   # 骷髅队长
    14 = @('WIZ', (Palette '#B0AA96' '#6E6A5C' '#B0AB9B'))                                        # 麻衣法师
    15 = @('WIZ', (Palette '#8A55C5' '#5A3185' '#6F42A5'))                                        # 高级法师
    16 = @('GUARD', (Palette '#7E8A9C' '#454C58' '#C03030'))                                      # 中级卫兵（钢甲红羽）
    17 = @('CROW', (Palette '#3A3A44' '#1E1E26' '#2C2C36' '#FF101014' '#FFE8D040' '#FF101018' '#FFD07828')) # 大乌鸦（黄瞳）
    18 = @('GOLEM', (Palette '#8E9298' '#5A5E66' '#74787E' '#FF141418' '#FF70D8F0'))              # 石头人（冰蓝发光眼）
    19 = @('KNIGHT', (Palette '#D6D4CC' '#84827A' '#C2C0B8'))                                     # 白衣武士
    20 = @('ORC', (Palette '#7A6238' '#4E3C1E' '#63502C'))                                        # 兽人武士
    21 = @('KNIGHT_GREAT', (Palette '#9AA2B0' '#5E6470' '#788090'))                               # 双手剑士
    22 = @('ZOMBIE', (Palette '#7A9E5A' '#4A6234' '#5F7F46' '#FF14181A' '#FFE8E0C0'))             # 僵尸
    23 = @('GUARD', (Palette '#D0B040' '#6E5818' '#C03030'))                                      # 高级卫兵（金甲红羽）

    # ===== 中段外推（24~33） =====
    24 = @('SKEL_SWORD', (Palette '#E4E0D0' '#767060' '#B4AC98' '#FF1A1A1A' '#FFD84040'))         # 骷髅勇士（红瞳）
    25 = @('BAT_BIG', (Palette '#8A2536' '#541220' '#6E1C2C' '#FF141418' '#FFD84040'))            # 血翼蝙蝠
    26 = @('GOLEM', (Palette '#9A8868' '#5E5138' '#7C6E50' '#FF141418' '#FF70D8F0'))              # 石头巨人
    27 = @('WIZ', (Palette '#3A3A4C' '#20202C' '#2C2C3A' '#FF101014' '#FFD84040'))                # 邪恶法师
    28 = @('OGRE', (Palette '#A87848' '#6E4A26' '#8C6238' '#FF141418' '#FFE8D040'))               # 食人魔
    29 = @('WIZ', (Palette '#E8E6F2' '#A8A6C0' '#C8C6DC' '#FF141418' '#FF4070D8'))                # 白袍法师
    30 = @('GUARD', (Palette '#96692E' '#5A4218' '#2E8B3A'))                                      # 青铜卫兵（绿羽）
    31 = @('KNIGHT_GREAT', (Palette '#C8CEDA' '#848A96' '#A8AEB8'))                               # 大剑士
    32 = @('WIZ', (Palette '#C03830' '#7A1E18' '#9C2A24'))                                        # 红衣法师
    33 = @('GUARD', (Palette '#AAB6C8' '#646C7C' '#C03030'))                                      # 银甲卫兵

    # ===== 高段外推（34~42） =====
    34 = @('KNIGHT', (Palette '#3C3C46' '#202028' '#2E2E38' '#FF101014' '#FFD84040'))             # 暗黑骑士
    35 = @('WIZ', (Palette '#E07828' '#93440E' '#BC5E1A'))                                        # 烈焰法师
    36 = @('GUARD', (Palette '#E2BE48' '#7A6016' '#C03030'))                                      # 黄金卫兵
    37 = @('GOLEM', (Palette '#A88A5A' '#6E5630' '#8C7244' '#FF141418' '#FF70D8F0'))              # 岩石巨人
    38 = @('SKEL_CROWN', (Palette '#F0EAD8' '#807A6A' '#C6BFB0' '#FF1A1A1A' '#FFD84040'))         # 骷髅王（金冠红瞳）
    39 = @('WIZ', (Palette '#68B8E0' '#33688A' '#4A90B8'))                                        # 冰霜法师
    40 = @('KNIGHT', (Palette '#4A4454' '#282234' '#3A3444' '#FF101014' '#FFD84040'))             # 魔王卫队
    41 = @('KNIGHT_GREAT', (Palette '#EEEAE0' '#B0AA9C' '#D2CCBE'))                               # 双手剑圣
    42 = @('KNIGHT', (Palette '#2C2C34' '#16161C' '#3C3C46' '#FF101014' '#FFD84040'))             # 黑骑士团长

    # ===== 终局外推（43~50） =====
    43 = @('GHOST', (Palette '#7AC8C0' '#3E847E' '#58A6A0' '#FF101014' '#FFE0F8F0'))              # 冥灵武士
    44 = @('WIZ', (Palette '#6A2FA0' '#3C165E' '#52227E' '#FF141418' '#FFE8D040'))                # 大法师
    45 = @('GUARD_BIG', (Palette '#D8D4C6' '#787262' '#C03030'))                                  # 圣殿卫士
    46 = @('GHOST', (Palette '#58A8C8' '#2C647E' '#3E86A4' '#FF101014' '#FFE0F8F0'))              # 冥灵队长
    47 = @('DRAGON_BONE', (Palette '#E8E2CE' '#A8A294' '#C6C0B2' '#FF1A1A1A' '#FFD84040'))        # 骨龙
    48 = @('WIZ', (Palette '#26262E' '#101014' '#1A1A22' '#FF08080C' '#FF88E8F8'))                # 灭世法师
    49 = @('DEMON', (Palette '#8A3038' '#521820' '#6E242C' '#FF101014' '#FFD84040'))              # 魔王亲卫
    50 = @('KNIGHT', (Palette '#5A2A6A' '#32163C' '#462054' '#FF101014' '#FFD84040'))             # 混沌魔将

    # ===== 里程碑 BOSS（51~60） =====
    51 = @('SKEL_CROWN', (Palette '#F4F2E6' '#8E8876' '#CFC9B5' '#FF1A1A1A' '#FFD84040'))         # 骷髅将军
    52 = @('OGRE', (Palette '#5C9E3A' '#39641F' '#4A8030' '#FF141418' '#FFD84040'))               # 兽人酋长
    53 = @('DEMON_KING', (Palette '#C03830' '#7A1E18' '#9C2A24' '#FF101014' '#FFD84040'))         # 魔王
    54 = @('GUARD_BIG', (Palette '#E2BE48' '#7A6016' '#C03030'))                                  # 黄金卫士
    55 = @('DRAGON', (Palette '#C03830' '#7A1E18' '#9C2A24' '#FF141418' '#FFE8D040'))             # 魔龙
    56 = @('DRAGON_BONE', (Palette '#F4F2E6' '#8E8876' '#CFC9B5' '#FF1A1A1A' '#FFD84040'))        # 骨龙王
    57 = @('DEMON_KING', (Palette '#E07828' '#93440E' '#BC5E1A' '#FF141418' '#FFE8D040'))         # 炎魔
    58 = @('DRAGON', (Palette '#3A3A46' '#20202A' '#2C2C38' '#FF0E0E12' '#FFD84040'))             # 暗黑魔龙
    59 = @('DEMON_KING', (Palette '#6A2FA0' '#3C165E' '#52227E' '#FF101014' '#FFE8D040'))         # 魔神
    60 = @('DEMON_KING', (Palette '#26262E' '#101014' '#1A1A22' '#FF08080C' '#FF88E8F8'))         # 冥灵魔王
}

# ---------------- 渲染 ----------------
function Convert-Color([string]$hex) {
    $hex = $hex.TrimStart("#")
    if ($hex.Length -eq 6) { $hex = "FF" + $hex } elseif ($hex.Length -ne 8) { throw "异常颜色值：[$hex] 长度 $($hex.Length)" }
    return [System.Drawing.Color]::FromArgb(
        [Convert]::ToInt32($hex.Substring(0, 2), 16),
        [Convert]::ToInt32($hex.Substring(2, 2), 16),
        [Convert]::ToInt32($hex.Substring(4, 2), 16),
        [Convert]::ToInt32($hex.Substring(6, 2), 16))
}

function Render([string]$file, [string]$shapeName, [hashtable]$palette) {
    $rows = $shapes[$shapeName]
    if ($null -eq $rows) { throw "未知形状：$shapeName" }
    $bmp = New-Object System.Drawing.Bitmap 48, 48
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    for ($y = 0; $y -lt 16; $y++) {
        for ($x = 0; $x -lt 16; $x++) {
            $ch = [string]$rows[$y][$x]
            if ($ch -eq '.') { continue }
            $hex = $palette[$ch]
            if (-not $hex) { throw "$file：字符 '$ch' 无配色（$shapeName 第 $y 行）" }
            $brush = New-Object System.Drawing.SolidBrush (Convert-Color $hex)
            $g.FillRectangle($brush, ($x * 3), ($y * 3), 3, 3)
            $brush.Dispose()
        }
    }
    $g.Dispose()
    $bmp.Save($file, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
}

$ok = 0
foreach ($k in ($sprites.Keys | Sort-Object)) {
    Render (Join-Path $OutDir "monster_$k.png") $sprites[$k][0] $sprites[$k][1]
    $ok++
}
Write-Host "已生成 $ok 张怪物贴图 → $OutDir"
