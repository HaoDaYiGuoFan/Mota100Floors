namespace Mota100Floors.Models;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>玩家实体（可绑定，JSON 序列化）</summary>
public partial class Player : ObservableObject
{
    /// <summary>
    /// 生命硬上限。按通关模拟器校准：满清参考玩家最大单层战损约 5.5 千、
    /// 攻防投入减半的构筑约 3.7 万（99 层），10 万约为其 2.7 倍余量且全程战损链不断
    /// （参考玩家贴限通关）；同时把无上限规则下 38 万+ 的终局血量收敛到可读量级。
    /// 超出上限的药水/商店回血直接浪费，引导后期金币转向攻防投资。
    /// </summary>
    public const int MaxHpCap = 100_000;

    // 初始生命按前几层怪物校准：最优顺序全清第 1 层损血 288（约占 58%），第 2 层起药水净收入转正
    [ObservableProperty]
    private int _hp = 500;

    [ObservableProperty]
    private int _maxHp = 500;

    [ObservableProperty]
    private int _attack = 10;

    [ObservableProperty]
    private int _defense = 10;

    [ObservableProperty]
    private int _gold;

    [ObservableProperty]
    private int _redKey;

    [ObservableProperty]
    private int _blueKey;

    [ObservableProperty]
    private int _yellowKey = 1;

    [ObservableProperty]
    private int _currentFloor = 1;

    [ObservableProperty]
    private int _posX = 1;

    [ObservableProperty]
    private int _posY = 1;

    [ObservableProperty]
    private bool _isDead;

    /// <summary>是否已通关（100 层 BOSS 已击杀）</summary>
    public bool IsWin { get; set; }

    /// <summary>是否拥有飞行器（楼层穿梭机，永久持有道具，随档保存）</summary>
    public bool IsHasFlyOrb { get; set; }

    /// <summary>已访问过的楼层集合（1~100，飞行器穿梭列表数据源，随档保存；重置游戏时清空）</summary>
    public List<int> VisitedFloors { get; set; } = new();

    /// <summary>各怪物模板累计击杀数（TemplateId → 次数，用于任务统计，随档保存）</summary>
    public Dictionary<int, int> KillByTemplate { get; set; } = new();

    /// <summary>各道具模板累计拾取数（TemplateId → 次数，用于任务统计，随档保存）</summary>
    public Dictionary<int, int> PickByTemplate { get; set; } = new();
}
