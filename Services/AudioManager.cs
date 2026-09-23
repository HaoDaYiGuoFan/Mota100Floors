using System;
using System.IO;
using System.Windows.Media;

namespace Mota100Floors.Services;

/// <summary>
/// 游戏音频管理：探索 BGM 与战斗 BGM（MediaPlayer 循环播放，支持静音切换）+ 拾取/战斗命中音效（一次性播放）。
/// 音频文件查找顺序（输出目录 Assets/Audio 下，MP3 优先于内置 WAV）：
///   bgm_explore.mp3 → bgm_explore.ogg → bgm_explore.wav
///   bgm_battle.mp3  → bgm_battle.ogg  → bgm_battle.wav
///   pickup.mp3      → pickup.ogg      → pickup.wav
///   battle_hit.mp3  → battle_hit.ogg  → battle_hit.wav
/// 内置 .wav 为仓库生成的原创 chiptune 占位曲；如你拥有正版音源，将文件
/// 命名为上述前缀放入 Assets/Audio 并重新构建即可直接替换（无需改代码）。
/// </summary>
public sealed class AudioManager : IDisposable
{
    private const string ExploreBase = "Assets/Audio/bgm_explore";
    private const string BattleBase = "Assets/Audio/bgm_battle";
    private const string PickupBase = "Assets/Audio/pickup";
    private const string HitBase = "Assets/Audio/battle_hit";
    private const string StepBase = "Assets/Audio/step";
    private const string DoorBase = "Assets/Audio/door_open";
    private const string VictoryBase = "Assets/Audio/victory";
    private const string DeathBase = "Assets/Audio/player_death";
    private const string FlyOrbPickupBase = "Assets/Audio/flyorb_pickup";
    private const string FlyOrbLaunchBase = "Assets/Audio/flyorb_launch";
    private const string FlyOrbArriveBase = "Assets/Audio/flyorb_arrive";
    private const double ExploreVolume = 0.30;
    private const double BattleVolume = 0.40;
    private const double PickupVolume = 0.70;
    private const double HitVolume = 0.85;
    private const double StepVolume = 0.55;
    private const double DoorVolume = 0.65;
    private const double VictoryVolume = 0.80;
    private const double DeathVolume = 0.80;
    private const double FlyOrbPickupVolume = 0.70;
    private const double FlyOrbLaunchVolume = 0.65;
    private const double FlyOrbArriveVolume = 0.60;

    private readonly MediaPlayer _explore = new();
    private readonly MediaPlayer _battle = new();

    /// <summary>一次性音效：可重复 Open+Play（每次播放从头开始）</summary>
    private readonly MediaPlayer _pickup = new();
    private readonly MediaPlayer _hit = new();
    private readonly MediaPlayer _step = new();
    private readonly MediaPlayer _door = new();
    private readonly MediaPlayer _victory = new();
    private readonly MediaPlayer _death = new();
    private readonly MediaPlayer _flyOrbPickup = new();
    private readonly MediaPlayer _flyOrbLaunch = new();
    private readonly MediaPlayer _flyOrbArrive = new();
    private bool _muted;

    public AudioManager()
    {
        _explore.MediaEnded += (_, _) => Restart(_explore);
        _battle.MediaEnded += (_, _) => Restart(_battle);
        _explore.MediaFailed += (_, _) => { }; // 音频不可用时静默降级
        _battle.MediaFailed += (_, _) => { };
        _pickup.MediaFailed += (_, _) => { };
        _hit.MediaFailed += (_, _) => { };
        _step.MediaFailed += (_, _) => { };
        _door.MediaFailed += (_, _) => { };
        _victory.MediaFailed += (_, _) => { };
        _death.MediaFailed += (_, _) => { };
        _flyOrbPickup.MediaFailed += (_, _) => { };
        _flyOrbLaunch.MediaFailed += (_, _) => { };
        _flyOrbArrive.MediaFailed += (_, _) => { };
        Open(_explore, ExploreBase);
        Open(_battle, BattleBase);
    }

    public bool IsMuted => _muted;

    /// <summary>切换静音，返回静音后的状态。静音时保留播放位置，恢复即回原声。</summary>
    public bool ToggleMute()
    {
        _muted = !_muted;
        ApplyVolume();
        return _muted;
    }

