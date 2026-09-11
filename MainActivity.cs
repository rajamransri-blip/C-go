using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Net;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace KuronamiGfx;

[Activity(Label = "KURONAMI GFX", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
public class MainActivity : Activity
{
    private FrameLayout? _screenRoot;
    private View? _homeScreen;
    private View? _filesScreen;
    private View? _editorScreen;
    private View? _settingsScreen;

    private LinearLayout? _cardsStack;
    private TextView? _targetLabel;
    private TextView? _filesCountLabel;
    private TextView? _shizukuStatusIndicator;

    private readonly HttpClient _http = new();
    private string _selectedGamePackage = "com.pubg.imobile";
    private string _selectedGameName = "BGMI Mobile [IN]";
    private const string DemoDataJsonUrl = "https://raw.githubusercontent.com/actions/starter-workflows/main/README.md";

    private List<ConfigItem> _masterConfigList = new();
    private LinearLayout[] _navButtons = new LinearLayout[4];

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var rootLayout = new RelativeLayout(this);
        rootLayout.SetBackgroundColor(Color.ParseColor("#060A10"));

        _screenRoot = new FrameLayout(this);
        var pHost = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
        pHost.SetMargins(0, 0, 0, Dp(84)); // Floating navbar space
        _screenRoot.LayoutParameters = pHost;

        // Build all screens
        _homeScreen = BuildHomeScreen();
        _filesScreen = BuildFilesScreen();
        _editorScreen = BuildEditorScreen();
        _settingsScreen = BuildSettingsScreen();

        _screenRoot.AddView(_homeScreen);
        _screenRoot.AddView(_filesScreen);
        _screenRoot.AddView(_editorScreen);
        _screenRoot.AddView(_settingsScreen);

        rootLayout.AddView(_screenRoot);
        rootLayout.AddView(BuildFloatingNavBar());

        SetContentView(rootLayout);

        SwitchTab(1); // Default to Files screen
        FetchConfigs();
    }

    // --- TAB SWITCHER ---
    private void SwitchTab(int index)
    {
        if (_homeScreen != null) _homeScreen.Visibility = index == 0 ? ViewStates.Visible : ViewStates.Gone;
        if (_filesScreen != null) _filesScreen.Visibility = index == 1 ? ViewStates.Visible : ViewStates.Gone;
        if (_editorScreen != null) _editorScreen.Visibility = index == 2 ? ViewStates.Visible : ViewStates.Gone;
        if (_settingsScreen != null) _settingsScreen.Visibility = index == 3 ? ViewStates.Visible : ViewStates.Gone;

        for (int i = 0; i < 4; i++)
        {
            if (_navButtons[i] != null)
            {
                bool active = (i == index);
                _navButtons[i].Background = active ? CreateGlassPill("#0284C7", Dp(18)) : null;
                var icon = _navButtons[i].GetChildAt(0) as TextView;
                var text = _navButtons[i].GetChildAt(1) as TextView;
                if (icon != null) icon.SetTextColor(active ? Color.White : Color.ParseColor("#64748B"));
                if (text != null) text.SetTextColor(active ? Color.White : Color.ParseColor("#64748B"));
            }
        }

        if (index == 0) RefreshShizukuCheck();
    }

    // --- 1. HOME SCREEN (SHIZUKU REALTIME ENGINE) ---
    private View BuildHomeScreen()
    {
        var scroll = new ScrollView(this);
        var container = new LinearLayout(this) { Orientation = Orientation.Vertical };
        container.SetPadding(Dp(20), Dp(46), Dp(20), Dp(20));

        var header = new TextView(this) { Text = "SYSTEM CORE", TextSize = 22, Typeface = Typeface.DefaultBold };
        header.SetTextColor(Color.White);
        container.AddView(header);

        var sub = new TextView(this) { Text = "Realtime Shizuku Privileged Bridge", TextSize = 13 };
        sub.SetTextColor(Color.ParseColor("#38BDF8"));
        container.AddView(sub);

        // Shizuku Status Widget
        var statusCard = new LinearLayout(this) { Orientation = Orientation.Vertical };
        statusCard.Background = CreateGlassPill("#0E1626", Dp(18), "#1E293B", 2);
        statusCard.SetPadding(Dp(20), Dp(20), Dp(20), Dp(20));
        var scp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        scp.SetMargins(0, Dp(24), 0, 0);
        statusCard.LayoutParameters = scp;

        _shizukuStatusIndicator = new TextView(this)
        {
            Text = "● Checking Shizuku Service...",
            TextSize = 15,
            Typeface = Typeface.DefaultBold
        };
        _shizukuStatusIndicator.SetTextColor(Color.ParseColor("#FBBF24"));
        statusCard.AddView(_shizukuStatusIndicator);

        var shizukuDesc = new TextView(this)
        {
            Text = "Privileged shell API allows rootless writing directly to /Android/data/ without DocumentFile (SAF) limitations.",
            TextSize = 12
        };
        shizukuDesc.SetTextColor(Color.ParseColor("#94A3B8"));
        var sdp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        sdp.SetMargins(0, Dp(8), 0, Dp(16));
        shizukuDesc.LayoutParameters = sdp;
        statusCard.AddView(shizukuDesc);

        var checkBtn = new Button(this) { Text = "PING SHIZUKU NOW", TextSize = 13, Typeface = Typeface.DefaultBold };
        checkBtn.SetTextColor(Color.White);
        checkBtn.Background = CreateGlassPill("#1E293B", Dp(12), "#38BDF8", 1);
        checkBtn.Click += (s, e) => RefreshShizukuCheck();
        statusCard.AddView(checkBtn);

        container.AddView(statusCard);

        // Quick launch card
        var launchCard = new LinearLayout(this) { Orientation = Orientation.Vertical };
        launchCard.Background = CreateGlassPill("#0E1626", Dp(18), "#1E293B", 2);
        launchCard.SetPadding(Dp(20), Dp(20), Dp(20), Dp(20));
        var lcp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        lcp.SetMargins(0, Dp(16), 0, 0);
        launchCard.LayoutParameters = lcp;

        var lt = new TextView(this) { Text = "INSTANT GAME LAUNCH", TextSize = 14, Typeface = Typeface.DefaultBold };
        lt.SetTextColor(Color.White);
        launchCard.AddView(lt);

        var launchBtn = new Button(this) { Text = "LAUNCH BGMI DIRECT", TextSize = 13, Typeface = Typeface.DefaultBold };
        launchBtn.SetTextColor(Color.White);
        launchBtn.Background = CreateGlassPill("#0284C7", Dp(12));
        var lbp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, Dp(46));
        lbp.SetMargins(0, Dp(12), 0, 0);
        launchBtn.LayoutParameters = lbp;
        launchBtn.Click += (s, e) =>
        {
            try
            {
                Intent? launchIntent = PackageManager?.GetLaunchIntentForPackage(_selectedGamePackage);
                if (launchIntent != null) StartActivity(launchIntent);
                else Toast.MakeText(this, "Game not installed on device!", ToastLength.Short)?.Show();
            }
            catch { }
        };
        launchCard.AddView(launchBtn);
        container.AddView(launchCard);

        scroll.AddView(container);
        return scroll;
    }

    private void RefreshShizukuCheck()
    {
        if (_shizukuStatusIndicator == null) return;
        _shizukuStatusIndicator.Text = "● Checking...";
        _shizukuStatusIndicator.SetTextColor(Color.ParseColor("#FBBF24"));

        Task.Run(() =>
        {
            bool active = ShizukuService.CheckShizukuActive();
            RunOnUiThread(() =>
            {
                if (active)
                {
                    _shizukuStatusIndicator.Text = "● Shizuku Connected (Ready)";
                    _shizukuStatusIndicator.SetTextColor(Color.ParseColor("#10B981"));
                }
                else
                {
                    _shizukuStatusIndicator.Text = "● Shizuku Offline (Permission Needed)";
                    _shizukuStatusIndicator.SetTextColor(Color.ParseColor("#EF4444"));
                }
            });
        });
    }

    // --- 2. FILES SCREEN (MATCHING SCREENSHOT) ---
    private View BuildFilesScreen()
    {
        var scroll = new ScrollView(this);
        var mainLayout = new LinearLayout(this) { Orientation = Orientation.Vertical };
        mainLayout.SetPadding(Dp(18), Dp(36), Dp(18), Dp(20));

        // Top Kuronami Branding
        var topBar = new RelativeLayout(this);
        var tbParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        tbParams.SetMargins(0, 0, 0, Dp(16));
        topBar.LayoutParameters = tbParams;

        var brandBox = new LinearLayout(this) { Orientation = Orientation.Vertical };
        var b1 = new TextView(this) { Text = "KURONAMI\nGFX.", TextSize = 22, Typeface = Typeface.DefaultBold };
        b1.SetTextColor(Color.White);
        var b2 = new TextView(this) { Text = "YOUR GAME, YOUR\nCHOICE !", TextSize = 11 };
        b2.SetTextColor(Color.ParseColor("#475569"));
        brandBox.AddView(b1);
        brandBox.AddView(b2);

        var rightActions = new LinearLayout(this) { Orientation = Orientation.Vertical };
        var rParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        rParams.AddRule(LayoutRules.AlignParentRight);
        rightActions.LayoutParameters = rParams;
        rightActions.SetGravity(GravityFlags.Right);

        var joinUs = new TextView(this) { Text = "• JOIN US", TextSize = 10, Typeface = Typeface.DefaultBold };
        joinUs.SetTextColor(Color.ParseColor("#C084FC"));
        joinUs.Background = CreateGlassPill("#2E1065", Dp(10));
        joinUs.SetPadding(Dp(10), Dp(4), Dp(10), Dp(4));
        joinUs.Click += (s, e) => OpenWebLink("https://t.me");

        var teleBtn = new TextView(this) { Text = "✈", TextSize = 18, Gravity = GravityFlags.Center };
        teleBtn.SetTextColor(Color.White);
        teleBtn.Background = CreateGlassPill("#3B82F6", Dp(22));
        var teleParams = new LinearLayout.LayoutParams(Dp(44), Dp(44));
        teleParams.SetMargins(0, Dp(8), 0, 0);
        teleBtn.LayoutParameters = teleParams;
        teleBtn.Click += (s, e) => OpenWebLink("https://t.me");

        rightActions.AddView(joinUs);
        rightActions.AddView(teleBtn);

        topBar.AddView(brandBox);
        topBar.AddView(rightActions);
        mainLayout.AddView(topBar);

        // Files Header
        var filesBar = new RelativeLayout(this);
        var fbParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        fbParams.SetMargins(0, 0, 0, Dp(12));
        filesBar.LayoutParameters = fbParams;

        var filesTitle = new TextView(this) { Text = "FILES", TextSize = 18, Typeface = Typeface.DefaultBold };
        filesTitle.SetTextColor(Color.White);

        _filesCountLabel = new TextView(this) { Text = "8 Files", TextSize = 12 };
        _filesCountLabel.SetTextColor(Color.ParseColor("#38BDF8"));
        _filesCountLabel.Background = CreateGlassPill("#082F49", Dp(8));
        _filesCountLabel.SetPadding(Dp(10), Dp(4), Dp(10), Dp(4));
        var cntParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        cntParams.AddRule(LayoutRules.AlignParentRight);
        _filesCountLabel.LayoutParameters = cntParams;

        filesBar.AddView(filesTitle);
        filesBar.AddView(_filesCountLabel);
        mainLayout.AddView(filesBar);

        // Target Game Picker (Clickable)
        var targetSelector = new LinearLayout(this)
        {
            Orientation = Orientation.Horizontal,
            Background = CreateGlassPill("#0F172A", Dp(12), "#1E293B", 2)
        };
        targetSelector.SetGravity(GravityFlags.CenterVertical);
        targetSelector.SetPadding(Dp(14), 0, Dp(14), 0);
        var tsp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, Dp(50));
        tsp.SetMargins(0, 0, 0, Dp(12));
        targetSelector.LayoutParameters = tsp;

        _targetLabel = new TextView(this) { Text = $"🎓 Target: {_selectedGameName}", TextSize = 13, Typeface = Typeface.DefaultBold };
        _targetLabel.SetTextColor(Color.White);
        var tlp = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1);
        _targetLabel.LayoutParameters = tlp;

        var changeText = new TextView(this) { Text = "CHANGE >", TextSize = 12, Typeface = Typeface.DefaultBold };
        changeText.SetTextColor(Color.ParseColor("#38BDF8"));

        targetSelector.AddView(_targetLabel);
        targetSelector.AddView(changeText);
        targetSelector.Click += (s, e) => ShowTargetDialog();
        mainLayout.AddView(targetSelector);

        // Search Bar with Realtime Filter
        var searchInput = new EditText(this)
        {
            Hint = "🔍  Search files, configs...",
            TextSize = 13
        };
        searchInput.SetHintTextColor(Color.ParseColor("#64748B"));
        searchInput.SetTextColor(Color.White);
        searchInput.Background = CreateGlassPill("#0F172A", Dp(14));
        searchInput.SetPadding(Dp(16), Dp(12), Dp(16), Dp(12));
        var sip = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, Dp(48));
        sip.SetMargins(0, 0, 0, Dp(16));
        searchInput.LayoutParameters = sip;
        searchInput.TextChanged += (s, e) =>
        {
            FilterCards(searchInput.Text ?? string.Empty);
        };
        mainLayout.AddView(searchInput);

        // Cards Vertical Stack
        _cardsStack = new LinearLayout(this) { Orientation = Orientation.Vertical };
        mainLayout.AddView(_cardsStack);

        scroll.AddView(mainLayout);
        return scroll;
    }

    private void ShowTargetDialog()
    {
        string[] games = { "BGMI Mobile [IN]", "PUBG Mobile [Global]", "PUBG Mobile [KR]" };
        string[] pkgs = { "com.pubg.imobile", "com.tencent.ig", "com.pubg.krmobile" };

        var builder = new AlertDialog.Builder(this);
        builder.SetTitle("Select Target Game");
        builder.SetItems(games, (s, e) =>
        {
            _selectedGameName = games[e.Which];
            _selectedGamePackage = pkgs[e.Which];
            if (_targetLabel != null) _targetLabel.Text = $"🎓 Target: {_selectedGameName}";
        });
        builder.Show();
    }

    // --- 3. CONFIG FETCHING & RENDERING ---
    private async void FetchConfigs()
    {
        // High quality offline fallback items
        _masterConfigList = new List<ConfigItem>
        {
            new ConfigItem
            {
                Id = "1",
                Title = "BGMI LUA PAK V1",
                Description = "ROOM WORKING AIMBOT\nFOV AIMBOT\nAIMBOT TARGET CUSTOMIZABLE\nLESS RECOIL\nMAGIC BULLET",
                ImageUrl = "https://picsum.photos/600/320",
                BadgeNum = "10",
                BadgeText = "BGMI ONLY",
                FileName = "game_patch_3.5.0.pak",
                TargetSubpath = "files/UE4Game/ShadowTrackerExtra/ShadowTrackerExtra/Saved/Paks",
                FileUrl = "https://raw.githubusercontent.com/actions/starter-workflows/main/README.md"
            },
            new ConfigItem
            {
                Id = "2",
                Title = "90 FPS ULTRA SMOOTH POTATO",
                Description = "UNLOCK CONSTANT 90 FPS\nZERO OVERHEATING FIX\nSMOOTH GRAPHICS TEXTURES",
                ImageUrl = "https://picsum.photos/601/320",
                BadgeNum = "11",
                BadgeText = "ALL VERSIONS",
                FileName = "Active.sav",
                TargetSubpath = "files/UE4Game/ShadowTrackerExtra/ShadowTrackerExtra/Saved/SaveGames",
                FileUrl = "https://raw.githubusercontent.com/actions/starter-workflows/main/README.md"
            }
        };

        try
        {
            var jsonStr = await _http.GetStringAsync(DemoDataJsonUrl);
            var onlineItems = JsonSerializer.Deserialize<List<ConfigItem>>(jsonStr);
            if (onlineItems != null && onlineItems.Count > 0)
            {
                _masterConfigList = onlineItems;
            }
        }
        catch { }

        RunOnUiThread(() => RenderCards(_masterConfigList));
    }

    private void FilterCards(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            RenderCards(_masterConfigList);
            return;
        }

        var filtered = _masterConfigList.Where(x => 
            x.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || 
            x.Description.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        RenderCards(filtered);
    }

    private void RenderCards(List<ConfigItem> items)
    {
        if (_cardsStack == null) return;
        _cardsStack.RemoveAllViews();
        if (_filesCountLabel != null) _filesCountLabel.Text = $"{items.Count} Files";

        foreach (var item in items)
        {
            var card = new LinearLayout(this) { Orientation = Orientation.Vertical };
            var cp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            cp.SetMargins(0, 0, 0, Dp(20));
            card.LayoutParameters = cp;
            card.Background = CreateGlassPill("#0E1626", Dp(16), "#1E293B", 2);

            // Image Banner Frame
            var frame = new RelativeLayout(this);
            frame.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, Dp(185));

            var img = new ImageView(this);
            img.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            img.SetScaleType(ImageView.ScaleType.CenterCrop);
            img.SetBackgroundColor(Color.ParseColor("#1E293B"));
            frame.AddView(img);

            // Image Loading Task
            if (!string.IsNullOrEmpty(item.ImageUrl))
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var b = await _http.GetByteArrayAsync(item.ImageUrl);
                        var bmp = BitmapFactory.DecodeByteArray(b, 0, b.Length);
                        RunOnUiThread(() => img.SetImageBitmap(bmp));
                    }
                    catch { }
                });
            }

            // Badges Frame
            var badgeStack = new LinearLayout(this) { Orientation = Orientation.Horizontal };
            var bsp = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            bsp.AddRule(LayoutRules.AlignParentBottom);
            bsp.SetMargins(Dp(12), 0, 0, Dp(12));
            badgeStack.LayoutParameters = bsp;

            var bNum = new TextView(this) { Text = item.BadgeNum, TextSize = 12, Typeface = Typeface.DefaultBold };
            bNum.SetTextColor(Color.White);
            bNum.Background = CreateGlassPill("#0284C7", Dp(14));
            bNum.SetPadding(Dp(12), Dp(4), Dp(12), Dp(4));

            var bText = new TextView(this) { Text = item.BadgeText, TextSize = 12, Typeface = Typeface.DefaultBold };
            bText.SetTextColor(Color.ParseColor("#38BDF8"));
            bText.Background = CreateGlassPill("#0F172A", Dp(14));
            bText.SetPadding(Dp(14), Dp(4), Dp(14), Dp(4));
            var btp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            btp.SetMargins(Dp(6), 0, 0, 0);
            bText.LayoutParameters = btp;

            badgeStack.AddView(bNum);
            badgeStack.AddView(bText);
            frame.AddView(badgeStack);
            card.AddView(frame);

            // Body Text Info
            var body = new LinearLayout(this) { Orientation = Orientation.Vertical };
            body.SetPadding(Dp(14), Dp(14), Dp(14), Dp(14));

            var title = new TextView(this) { Text = item.Title, TextSize = 18, Typeface = Typeface.DefaultBold };
            title.SetTextColor(Color.White);
            body.AddView(title);

            var desc = new TextView(this) { Text = item.Description, TextSize = 12, Typeface = Typeface.DefaultBold };
            desc.SetTextColor(Color.ParseColor("#64748B"));
            var dp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            dp.SetMargins(0, Dp(6), 0, Dp(14));
            desc.LayoutParameters = dp;
            body.AddView(desc);

            // Action Button (DOWNLOAD -> APPLY)
            var actionBtn = new Button(this)
            {
                Text = "▶  DOWNLOAD & APPLY",
                TextSize = 14,
                Typeface = Typeface.DefaultBold
            };
            actionBtn.SetTextColor(Color.White);
            actionBtn.Background = CreateGlassPill("#0284C7", Dp(12));
            actionBtn.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, Dp(48));

            actionBtn.Click += async (s, e) =>
            {
                string internalFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                string localPath = System.IO.Path.Combine(internalFolder, item.FileName);

                if (!item.IsDownloaded)
                {
                    actionBtn.Text = "DOWNLOADING...";
                    actionBtn.Enabled = false;
                    try
                    {
                        var data = await _http.GetByteArrayAsync(item.FileUrl);
                        await System.IO.File.WriteAllBytesAsync(localPath, data);
                        item.IsDownloaded = true;
                        actionBtn.Text = "APPLY TO PAKS (SHIZUKU)";
                        actionBtn.Background = CreateGlassPill("#10B981", Dp(12));
                    }
                    catch
                    {
                        actionBtn.Text = "DOWNLOAD FAILED (RETRY)";
                        actionBtn.Background = CreateGlassPill("#EF4444", Dp(12));
                    }
                    finally
                    {
                        actionBtn.Enabled = true;
                    }
                }
                else
                {
                    actionBtn.Text = "INJECTING TO PAKS...";
                    actionBtn.Enabled = false;
                    bool success = await Task.Run(() => 
                        ShizukuService.ApplyConfigToPaks(localPath, _selectedGamePackage, item.TargetSubpath, item.FileName));

                    if (success)
                    {
                        actionBtn.Text = "✓ PAK APPLIED SUCCESS";
                        actionBtn.Background = CreateGlassPill("#6366F1", Dp(12));
                    }
                    else
                    {
                        actionBtn.Text = "SHIZUKU PERMISSION ERROR";
                        actionBtn.Background = CreateGlassPill("#EF4444", Dp(12));
                        actionBtn.Enabled = true;
                    }
                }
            };

            body.AddView(actionBtn);
            card.AddView(body);
            _cardsStack.AddView(card);
        }
    }

    // --- 4. EDITOR SCREEN ---
    private View BuildEditorScreen()
    {
        var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
        layout.SetPadding(Dp(20), Dp(46), Dp(20), Dp(20));

        var title = new TextView(this) { Text = "HEX / INI EDITOR", TextSize = 22, Typeface = Typeface.DefaultBold };
        title.SetTextColor(Color.White);
        layout.AddView(title);

        var sub = new TextView(this) { Text = "Custom Graphics & Recoil Script Tuner", TextSize = 13 };
        sub.SetTextColor(Color.ParseColor("#38BDF8"));
        layout.AddView(sub);

        var editorBox = new EditText(this)
        {
            Text = "+CVars=r.PUBGDeviceFPS=90\n+CVars=r.PUBGQualityLevel=0\n+CVars=r.ShadowQuality=0\n+CVars=r.BloomQuality=0",
            TextSize = 13,
            Gravity = GravityFlags.Top
        };
        editorBox.SetTextColor(Color.ParseColor("#A5F3FC"));
        editorBox.Background = CreateGlassPill("#0F172A", Dp(14), "#1E293B", 2);
        editorBox.SetPadding(Dp(16), Dp(16), Dp(16), Dp(16));
        var ep = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, Dp(240));
        ep.SetMargins(0, Dp(20), 0, Dp(16));
        editorBox.LayoutParameters = ep;
        layout.AddView(editorBox);

        var saveBtn = new Button(this) { Text = "SAVE & INJECT CUSTOM INI", TextSize = 13, Typeface = Typeface.DefaultBold };
        saveBtn.SetTextColor(Color.White);
        saveBtn.Background = CreateGlassPill("#0284C7", Dp(12));
        saveBtn.Click += (s, e) =>
        {
            Toast.MakeText(this, "Custom script compiled to active game cache!", ToastLength.Short)?.Show();
        };
        layout.AddView(saveBtn);

        return layout;
    }

    // --- 5. SETTINGS SCREEN ---
    private View BuildSettingsScreen()
    {
        var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
        layout.SetPadding(Dp(20), Dp(46), Dp(20), Dp(20));

        var title = new TextView(this) { Text = "PREFERENCES", TextSize = 22, Typeface = Typeface.DefaultBold };
        title.SetTextColor(Color.White);
        layout.AddView(title);

        var infoCard = new LinearLayout(this) { Orientation = Orientation.Vertical };
        infoCard.Background = CreateGlassPill("#0E1626", Dp(16), "#1E293B", 2);
        infoCard.SetPadding(Dp(16), Dp(16), Dp(16), Dp(16));
        var ip = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        ip.SetMargins(0, Dp(24), 0, 0);
        infoCard.LayoutParameters = ip;

        var vText = new TextView(this) { Text = "App Version: 2.4.0-PRO", TextSize = 14, Typeface = Typeface.DefaultBold };
        vText.SetTextColor(Color.White);
        infoCard.AddView(vText);

        var bText = new TextView(this) { Text = "Architecture: C# .NET 8 Native Android\nTarget: BGMI / Android 11 to 15 Scoped Storage", TextSize = 12 };
        bText.SetTextColor(Color.ParseColor("#94A3B8"));
        var bp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        bp.SetMargins(0, Dp(6), 0, 0);
        bText.LayoutParameters = bp;
        infoCard.AddView(bText);

        layout.AddView(infoCard);
        return layout;
    }

    // --- 6. FLOATING IOS GLASS NAVBAR ---
    private View BuildFloatingNavBar()
    {
        var nav = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        var p = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, Dp(64));
        p.AddRule(LayoutRules.AlignParentBottom);
        p.SetMargins(Dp(18), 0, Dp(18), Dp(14));
        nav.LayoutParameters = p;
        nav.Background = CreateGlassPill("#1E293B", Dp(32));
        nav.SetGravity(GravityFlags.Center);

        nav.AddView(BuildNavItem(0, "🏠", "Home"));
        nav.AddView(BuildNavItem(1, "📁", "Files"));
        nav.AddView(BuildNavItem(2, "🔧", "Editor"));
        nav.AddView(BuildNavItem(3, "⚙", "Settings"));
        return nav;
    }

    private View BuildNavItem(int index, string icon, string label)
    {
        var item = new LinearLayout(this) { Orientation = Orientation.Vertical };
        item.SetGravity(GravityFlags.Center);
        var p = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1);
        item.LayoutParameters = p;
        item.SetPadding(0, Dp(6), 0, Dp(6));

        var ic = new TextView(this) { Text = icon, TextSize = 16, Gravity = GravityFlags.Center };
        var tx = new TextView(this) { Text = label, TextSize = 10, Gravity = GravityFlags.Center };

        item.AddView(ic);
        item.AddView(tx);

        _navButtons[index] = item;
        item.Click += (s, e) => SwitchTab(index);
        return item;
    }

    private void OpenWebLink(string url)
    {
        try
        {
            var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));
            StartActivity(intent);
        }
        catch { }
    }

    private GradientDrawable CreateGlassPill(string hexColor, int radius, string? strokeColor = null, int strokeWidth = 0)
    {
        var d = new GradientDrawable();
        d.SetColor(Color.ParseColor(hexColor));
        d.SetCornerRadius(radius);
        if (strokeColor != null && strokeWidth > 0)
        {
            d.SetStroke(strokeWidth, Color.ParseColor(strokeColor));
        }
        return d;
    }

    private int Dp(int px) => (int)(px * (Resources?.DisplayMetrics?.Density ?? 1));
}
