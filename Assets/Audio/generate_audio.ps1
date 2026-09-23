# 生成两段循环 BGM（WAV 16-bit / 44.1kHz 单声道）：
#   bgm_explore.wav —— 探索主题（A 小调 / 84 BPM / 8 小节 / ~22.86s 无缝循环）
#   bgm_battle.wav  —— 战斗主题（E 小调 / 140 BPM / 8 小节 / ~13.71s 无缝循环）
# 用法：pwsh -ExecutionPolicy Bypass -File Assets/Audio/generate_audio.ps1
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
        // ================= 探索 BGM：A 小调 / 84 BPM / 8 小节 =================
        double spb = SR * 60.0 / 84.0; // 倍拍 = 31500 采样
        var notes = new List<Note>();

        // 和弦根音（midi）：小节0-1 Am(A2=45)，2-3 F(F2=41)，4-5 C(C3=48)，6-7 G(G2=43)
        int[] roots = { 45, 45, 41, 41, 48, 48, 43, 43 };
        int[] arp = { 0, 7, 12, 15, 19, 15, 12, 7 };

        for (int bar = 0; bar < 8; bar++)
        {
            double b0 = bar * 4.0;
            for (int e = 0; e < 8; e++)
                AddNote(notes, b0 + e * 0.5, 0.46, roots[bar] + arp[e], 0.15, W_TRI, 0.5, 0.010);
            for (int beat = 0; beat < 4; beat++)
                AddNote(notes, b0 + beat, 0.92, roots[bar] - 12 + (beat % 2 == 1 ? 7 : 0), 0.20, W_PULSE, 0.25, 0.014);
        }

        // 旋律（后半段）：C/F 上行进入 G 收束
        int[,] melody = new int[,]
        {
            // { beat 拍, 时值拍, midi }
            { 160, 14, 72 }, { 175, 4, 76 }, { 180, 9, 79 }, { 190, 9, 76 },
            { 210, 14, 72 }, { 225, 4, 74 }, { 230, 4, 72 },
            { 240, 14, 76 }, { 255, 4, 79 }, { 260, 14, 81 }, { 275, 4, 79 },
            { 280, 9, 71 }, { 290, 9, 74 }, { 300, 14, 76 }, { 315, 4, 79 },
        };
        for (int i = 0; i < melody.GetLength(0); i++)
            AddNote(notes, melody[i, 0] / 10.0, melody[i, 1] / 10.0, melody[i, 2], 0.30, W_TRI, 0.5, 0.014);

        int totalExplore = (int)(32 * spb);
        var explore = new double[totalExplore];
        Render(notes, explore, spb);
        FadeTail(explore, 0.10);
        Normalize(explore, 0.60);

        // ================= 战斗 BGM：E 小调 / 140 BPM / 8 小节 =================
        double spb2 = SR * 60.0 / 140.0; // 倍拍 = 18900 采样
        var n2 = new List<Note>();

        for (int bar = 0; bar < 8; bar++)
        {
            double b0 = bar * 4.0;
            // 贝斯 8 分音符：E1/E2 交替
            for (int e = 0; e < 8; e++)
                AddNote(n2, b0 + e * 0.5, 0.47, 28 + (e % 2 == 1 ? 12 : 0), 0.30, W_PULSE, 0.22, 0.010);
        }
        int totalBattle = (int)(32 * spb2);
        (double b, double d, int m)[] lead = new (double, double, int)[]
        {
            (0.0, 0.45, 76), (1.0, 0.45, 79), (2.0, 0.45, 81), (3.0, 0.45, 79),
            (4.0, 0.45, 76), (5.0, 0.45, 79), (6.0, 0.45, 81), (7.0, 0.45, 79),
            (8.0, 0.45, 76), (9.0, 0.45, 79), (10.0, 0.45, 81), (11.0, 0.45, 79),
            (12.0, 0.45, 83), (13.0, 0.45, 81), (14.0, 0.45, 79), (15.0, 0.45, 81),
            (16.0, 0.45, 83), (17.0, 0.45, 81), (18.0, 0.45, 79), (19.0, 0.45, 81),
            (20.0, 0.45, 83), (21.0, 0.45, 81), (22.0, 0.45, 79), (23.0, 0.45, 81),
            (24.0, 0.22, 76), (24.5, 0.22, 79), (25.0, 0.22, 81), (25.5, 0.22, 83),
            (26.0, 0.42, 86), (27.0, 0.42, 88),
            (28.0, 0.42, 88), (28.5, 0.22, 86), (29.0, 0.42, 83), (29.5, 0.22, 81),
            (30.0, 0.42, 79), (30.5, 0.22, 81), (31.0, 0.42, 76), (31.5, 0.38, 79),
        };
        for (int i = 0; i < lead.Length; i++)
            AddNote(n2, lead[i].b, lead[i].d, lead[i].m, 0.24, W_SQUARE, 0.5, 0.012);

        var battle = new double[totalBattle];
        Render(n2, battle, spb2);
        ReAddDrums(battle, spb2);
        FadeTail(battle, 0.08);
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