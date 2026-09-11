using System;
using System.IO;
using System.Net.Http;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Graphics;

namespace KuronamiGfx;

[Activity(Label = "Kuronami GFX", MainLauncher = true)]
public class MainActivity : Activity
{
    private LinearLayout? _container;
    private readonly HttpClient _http = new();

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var scroll = new ScrollView(this);
        _container = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };
        _container.SetPadding(40, 60, 40, 60);
        _container.SetBackgroundColor(Color.ParseColor("#090D16"));
        scroll.AddView(_container);
        SetContentView(scroll);

        var title = new TextView(this)
        {
            Text = "KURONAMI GFX",
            TextSize = 24,
            Typeface = Typeface.DefaultBold
        };
        title.SetTextColor(Color.ParseColor("#00E5FF"));
        _container.AddView(title);

        RenderCard(new ConfigItem
        {
            Id = "1",
            Title = "Optimal Graphics Profile",
            FileName = "Active.sav",
            TargetSubpath = "files/UE4Game/ShadowTrackerExtra/ShadowTrackerExtra/Saved/SaveGames",
            FileUrl = "https://raw.githubusercontent.com/actions/starter-workflows/main/README.md"
        });
    }

    private void RenderCard(ConfigItem item)
    {
        var card = new LinearLayout(this) { Orientation = Orientation.Vertical };
        card.SetBackgroundColor(Color.ParseColor("#161B22"));
        card.SetPadding(30, 30, 30, 30);

        var label = new TextView(this) { Text = item.Title, TextSize = 16 };
        label.SetTextColor(Color.White);
        card.AddView(label);

        var btn = new Button(this) { Text = "DOWNLOAD" };
        btn.SetBackgroundColor(Color.ParseColor("#007ACC"));
        btn.SetTextColor(Color.White);

        btn.Click += async (s, e) =>
        {
            string localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), item.FileName);
            if (!item.IsDownloaded)
            {
                btn.Text = "DOWNLOADING...";
                btn.Enabled = false;
                try
                {
                    var data = await _http.GetByteArrayAsync(item.FileUrl);
                    await File.WriteAllBytesAsync(localPath, data);
                    item.IsDownloaded = true;
                    btn.Text = "APPLY";
                    btn.SetBackgroundColor(Color.ParseColor("#10B981"));
                }
                catch
                {
                    btn.Text = "DOWNLOAD FAILED";
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
                ShizukuService.ApplyConfig(localPath, "com.pubg.imobile", item.TargetSubpath, item.FileName);
                btn.Text = "APPLIED";
                btn.SetBackgroundColor(Color.ParseColor("#6366F1"));
            }
        };

        card.AddView(btn);
        _container?.AddView(card);
    }
}
