using System.Diagnostics;
using YoutubeExplode.Exceptions;

namespace YDownload.WinForms;

public partial class MainForm : Form
{
    private static readonly int[] Bitrates = [128, 192, 256, 320];

    private readonly Downloader _downloader = new();
    private readonly UserSettings _settings = UserSettings.Load();

    private CancellationTokenSource? _cts;
    private string? _lastFile;

    public MainForm()
    {
        InitializeComponent();

        using (var icon = typeof(MainForm).Assembly.GetManifestResourceStream("YDownload.app.ico")!)
            Icon = new Icon(icon);

        bitrateComboBox.Items.AddRange(Bitrates.Cast<object>().ToArray());
        bitrateComboBox.SelectedItem = _settings.BitrateKbps;
        if (bitrateComboBox.SelectedIndex < 0)
            bitrateComboBox.SelectedItem = 192;

        folderTextBox.Text = _settings.OutputDirectory;

        if (_settings.Mode == DownloadMode.Video)
            videoRadioButton.Checked = true;
        else
            audioRadioButton.Checked = true;
    }

    private DownloadMode SelectedMode => videoRadioButton.Checked ? DownloadMode.Video : DownloadMode.Audio;

    private void modeRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        if (!((RadioButton)sender).Checked)
            return;

        bitrateComboBox.Enabled = SelectedMode == DownloadMode.Audio;

        if (_settings.Mode != SelectedMode)
        {
            _settings.Mode = SelectedMode;
            _settings.Save();
        }
    }

    private void browseButton_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Carpeta donde guardar las descargas",
            UseDescriptionForTitle = true,
            SelectedPath = folderTextBox.Text,
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            folderTextBox.Text = dialog.SelectedPath;
    }

    private async void downloadButton_Click(object sender, EventArgs e)
    {
        var url = urlTextBox.Text.Trim();
        if (url.Length == 0)
        {
            urlTextBox.Focus();
            return;
        }

        var options = new DownloadOptions
        {
            Mode = SelectedMode,
            OutputDirectory = folderTextBox.Text.Trim(),
            BitrateKbps = (int)bitrateComboBox.SelectedItem!,
        };

        _settings.OutputDirectory = options.OutputDirectory;
        _settings.BitrateKbps = options.BitrateKbps;
        _settings.Save();

        using var cts = new CancellationTokenSource();
        _cts = cts;
        _lastFile = null;
        SetBusy(true);

        try
        {
            var result = await _downloader.DownloadAsync(url, options, new Progress<DownloadProgress>(ShowProgress), cts.Token);

            _lastFile = result.FilePath;
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 100;
            ShowStatus(options.Mode == DownloadMode.Audio && !result.CoverEmbedded
                ? $"Guardado sin carátula: {Path.GetFileName(result.FilePath)}"
                : $"Guardado: {Path.GetFileName(result.FilePath)}");
        }
        catch (OperationCanceledException)
        {
            ShowStatus("Cancelado.");
        }
        catch (Exception ex) when (ex is YoutubeExplodeException or FFmpegException or InvalidOperationException
                                       or ArgumentException or IOException or UnauthorizedAccessException)
        {
            ShowStatus(ex.Message, error: true);
        }
        catch (Exception ex)
        {
            ShowStatus("Error inesperado.", error: true);
            MessageBox.Show(this, ex.ToString(), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _cts = null;
            SetBusy(false);
        }
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
        cancelButton.Enabled = false;
        _cts?.Cancel();
    }

    private void openFolderButton_Click(object sender, EventArgs e)
    {
        if (_lastFile is not null && File.Exists(_lastFile))
            Process.Start("explorer.exe", $"/select,\"{_lastFile}\"");
    }

    private void ShowProgress(DownloadProgress progress)
    {
        if (_cts is null)
            return;

        var indeterminate = progress.Stage is DownloadStage.Resolving or DownloadStage.Tagging;

        progressBar.Style = indeterminate ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
        if (!indeterminate)
            progressBar.Value = (int)Math.Round(progress.Fraction * 100);

        var label = progress.Stage switch
        {
            DownloadStage.Resolving => "Resolviendo vídeo",
            DownloadStage.Downloading => "Descargando",
            DownloadStage.Converting => "Convirtiendo a MP3",
            DownloadStage.Merging => "Uniendo vídeo y audio",
            DownloadStage.Tagging => "Escribiendo etiquetas",
            _ => progress.Stage.ToString(),
        };

        ShowStatus(indeterminate ? $"{label}…" : $"{label} {progressBar.Value} %");
    }

    private void ShowStatus(string text, bool error = false)
    {
        statusLabel.Text = text;
        statusLabel.ForeColor = error ? Color.Firebrick : SystemColors.ControlText;
    }

    private void SetBusy(bool busy)
    {
        urlTextBox.Enabled = !busy;
        modePanel.Enabled = !busy;
        folderTextBox.Enabled = !busy;
        browseButton.Enabled = !busy;
        bitrateComboBox.Enabled = !busy && SelectedMode == DownloadMode.Audio;
        downloadButton.Enabled = !busy;
        cancelButton.Enabled = busy;
        openFolderButton.Enabled = !busy && _lastFile is not null;

        if (busy)
        {
            progressBar.Value = 0;
            progressBar.Style = ProgressBarStyle.Marquee;
            ShowStatus("");
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _cts?.Cancel();
        base.OnFormClosing(e);
    }
}