    public void PlayExplore()
    {
        _battle.Stop();
        _explore.Play();
        ApplyVolume();
    }

    public void PlayBattle()
    {
        _explore.Stop();
        _battle.Play();
        ApplyVolume();
    }

    public void StopAll()
    {
        _explore.Stop();
        _battle.Stop();
    }

    /// <summary>拾取道具音效（一次性，不打断 BGM）</summary>
    public void PlayPickup() => PlaySfx(_pickup, PickupBase, PickupVolume);

    /// <summary>战斗命中音效（战斗开始时播放一次，不打断 BGM）</summary>
    public void PlayHit() => PlaySfx(_hit, HitBase, HitVolume);

    /// <summary>玩家移动踩踏音效</summary>
    public void PlayStep() => PlaySfx(_step, StepBase, StepVolume);

    /// <summary>开门音效（黄/蓝/红门共用）</summary>
    public void PlayDoor() => PlaySfx(_door, DoorBase, DoorVolume);

    /// <summary>战斗胜利音效（击杀怪物/BOSS 后播放）</summary>
    public void PlayVictory() => PlaySfx(_victory, VictoryBase, VictoryVolume);

    /// <summary>玩家阵亡音效</summary>
    public void PlayDeath() => PlaySfx(_death, DeathBase, DeathVolume);

    /// <summary>拾取飞行器：短促 8bit「叮」正向获得音效</summary>
    public void PlayFlyOrbPickup() => PlaySfx(_flyOrbPickup, FlyOrbPickupBase, FlyOrbPickupVolume);

    /// <summary>飞行器启动 / 传送瞬间：短促嗡鸣 + 闪烁音效（约 0.8s）</summary>
    public void PlayFlyOrbLaunch() => PlaySfx(_flyOrbLaunch, FlyOrbLaunchBase, FlyOrbLaunchVolume);

    /// <summary>飞行器传送完成：清脆 8bit 落地音</summary>
    public void PlayFlyOrbArrive() => PlaySfx(_flyOrbArrive, FlyOrbArriveBase, FlyOrbArriveVolume);

    public void Dispose()
    {
        _explore.Stop();
        _battle.Stop();
        _pickup.Stop();
        _hit.Stop();
        _step.Stop();
        _door.Stop();
        _victory.Stop();
        _death.Stop();
        _flyOrbPickup.Stop();
        _flyOrbLaunch.Stop();
        _flyOrbArrive.Stop();
        _explore.Close();
        _battle.Close();
        _pickup.Close();
        _hit.Close();
        _step.Close();
        _door.Close();
        _victory.Close();
        _death.Close();
        _flyOrbPickup.Close();
        _flyOrbLaunch.Close();
        _flyOrbArrive.Close();
    }

    private void ApplyVolume()
    {
        _explore.Volume = _muted ? 0 : ExploreVolume;
        _battle.Volume = _muted ? 0 : BattleVolume;
    }

    private static void Restart(MediaPlayer p)
    {
        try
        {
            p.Position = TimeSpan.Zero;
            p.Play();
        }
        catch (InvalidOperationException)
        {
            // 尚未打开媒体时忽略
        }
    }

    private static void Open(MediaPlayer p, string basePath)
    {
        try
        {
            string root = AppContext.BaseDirectory;
            foreach (string ext in new[] { ".mp3", ".ogg", ".wav" })
            {
                string path = Path.Combine(root, basePath + ext);
                if (!File.Exists(path)) continue;
                p.Open(new Uri(path, UriKind.Absolute));
                return;
            }
        }
        catch
        {
            // 缺文件或无解码器时静默降级，不影响游戏进行
        }
    }

    /// <summary>一次性音效：重新 Open（若已打开则从头播放），受静音开关控制。</summary>
    private void PlaySfx(MediaPlayer p, string basePath, double volume)
    {
        try
        {
            string root = AppContext.BaseDirectory;
            foreach (string ext in new[] { ".mp3", ".ogg", ".wav" })
            {
                string path = Path.Combine(root, basePath + ext);
                if (!File.Exists(path)) continue;
                p.Volume = _muted ? 0 : volume;
                p.Stop();
                p.Open(new Uri(path, UriKind.Absolute));
                p.Play();
                return;
            }
        }
        catch
        {
            // 音效缺失或解码失败时静默跳过
        }
    }
}