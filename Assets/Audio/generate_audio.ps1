# 生成两段循环 BGM（WAV 16-bit / 44.1kHz 单声道）：
#   bgm_explore.wav —— 探索主题（C 大调 / 112 BPM / 8 小节 / ~17.14s 无缝循环，明快弹跳感）
#   bgm_battle.wav  —— 战斗主题（A 大调 / 150 BPM / 8 小节 / ~12.8s 无缝循环，热血向上）
# 用法：powershell -ExecutionPolicy Bypass -File Assets/Audio/generate_audio.ps1
$ErrorActionPreference = 'Stop'
$OutDir = $PSScriptRoot

Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class BgmGen
{
    const int SR = 44100;
    const int W_SQUARE = 0, W_PULSE = 1, W_TRI = 2;

    sealed class Note
    {
        public double Beat, Dur, Vol;
        public int Midi, Wave;
        public double Duty;
        public double Release;
        public Note(double beat, double dur, int midi, double vol, int wave, double duty = 0.5, double release = 0.012)
        {
            Beat = beat; Dur = dur; Midi = midi; Vol = vol; Wave = wave; Duty = duty; Release = release;
        }
    }

    static double Freq(int midi) => 440.0 * Math.Pow(2.0, (midi - 69) / 12.0);

    static double Osc(double t, double f, int wave, double duty)
    {
        double ph = f * t - Math.Floor(f * t);
        switch (wave)
        {
            case W_SQUARE: return Math.Sign(Math.Sin(2 * Math.PI * f * t));
            case W_PULSE:  return ph < duty ? 1.0 : -1.0;
            case W_TRI:    return 2 * Math.Abs(2 * ph - 1) - 1;
            default:       return 0;
        }
    }

    static void AddNote(List<Note> notes, double beat, double dur, int midi, double vol, int wave, double duty = 0.5, double release = 0.012)
        => notes.Add(new Note(beat, dur, midi, vol, wave, duty, release));

    static void Render(List<Note> notes, double[] buf, double spb)
    {
        foreach (var n in notes)
        {
            double f = Freq(n.Midi);
            int s0 = (int)(n.Beat * spb);
            int len = (int)(n.Dur * spb);
            if (len < 1) continue;
            int end = Math.Min(buf.Length, s0 + len);
            if (s0 >= end) continue;
            int atk = Math.Min((int)(0.003 * SR), len);
            int rel = (int)Math.Min(n.Release * SR, len);
            for (int i = s0; i < end; i++)
            {
                int fromStart = i - s0;
                int fromEnd = end - i;
                double env = fromStart < atk ? (double)fromStart / atk : 1.0;
                if (fromEnd <= rel) env = Math.Min(env, (double)fromEnd / rel);
                buf[i] += Osc(fromStart / (double)SR, f, n.Wave, n.Duty) * n.Vol * env;
            }
        }
    }

    static void AddKick(double[] buf, double beat, double spb, double vol)
    {
        int s0 = (int)(beat * spb);
        int len = (int)(0.13 * SR);
        for (int i = 0; i < len && s0 + i < buf.Length; i++)
        {
            double t = i / (double)SR;
            double f = 155 - 105 * t / 0.13;
            double env = 1 - t / 0.13;
            buf[s0 + i] += Math.Sin(2 * Math.PI * f * t) * vol * env;
        }
    }

    static void AddSnare(double[] buf, double beat, double spb, double vol)
    {
        int s0 = (int)(beat * spb);
        int len = (int)(0.16 * SR);
        var rnd = new Random(7);
        double noise = 0;
        for (int i = 0; i < len && s0 + i < buf.Length; i++)
        {
            double t = i / (double)SR;
            noise = noise * 0.9 + (rnd.NextDouble() * 2 - 1) * 0.1;
            double env = Math.Pow(1 - t / 0.16, 2);
            buf[s0 + i] += (noise * 2.2 + Math.Sin(2 * Math.PI * 190 * t) * 0.4) * vol * env;
        }
    }

    static void AddHat(double[] buf, double beat, double spb, double vol, double dur)
    {
        int s0 = (int)(beat * spb);
        int len = (int)(dur * SR);
        var rnd = new Random(11);
        double delay = 0;
        for (int i = 0; i < len && s0 + i < buf.Length; i++)
        {
            double t = i / (double)SR;
            double w = rnd.NextDouble() * 2 - 1;
            double hp = w - delay;
            delay = w;
            double env = Math.Pow(1 - t / dur, 2);
            buf[s0 + i] += hp * 1.4 * vol * env;
        }
    }

    static void FadeTail(double[] buf, double seconds)
    {
        int n = (int)Math.Min(buf.Length, seconds * SR);
        for (int i = 0; i < n; i++)
        {
            double f = 0.5 + 0.5 * Math.Sin(Math.PI / 2 * (double)i / n);
            buf[buf.Length - n + i] *= f;
        }
    }

    static void Normalize(double[] buf, double peak)
    {
        double max = 0;
        for (int i = 0; i < buf.Length; i++) { double a = Math.Abs(buf[i]); if (a > max) max = a; }
        if (max < 1e-9) return;
        double k = peak / max;
        for (int i = 0; i < buf.Length; i++) buf[i] *= k;
    }

    static void WriteWav(string path, double[] buf)
    {
        using var ms = new MemoryStream();
        using (var w = new BinaryWriter(ms, Encoding.UTF8, true))
        {
            w.Write(Encoding.ASCII.GetBytes("RIFF"));
            w.Write(36 + buf.Length * 2);
            w.Write(Encoding.ASCII.GetBytes("WAVE"));
            w.Write(Encoding.ASCII.GetBytes("fmt "));
            w.Write(16);
            w.Write((short)1);
            w.Write((short)1);
            w.Write(SR);
            w.Write(SR * 2);
            w.Write((short)2);
            w.Write((short)16);
            w.Write(Encoding.ASCII.GetBytes("data"));
            w.Write(buf.Length * 2);
        }
        byte[] raw = new byte[buf.Length * 2];
        for (int i = 0; i < buf.Length; i++)
        {
            double s = Math.Max(-1.0, Math.Min(1.0, buf[i]));
            short v = (short)(s * short.MaxValue);
            raw[i * 2] = (byte)(v & 0xFF);
            raw[i * 2 + 1] = (byte)((v >> 8) & 0xFF);
        }
        ms.Write(raw, 0, raw.Length);
        File.WriteAllBytes(path, ms.ToArray());
    }
public static void Generate(string outDir)
    {
        // ================= 探索 BGM：C 大调 / 112 BPM / 8 小节（明快弹跳） =================
        double spb = SR * 60.0 / 112.0;
        var notes = new List<Note>();

        // 和弦进行：C / G / Am / F / C / G / F / C（经典欢快走向）
        // 每小节：低音八分弹跳（根音↔高八度）+ 三角波琶音
        int[][] prog =
        {
            new[] { 36, 48 },  // C
            new[] { 31, 43 },  // G
            new[] { 33, 45 },  // Am
            new[] { 29, 41 },  // F
            new[] { 36, 48 },  // C
            new[] { 31, 43 },  // G
            new[] { 29, 41 },  // F
            new[] { 36, 48 },  // C
        };
        int[][] arps =
        {
            new[] { 60, 64, 67, 72, 76, 72, 67, 64 },  // C
            new[] { 59, 62, 67, 71, 74, 71, 67, 62 },  // G
            new[] { 57, 60, 64, 69, 72, 69, 64, 60 },  // Am
            new[] { 57, 60, 65, 69, 72, 69, 65, 60 },  // F
            new[] { 60, 64, 67, 72, 76, 72, 67, 64 },  // C
            new[] { 59, 62, 67, 71, 74, 71, 67, 62 },  // G
            new[] { 57, 60, 65, 69, 72, 69, 65, 60 },  // F
            new[] { 60, 64, 67, 72, 76, 72, 67, 64 },  // C
        };

        for (int bar = 0; bar < 8; bar++)
        {
            double b0 = bar * 4.0;
            for (int e = 0; e < 8; e++)
            {
                AddNote(notes, b0 + e * 0.5, 0.46, arps[bar][e], 0.11, W_TRI, 0.5, 0.010);
                AddNote(notes, b0 + e * 0.5, 0.44, prog[bar][e % 2], 0.22, W_PULSE, 0.25, 0.012);
            }
        }

        // 主旋律（方波，跳音感）：明亮上行 + 弹跳回落
        (double b, double d, int m)[] lead =
        {
            (0.0, 0.45, 76), (0.5, 0.45, 79), (1.0, 0.90, 84), (2.0, 0.45, 79), (2.5, 0.45, 81), (3.0, 0.90, 79),
            (4.0, 0.45, 74), (4.5, 0.45, 79), (5.0, 0.90, 83), (6.0, 0.45, 79), (6.5, 0.45, 81), (7.0, 0.90, 86),
            (8.0, 0.45, 81), (8.5, 0.45, 76), (9.0, 0.90, 81), (10.0, 0.45, 84), (10.5, 0.45, 81), (11.0, 0.90, 79),
            (12.0, 0.45, 81), (12.5, 0.45, 77), (13.0, 0.90, 81), (14.0, 0.45, 84), (14.5, 0.45, 81), (15.0, 0.90, 77),
            (16.0, 0.45, 76), (16.5, 0.45, 79), (17.0, 0.90, 84), (18.0, 0.45, 88), (18.5, 0.45, 84), (19.0, 0.90, 79),
            (20.0, 0.45, 79), (20.5, 0.45, 83), (21.0, 0.90, 86), (22.0, 0.45, 83), (22.5, 0.45, 79), (23.0, 0.90, 74),
            (24.0, 0.45, 81), (24.5, 0.45, 84), (25.0, 0.90, 88), (26.0, 0.45, 84), (26.5, 0.45, 81), (27.0, 0.90, 77),
            (28.0, 0.45, 79), (28.5, 0.45, 83), (29.0, 0.45, 86), (29.5, 0.45, 83), (30.0, 1.90, 84),
        };
        for (int i = 0; i < lead.Length; i++)
            AddNote(notes, lead[i].b, lead[i].d, lead[i].m, 0.24, W_SQUARE, 0.5, 0.014);

        int totalExplore = (int)(32 * spb);
        var explore = new double[totalExplore];
        Render(notes, explore, spb);
        // 轻鼓点：1/3 拍软底鼓 + 反拍踩镲（轻快不吵）
        for (int bar = 0; bar < 8; bar++)
        {
            double b0 = bar * 4.0;
            AddKick(explore, b0 + 0, spb, 0.20);
            AddKick(explore, b0 + 2, spb, 0.16);
            for (int e = 0; e < 8; e++)
                if (e % 2 == 1) AddHat(explore, b0 + e * 0.5, spb, 0.06, 0.04);
        }
        FadeTail(explore, 0.06);
        Normalize(explore, 0.60);

        // ================= 战斗 BGM：A 大调 / 150 BPM / 8 小节（热血向上） =================
        double spb2 = SR * 60.0 / 150.0;
        var n2 = new List<Note>();

        // 和弦进行：A / D / E / A / F#m / D / E / A
        int[][] bass2 =
        {
            new[] { 33, 45 },  // A
            new[] { 38, 50 },  // D
            new[] { 40, 52 },  // E
            new[] { 33, 45 },  // A
            new[] { 30, 42 },  // F#m
            new[] { 38, 50 },  // D
            new[] { 40, 52 },  // E
            new[] { 33, 45 },  // A
        };
        for (int bar = 0; bar < 8; bar++)
        {
            double b0 = bar * 4.0;
            for (int e = 0; e < 8; e++)
                AddNote(n2, b0 + e * 0.5, 0.47, bass2[bar][e % 2], 0.30, W_PULSE, 0.22, 0.010);
        }

        (double b, double d, int m)[] lead2 =
        {
            (0.0, 0.45, 69), (0.5, 0.45, 73), (1.0, 0.45, 76), (1.5, 0.45, 81), (2.0, 0.45, 76), (2.5, 0.45, 73), (3.0, 0.90, 76),
            (4.0, 0.45, 74), (4.5, 0.45, 78), (5.0, 0.45, 81), (5.5, 0.45, 86), (6.0, 0.45, 81), (6.5, 0.45, 78), (7.0, 0.90, 81),
            (8.0, 0.45, 76), (8.5, 0.45, 80), (9.0, 0.45, 83), (9.5, 0.45, 88), (10.0, 0.45, 83), (10.5, 0.45, 80), (11.0, 0.90, 83),
            (12.0, 0.45, 81), (12.5, 0.45, 76), (13.0, 0.45, 73), (13.5, 0.45, 69), (14.0, 0.90, 73), (15.0, 0.90, 76),
            (16.0, 0.45, 78), (16.5, 0.45, 81), (17.0, 0.45, 85), (17.5, 0.45, 81), (18.0, 0.45, 78), (18.5, 0.45, 81), (19.0, 0.90, 85),
            (20.0, 0.45, 86), (20.5, 0.45, 81), (21.0, 0.45, 78), (21.5, 0.45, 74), (22.0, 0.90, 78), (23.0, 0.90, 81),
            (24.0, 0.45, 88), (24.5, 0.45, 83), (25.0, 0.45, 80), (25.5, 0.45, 76), (26.0, 0.45, 80), (26.5, 0.45, 83), (27.0, 0.90, 88),
            (28.0, 0.45, 81), (28.5, 0.45, 85), (29.0, 0.45, 88), (29.5, 0.45, 85), (30.0, 1.90, 81),
        };
        for (int i = 0; i < lead2.Length; i++)
            AddNote(n2, lead2[i].b, lead2[i].d, lead2[i].m, 0.24, W_SQUARE, 0.5, 0.012);

        int totalBattle = (int)(32 * spb2);
        var battle = new double[totalBattle];
        Render(n2, battle, spb2);
        ReAddDrums(battle, spb2);
        FadeTail(battle, 0.06);
        Normalize(battle, 0.62);

        WriteWav(Path.Combine(outDir, "bgm_explore.wav"), explore);
        WriteWav(Path.Combine(outDir, "bgm_battle.wav"), battle);

        // ================= 通用音效（短促一次性） =================
        GenerateSfx(Path.Combine(outDir, "step.wav"), BuildStep());            // 移动：短促踩踏
        GenerateSfx(Path.Combine(outDir, "door_open.wav"), BuildDoor());       // 开门：金属门滑动
        GenerateSfx(Path.Combine(outDir, "victory.wav"), BuildVictory());      // 战斗胜利：上行琶音
        GenerateSfx(Path.Combine(outDir, "player_death.wav"), BuildDeath());   // 玩家阵亡：下行悲音
        GenerateSfx(Path.Combine(outDir, "flyorb_pickup.wav"), BuildFlyOrbPickup()); // 拾取飞行器：叮
        GenerateSfx(Path.Combine(outDir, "flyorb_launch.wav"), BuildFlyOrbLaunch()); // 启动穿梭：嗡鸣闪烁
        GenerateSfx(Path.Combine(outDir, "flyorb_arrive.wav"), BuildFlyOrbArrive()); // 传送完成：落地
    }

    static double[] BuildStep()
    {
        var buf = new double[(int)(0.09 * SR)];
        var rnd = new Random(21);
        double noise = 0;
        for (int i = 0; i < buf.Length; i++)
        {
            double t = i / (double)SR;
            noise = noise * 0.85 + (rnd.NextDouble() * 2 - 1) * 0.15;
            double thump = Math.Sin(2 * Math.PI * (115 - 45 * t / 0.09) * t) * 0.5;
            double env = Math.Pow(1 - t / 0.09, 2.2);
            buf[i] = (thump + noise * 0.5) * env;
        }
        return buf;
    }

    static double[] BuildDoor()
    {
        var buf = new double[(int)(0.30 * SR)];
        var rnd = new Random(33);
        for (int i = 0; i < buf.Length; i++)
        {
            double t = i / (double)SR;
            double f = 640 + 460 * Math.Sin(2 * Math.PI * 5 * t) * (1 - t / 0.30) + 90 * Math.Sin(2 * Math.PI * 17 * t);
            double metallic = Math.Sign(Math.Sin(2 * Math.PI * f * t)) * 0.5;
            double env = Math.Min(1.0, 12 * t) * Math.Pow(1 - t / 0.30, 2.0);
            double squeak = (rnd.NextDouble() * 2 - 1) * 0.22;
            buf[i] = (metallic + squeak) * env;
        }
        return buf;
    }

    static double[] BuildVictory()
    {
        var buf = new double[(int)(0.62 * SR)];
        var notes = new List<Note>();
        AddNote(notes, 0.00, 0.22, 72, 0.34, W_SQUARE, 0.5, 0.015);
        AddNote(notes, 0.22, 0.22, 76, 0.34, W_SQUARE, 0.5, 0.015);
        AddNote(notes, 0.44, 0.50, 79, 0.36, W_SQUARE, 0.5, 0.020);
        Render(notes, buf, SR * 60.0 / 120.0);
        FadeTail(buf, 0.10);
        return buf;
    }

    static double[] BuildDeath()
    {
        var buf = new double[(int)(0.78 * SR)];
        var notes = new List<Note>();
        AddNote(notes, 0.00, 0.20, 62, 0.32, W_TRI, 0.5, 0.015);
        AddNote(notes, 0.20, 0.24, 57, 0.32, W_TRI, 0.5, 0.015);
        AddNote(notes, 0.44, 0.30, 50, 0.32, W_TRI, 0.5, 0.020);
        Render(notes, buf, SR * 60.0 / 80.0);
        FadeTail(buf, 0.15);
        return buf;
    }

    static double[] BuildFlyOrbPickup()
    {
        // 拾取飞行器：短 8bit「叮」正向双音上行（约 0.28s）
        var buf = new double[(int)(0.28 * SR)];
        var notes = new List<Note>();
        AddNote(notes, 0.00, 0.10, 88, 0.34, W_SQUARE, 0.5, 0.010);
        AddNote(notes, 0.10, 0.16, 92, 0.34, W_SQUARE, 0.5, 0.015);
        AddNote(notes, 0.00, 0.26, 88, 0.16, W_TRI, 0.5, 0.020);
        Render(notes, buf, SR * 60.0 / 120.0);
        FadeTail(buf, 0.05);
        return buf;
    }

    static double[] BuildFlyOrbLaunch()
    {
        // 启动 / 传送瞬间：低频嗡鸣 + 高频闪烁（约 0.8s）
        var buf = new double[(int)(0.80 * SR)];
        var notes = new List<Note>();
        AddNote(notes, 0.00, 0.80, 45, 0.26, W_PULSE, 0.25, 0.010);
        AddNote(notes, 0.00, 0.80, 52, 0.13, W_PULSE, 0.25, 0.010);
        AddNote(notes, 0.00, 0.13, 76, 0.12, W_SQUARE, 0.5, 0.008);
        AddNote(notes, 0.22, 0.13, 81, 0.12, W_SQUARE, 0.5, 0.008);
        AddNote(notes, 0.44, 0.13, 76, 0.12, W_SQUARE, 0.5, 0.008);
        AddNote(notes, 0.66, 0.12, 84, 0.12, W_SQUARE, 0.5, 0.008);
        Render(notes, buf, SR * 60.0 / 120.0);
        FadeTail(buf, 0.10);
        return buf;
    }

    static double[] BuildFlyOrbArrive()
    {
        // 传送完成：清脆 8bit 落地音（约 0.35s）
        var buf = new double[(int)(0.35 * SR)];
        var notes = new List<Note>();
        AddNote(notes, 0.00, 0.08, 96, 0.30, W_SQUARE, 0.5, 0.008);
        AddNote(notes, 0.08, 0.22, 88, 0.26, W_TRI, 0.5, 0.015);
        Render(notes, buf, SR * 60.0 / 120.0);
        FadeTail(buf, 0.06);
        return buf;
    }

    static void GenerateSfx(string path, double[] buf)
    {
        Normalize(buf, 0.72);
        WriteWav(path, buf);
    }

    static void ReAddDrums(double[] battle, double spb2)
    {
        for (int bar = 0; bar < 8; bar++)
        {
            double b0 = bar * 4.0;
            AddKick(battle, b0 + 0, spb2, 0.42);
            AddKick(battle, b0 + 2, spb2, 0.42);
            AddSnare(battle, b0 + 1, spb2, 0.36);
            AddSnare(battle, b0 + 3, spb2, 0.36);
            for (int s = 0; s < 16; s++)
                AddHat(battle, b0 + s * 0.25, spb2, s % 4 == 2 ? 0.13 : 0.08, 0.055);
        }
    }
}
'@
# ---------- 执行合成 ----------
Push-Location $OutDir
try {
    [BgmGen]::Generate($OutDir)
} finally {
    Pop-Location
}

foreach ($f in 'bgm_explore.wav', 'bgm_battle.wav', 'step.wav', 'door_open.wav', 'victory.wav', 'player_death.wav',
    'flyorb_pickup.wav', 'flyorb_launch.wav', 'flyorb_arrive.wav') {
    $p = Join-Path $OutDir $f
    if (-not (Test-Path $p)) { throw "生成失败：$p" }
    $len = (Get-Item $p).Length
    $secs = ($len - 44) / 2 / 44100.0
    Write-Host ("OK  {0,-20} {1,9:N0} bytes  {2:N2}s" -f $f, $len, $secs)
}