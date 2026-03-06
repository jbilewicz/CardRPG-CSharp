using System.Windows.Media;

namespace CardRPG.UI;

public static class AudioManager
{
    private static readonly MediaPlayer _menuPlayer = new();
    private static readonly MediaPlayer _battlePlayer = new();
    private static readonly MediaPlayer _sfxPlayer = new();
    private static readonly MediaPlayer _combatSfxPlayer = new();
    private static readonly Random _rng = new();

    private static double _masterVolume = 0.5;
    private static double _menuVolume = 0.5;
    private static double _battleVolume = 0.5;
    private static double _sfxVolume = 0.7;

    private static bool _masterMuted;
    private static bool _menuMuted;
    private static bool _battleMuted;

    private static bool _menuPlaying;
    private static bool _battlePlaying;

    private static readonly string _assetsPath = System.IO.Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "Assets");

    public static double MasterVolume
    {
        get => _masterVolume;
        set { _masterVolume = Math.Clamp(value, 0, 1); ApplyVolumes(); }
    }

    public static double MenuVolume
    {
        get => _menuVolume;
        set { _menuVolume = Math.Clamp(value, 0, 1); ApplyVolumes(); }
    }

    public static double BattleVolume
    {
        get => _battleVolume;
        set { _battleVolume = Math.Clamp(value, 0, 1); ApplyVolumes(); }
    }

    public static double SfxVolume
    {
        get => _sfxVolume;
        set { _sfxVolume = Math.Clamp(value, 0, 1); ApplyVolumes(); }
    }

    public static bool MasterMuted
    {
        get => _masterMuted;
        set { _masterMuted = value; ApplyVolumes(); }
    }

    public static bool MenuMuted
    {
        get => _menuMuted;
        set { _menuMuted = value; ApplyVolumes(); }
    }

    public static bool BattleMuted
    {
        get => _battleMuted;
        set { _battleMuted = value; ApplyVolumes(); }
    }

    static AudioManager()
    {
        _menuPlayer.MediaEnded += (s, e) =>
        {
            _menuPlayer.Position = TimeSpan.Zero;
            _menuPlayer.Play();
        };
        _battlePlayer.MediaEnded += (s, e) =>
        {
            _battlePlayer.Position = TimeSpan.Zero;
            _battlePlayer.Play();
        };
    }

    public static void PlayMenuMusic()
    {
        if (_menuPlaying) return;
        try
        {
            var path = System.IO.Path.Combine(_assetsPath, "main_menu_sounds.wav");
            _menuPlayer.Open(new Uri(path));
            ApplyVolumes();
            _menuPlayer.Play();
            _menuPlaying = true;
        }
        catch { }
    }

    public static void StopMenuMusic()
    {
        try
        {
            _menuPlayer.Stop();
            _menuPlaying = false;
        }
        catch { }
    }

    public static void PlayBattleMusic()
    {
        if (_battlePlaying) return;
        try
        {
            var path = System.IO.Path.Combine(_assetsPath, "battle_music.flac");
            _battlePlayer.Open(new Uri(path));
            ApplyVolumes();
            _battlePlayer.Play();
            _battlePlaying = true;
        }
        catch { }
    }

    public static void StopBattleMusic()
    {
        try
        {
            _battlePlayer.Stop();
            _battlePlaying = false;
        }
        catch { }
    }

    public static void PlayButtonClick()
    {
        try
        {
            var path = System.IO.Path.Combine(_assetsPath, "button_click.wav");
            _sfxPlayer.Open(new Uri(path));
            ApplyVolumes();
            _sfxPlayer.Play();
        }
        catch { }
    }

    private static readonly string[] _attackSounds = 
        ["slash_attack.wav", "attack_sound.wav", "knife_scrape.wav"];

    public static void PlayAttackSound()
    {
        try
        {
            var file = _attackSounds[_rng.Next(_attackSounds.Length)];
            var path = System.IO.Path.Combine(_assetsPath, file);
            _combatSfxPlayer.Open(new Uri(path));
            double vol = _masterMuted ? 0 : _masterVolume * _sfxVolume;
            _combatSfxPlayer.Volume = vol;
            _combatSfxPlayer.Play();
        }
        catch { }
    }

    public static void PlayBuySound()
    {
        try
        {
            var path = System.IO.Path.Combine(_assetsPath, "cash_buy_shop.wav");
            _sfxPlayer.Open(new Uri(path));
            ApplyVolumes();
            _sfxPlayer.Play();
        }
        catch { }
    }

    private static void ApplyVolumes()
    {
        double effectiveMenu = _masterMuted || _menuMuted ? 0 : _masterVolume * _menuVolume;
        double effectiveBattle = _masterMuted || _battleMuted ? 0 : _masterVolume * _battleVolume;
        double effectiveSfx = _masterMuted ? 0 : _masterVolume * _sfxVolume;

        _menuPlayer.Volume = effectiveMenu;
        _battlePlayer.Volume = effectiveBattle;
        _sfxPlayer.Volume = effectiveSfx;
    }
}
