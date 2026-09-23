namespace Mota100Floors.Services;

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Mota100Floors.Models;

/// <summary>
/// 存档磁盘存取：JSON 保存到 %LOCALAPPDATA%\Mota100Floors\save.json。
/// 选此路径的理由：Windows 标准用户数据目录，任意安装位置（含 Program Files）均可写；
/// 不随程序目录重建/覆盖而丢失，且按系统用户隔离。写入采用“临时文件 + Move 覆盖”，
/// 任一时刻磁盘上都有完整文件，中途崩溃不会损坏存档。
/// </summary>
public static class SaveSystem
{
    public static string SaveFilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Mota100Floors",
        "save.json");

    private static string TempPath => SaveFilePath + ".tmp";

    public static bool HasSave => File.Exists(SaveFilePath);

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>写入存档，成功返回 true（含目录创建与原子替换）</summary>
    public static bool Write(GameSaveData data)
    {
        try
        {
            string dir = Path.GetDirectoryName(SaveFilePath)!;
            Directory.CreateDirectory(dir);
            string json = JsonSerializer.Serialize(data, Options);
            File.WriteAllText(TempPath, json);
            File.Move(TempPath, SaveFilePath, overwrite: true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>读取存档；文件缺失或损坏（含版本不支持）返回 null</summary>
    public static GameSaveData? Read()
    {
        if (!HasSave) return null;

        try
        {
            var data = JsonSerializer.Deserialize<GameSaveData>(File.ReadAllText(SaveFilePath), Options);
            return data is { Player: not null }
                   && data.Version is >= 1 and <= GameSaveData.CurrentVersion
                ? data
                : null;
        }
        catch
        {
            return null;
        }
    }
}