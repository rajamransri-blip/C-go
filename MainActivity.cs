using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace KuronamiGfx;

[Activity(Label = "Kuronami GFX", MainLauncher = true)]
public class MainActivity : Activity
{
    private LinearLayout? _cardsContainer;
    private readonly HttpClient _http = new();
    private const string PackageName = "com.pubg.imobile";

    // Remote raw JSON link (Can be replaced with your own raw github link)
    private const string RawJsonUrl = "https://raw.githubusercontent.com/actions/starter-workflows/main/README.md";

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var rootLayout = new RelativeLayout(this);
        rootLayout.SetBackgroundColor(Color.ParseColor("#060A10"));

        var scrollView = new ScrollView(this);
        scrollView.LayoutParameters = new RelativeLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.MatchParent);

        var mainVertical = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };
        mainVertical.SetPadding(dp(18), dp(36), dp(18), dp(100)); // space for bottom bar

        // --- 1. HEADER SECTION ---
        mainVertical.AddView(CreateHeaderView());

        // --- 2. FILES TITLE & BADGE ---
        mainVertical.AddView(CreateFilesHeader());

        // --- 3. TARGET GAME SELECTOR ---
        mainVertical.AddView(CreateTargetSelector());

        // --- 4. SEARCH BAR ---
        mainVertical.AddView(CreateSearchBar());

        // --- 5. DYNAMIC CARDS CONTAINER ---
        _cardsContainer = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };
        mainVertical.AddView(_cardsContainer);

        scrollView.AddView(mainVertical);
        rootLayout.AddView(scrollView);

        // --- 6. FLOATING BOTTOM NAV PILL ---
        rootLayout.AddView(CreateBottomNav());

        SetContentView(rootLayout);

        LoadConfigs();
    }

    private View CreateHeaderView()
    {
        var header = new RelativeLayout(this);
        var p = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        p.SetMargins(0, 0, 0, dp(16));
        header.LayoutParameters = p;

        var titleBox = new LinearLayout(this) { Orientation = Orientation.Vertical };
        var t1 = new TextView(this) { Text = "KURONAMI\nGFX.", TextSize = 22, Typeface = Typeface.DefaultBold };
        t1.SetTextColor(Color.White);
        var t2 = new TextView(this) { Text = "YOUR GAME, YOUR\nCHOICE !", TextSize = 11 };
        t2.SetTextColor(Color.ParseColor("#475569"));
        titleBox.AddView(t1);
        titleBox.AddView(t2);

        var rightActions = new LinearLayout(this) { Orientation = Orientation.Vertical };
        var rParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        rParams.AddRule(LayoutRules.AlignParentRight);
        rightActions.LayoutParameters = rParams;
        rightActions.SetGravity(GravityFlags.Right);

        var joinBadge = new TextView(this) { Text = "• JOIN US", TextSize = 10, Typeface = Typeface.DefaultBold };
        joinBadge.SetTextColor(Color.ParseColor("#C084FC"));
        joinBadge.Background = CreateRoundedDrawable("#2E1065", dp(10));
        joinBadge.SetPadding(dp(10), dp(4), dp(10), dp(4));

        var teleIcon = new TextView(this) { Text = "✈", TextSize = 16 };
        teleIcon.SetTextColor(Color.White);
        teleIcon.SetGravity(GravityFlags.Center);
        teleIcon.Background = CreateRoundedDrawable("#3B82F6", dp(20));
        var iconP = new LinearLayout.LayoutParams(dp(40), dp(40));
        iconP.SetMargins(0, dp(8), 0, 0);
        teleIcon.LayoutParameters = iconP;

        rightActions.AddView(joinBadge);
        rightActions.AddView(teleIcon);

        header.AddView(titleBox);
        header.AddView(rightActions);
        return header;
    }

    private View CreateFilesHeader()
    {
        var h = new RelativeLayout(this);
        var p = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        p.SetMargins(0, 0, 0, dp(12));
        h.LayoutParameters = p;

        var title = new TextView(this) { Text = "FILES", TextSize = 18, Typeface = Typeface.DefaultBold };
        title.SetTextColor(Color.White);

        var count = new TextView(this) { Text = "8 Files", TextSize = 12 };
        count.SetTextColor(Color.ParseColor("#38BDF8"));
        count.Background = CreateRoundedDrawable("#082F49", dp(8));
        count.SetPadding(dp(10), dp(4), dp(10), dp(4));
        var cp = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        cp.AddRule(LayoutRules.AlignParentRight);
        count.LayoutParameters = cp;

        h.AddView(title);
        h.AddView(count);
        return h;
    }

    private View CreateTargetSelector()
    {
        var targetBox = new LinearLayout(this)
        {
            Orientation = Orientation.Horizontal
        };
        var p = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, dp(48));
        p.SetMargins(0, 0, 0, dp(10));
        targetBox.LayoutParameters = p;
        targetBox.Background = CreateRoundedDrawable("#0F172A", dp(12), "#1E293B", 2);
        targetBox.SetGravity(GravityFlags.CenterVertical);
        targetBox.SetPadding(dp(14), 0, dp(14), 0);

        var label = new TextView(this) { Text = "🎓 Target: BGMI Mobile [IN]", TextSize = 13, Typeface = Typeface.DefaultBold };
        label.SetTextColor(Color.White);
        var lp = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1);
        label.LayoutParameters = lp;

        var changeBtn = new TextView(this) { Text = "CHANGE >", TextSize = 12, Typeface = Typeface.DefaultBold };
        changeBtn.SetTextColor(Color.ParseColor("#38BDF8"));

        targetBox.AddView(label);
        targetBox.AddView(changeBtn);
        return targetBox;
    }

    private View CreateSearchBar()
    {
        var search = new EditText(this)
        {
            Hint = "🔍  Search files, configs...",
            TextSize = 13
        };
        search.SetHintTextColor(Color.ParseColor("#64748B"));
        search.SetTextColor(Color.White);
        search.Background = CreateRoundedDrawable("#0F172A", dp(14));
        search.SetPadding(dp(16), dp(12), dp(16), dp(12));
        var p = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, dp(46));
        p.SetMargins(0, 0, 0, dp(16));
        search.LayoutParameters = p;
        return search;
    }

    private View CreateBottomNav()
    {
        var nav = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        var p = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, dp(62));
        p.AddRule(LayoutRules.AlignParentBottom);
        p.SetMargins(dp(20), 0, dp(20), dp(16));
        nav.LayoutParameters = p;
        nav.Background = CreateRoundedDrawable("#1E293B", dp(30));
        nav.SetGravity(GravityFlags.Center);

        nav.AddView(CreateNavItem("🏠", "Home", false));
        nav.AddView(CreateNavItem("📁", "Files", true));
        nav.AddView(CreateNavItem("🔧", "Editor", false));
        nav.AddView(CreateNavItem("⚙", "Settings", false));
        return nav;
    }

    private View CreateNavItem(string icon, string title, bool active)
    {
        var item = new LinearLayout(this) { Orientation = Orientation.Vertical };
        item.SetGravity(GravityFlags.Center);
        var p = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1);
        item.LayoutParameters = p;

        if (active)
        {
            item.Background = CreateRoundedDrawable("#0284C7", dp(20));
            item.SetPadding(dp(6), dp(6), dp(6), dp(6));
        }

        var i = new TextView(this) { Text = icon, TextSize = 16 };
        i.SetTextColor(active ? Color.White : Color.ParseColor("#64748B"));
        i.SetGravity(GravityFlags.Center);

        var t = new TextView(this) { Text = title, TextSize = 10 };
        t.SetTextColor(active ? Color.White : Color.ParseColor("#64748B"));
        t.SetGravity(GravityFlags.Center);

        item.AddView(i);
        item.AddView(t);
        return item;
    }

    private async void LoadConfigs()
    {
        // Default sample data mimicking real screenshot
        var defaultList = new List<ConfigItem>
        {
            new ConfigItem
            {
                Id = "1",
                Title = "BGMI LUA PAK V1",
                Description = "ROOM WORKING AIMBOT\nFOV AIMBOT\nAIMBOT TARGET CUSTOMIZABLE\nLESS RECOIL\nMAGIC BULLET",
                ImageUrl = "https://picsum.photos/600/340",
                BadgeNum = "10",
                BadgeText = "BGMI ONLY",
                FileName = "Active.sav",
                TargetSubpath = "files/UE4Game/ShadowTrackerExtra/ShadowTrackerExtra/Saved/SaveGames",
                FileUrl = "https://raw.githubusercontent.com/actions/starter-workflows/main/README.md"
            }
        };

        try
        {
            var response = await _http.GetStringAsync(RawJsonUrl);
            var items = JsonSerializer.Deserialize<List<ConfigItem>>(response);
            if (items != null && items.Count > 0)
            {
                defaultList = items;
            }
        }
        catch { }

        foreach (var item in defaultList)
        {
            RenderItemCard(item);
        }
    }

    private void RenderItemCard(ConfigItem item)
    {
        var card = new LinearLayout(this) { Orientation = Orientation.Vertical };
        var cardParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        cardParams.SetMargins(0, 0, 0, dp(20));
        card.LayoutParameters = cardParams;
        card.Background = CreateRoundedDrawable("#0E1626", dp(16), "#1E293B", 2);

        // Preview Image Frame
        var imageFrame = new RelativeLayout(this);
        var frameParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, dp(180));
        imageFrame.LayoutParameters = frameParams;

        var img = new ImageView(this);
        img.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
        img.SetScaleType(ImageView.ScaleType.CenterCrop);
        img.SetBackgroundColor(Color.ParseColor("#1E293B"));
        imageFrame.AddView(img);

        // Load Remote Image Async
        if (!string.IsNullOrEmpty(item.ImageUrl))
        {
            Task.Run(async () =>
            {
                try
                {
                    var bytes = await _http.GetByteArrayAsync(item.ImageUrl);
                    var bmp = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
                    RunOnUiThread(() => img.SetImageBitmap(bmp));
                }
                catch { }
            });
        }

        // Badges (e.g. 10 | BGMI ONLY)
        var badgeLayout = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        var bParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        bParams.AddRule(LayoutRules.AlignParentBottom);
        bParams.SetMargins(dp(12), 0, 0, dp(12));
        badgeLayout.LayoutParameters = bParams;

        var b1 = new TextView(this) { Text = item.BadgeNum, TextSize = 12, Typeface = Typeface.DefaultBold };
        b1.SetTextColor(Color.White);
        b1.Background = CreateRoundedDrawable("#0284C7", dp(14));
        b1.SetPadding(dp(12), dp(4), dp(12), dp(4));

        var b2 = new TextView(this) { Text = item.BadgeText, TextSize = 12, Typeface = Typeface.DefaultBold };
        b2.SetTextColor(Color.ParseColor("#38BDF8"));
        b2.Background = CreateRoundedDrawable("#0F172A", dp(14));
        b2.SetPadding(dp(14), dp(4), dp(14), dp(4));
        var b2p = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        b2p.SetMargins(dp(6), 0, 0, 0);
        b2.LayoutParameters = b2p;

        badgeLayout.AddView(b1);
        badgeLayout.AddView(b2);
        imageFrame.AddView(badgeLayout);
        card.AddView(imageFrame);

        // Details Body
        var body = new LinearLayout(this) { Orientation = Orientation.Vertical };
        body.SetPadding(dp(14), dp(14), dp(14), dp(14));

        var title = new TextView(this) { Text = item.Title, TextSize = 18, Typeface = Typeface.DefaultBold };
        title.SetTextColor(Color.White);
        body.AddView(title);

        var desc = new TextView(this) { Text = item.Description, TextSize = 12, Typeface = Typeface.DefaultBold };
        desc.SetTextColor(Color.ParseColor("#64748B"));
        var dpParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        dpParams.SetMargins(0, dp(6), 0, dp(14));
        desc.LayoutParameters = dpParams;
        body.AddView(desc);

        // Action Button (DOWNLOAD -> APPLY)
        var btn = new Button(this)
        {
            Text = "▶  DOWNLOAD & APPLY",
            TextSize = 14,
            Typeface = Typeface.DefaultBold
        };
        btn.SetTextColor(Color.White);
        btn.Background = CreateRoundedDrawable("#0284C7", dp(12));
        btn.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, dp(48));

        btn.Click += async (s, e) =>
        {
            string appDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string localPath = System.IO.Path.Combine(appDir, item.FileName);

            if (!item.IsDownloaded)
            {
                btn.Text = "DOWNLOADING...";
                btn.Enabled = false;
                try
                {
                    var data = await _http.GetByteArrayAsync(item.FileUrl);
                    await System.IO.File.WriteAllBytesAsync(localPath, data);
                    item.IsDownloaded = true;
                    btn.Text = "APPLY VIA SHIZUKU";
                    btn.Background = CreateRoundedDrawable("#10B981", dp(12));
                }
                catch
                {
                    btn.Text = "DOWNLOAD FAILED";
                    btn.Background = CreateRoundedDrawable("#EF4444", dp(12));
                }
                finally
                {
                    btn.Enabled = true;
                }
            }
            else
            {
                btn.Text = "APPLYING...";
                btn.Enabled = false;
                ShizukuService.ApplyConfig(localPath, PackageName, item.TargetSubpath, item.FileName);
                btn.Text = "APPLIED SUCCESSFULLY";
                btn.Background = CreateRoundedDrawable("#6366F1", dp(12));
            }
        };

        body.AddView(btn);
        card.AddView(body);
        _cardsContainer?.AddView(card);
    }

    private GradientDrawable CreateRoundedDrawable(string hexColor, int radius, string? strokeColor = null, int strokeWidth = 0)
    {
        var drawable = new GradientDrawable();
        drawable.SetColor(Color.ParseColor(hexColor));
        drawable.SetCornerRadius(radius);
        if (strokeColor != null && strokeWidth > 0)
        {
            drawable.SetStroke(strokeWidth, Color.ParseColor(strokeColor));
        }
        return drawable;
    }

    private int dp(int pixels) => (int)(pixels * Resources?.DisplayMetrics?.Density ?? 1);
}
