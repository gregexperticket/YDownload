namespace YDownload.WinForms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        layout = new TableLayoutPanel();
        urlLabel = new Label();
        urlTextBox = new TextBox();
        modePanel = new FlowLayoutPanel();
        videoRadioButton = new RadioButton();
        audioRadioButton = new RadioButton();
        folderLabel = new Label();
        folderTextBox = new TextBox();
        browseButton = new Button();
        bitrateLabel = new Label();
        bitratePanel = new FlowLayoutPanel();
        bitrateComboBox = new ComboBox();
        kbpsLabel = new Label();
        buttonsPanel = new FlowLayoutPanel();
        downloadButton = new Button();
        cancelButton = new Button();
        progressBar = new ProgressBar();
        statusLabel = new Label();
        openFolderButton = new Button();
        layout.SuspendLayout();
        modePanel.SuspendLayout();
        bitratePanel.SuspendLayout();
        buttonsPanel.SuspendLayout();
        SuspendLayout();
        //
        // layout
        //
        layout.ColumnCount = 3;
        layout.ColumnStyles.Add(new ColumnStyle());
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle());
        layout.Controls.Add(urlLabel, 0, 0);
        layout.Controls.Add(urlTextBox, 1, 0);
        layout.Controls.Add(modePanel, 1, 1);
        layout.Controls.Add(folderLabel, 0, 2);
        layout.Controls.Add(folderTextBox, 1, 2);
        layout.Controls.Add(browseButton, 2, 2);
        layout.Controls.Add(bitrateLabel, 0, 3);
        layout.Controls.Add(bitratePanel, 1, 3);
        layout.Controls.Add(buttonsPanel, 0, 4);
        layout.Controls.Add(progressBar, 0, 5);
        layout.Controls.Add(statusLabel, 0, 6);
        layout.Controls.Add(openFolderButton, 2, 6);
        layout.Dock = DockStyle.Fill;
        layout.Location = new Point(0, 0);
        layout.Name = "layout";
        layout.Padding = new Padding(12, 12, 12, 8);
        layout.RowCount = 7;
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle());
        layout.Size = new Size(584, 246);
        layout.TabIndex = 0;
        //
        // urlLabel
        //
        urlLabel.Anchor = AnchorStyles.Left;
        urlLabel.AutoSize = true;
        urlLabel.Name = "urlLabel";
        urlLabel.Text = "Vídeo";
        //
        // urlTextBox
        //
        urlTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        layout.SetColumnSpan(urlTextBox, 2);
        urlTextBox.Name = "urlTextBox";
        urlTextBox.PlaceholderText = "URL o id del vídeo de YouTube";
        urlTextBox.TabIndex = 0;
        //
        // modePanel
        //
        modePanel.AutoSize = true;
        layout.SetColumnSpan(modePanel, 2);
        modePanel.Controls.Add(videoRadioButton);
        modePanel.Controls.Add(audioRadioButton);
        modePanel.Margin = new Padding(0);
        modePanel.Name = "modePanel";
        modePanel.TabIndex = 1;
        modePanel.WrapContents = false;
        //
        // videoRadioButton
        //
        videoRadioButton.AutoSize = true;
        videoRadioButton.Margin = new Padding(3, 3, 16, 3);
        videoRadioButton.Name = "videoRadioButton";
        videoRadioButton.TabIndex = 0;
        videoRadioButton.Text = "Descargar vídeo completo";
        videoRadioButton.CheckedChanged += modeRadioButton_CheckedChanged;
        //
        // audioRadioButton
        //
        audioRadioButton.AutoSize = true;
        audioRadioButton.Name = "audioRadioButton";
        audioRadioButton.TabIndex = 1;
        audioRadioButton.Text = "Descargar solo audio";
        audioRadioButton.CheckedChanged += modeRadioButton_CheckedChanged;
        //
        // folderLabel
        //
        folderLabel.Anchor = AnchorStyles.Left;
        folderLabel.AutoSize = true;
        folderLabel.Name = "folderLabel";
        folderLabel.Text = "Carpeta";
        //
        // folderTextBox
        //
        folderTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        folderTextBox.Name = "folderTextBox";
        folderTextBox.TabIndex = 2;
        //
        // browseButton
        //
        browseButton.Anchor = AnchorStyles.Left;
        browseButton.AutoSize = true;
        browseButton.Name = "browseButton";
        browseButton.Padding = new Padding(6, 0, 6, 0);
        browseButton.TabIndex = 3;
        browseButton.Text = "Examinar…";
        browseButton.Click += browseButton_Click;
        //
        // bitrateLabel
        //
        bitrateLabel.Anchor = AnchorStyles.Left;
        bitrateLabel.AutoSize = true;
        bitrateLabel.Name = "bitrateLabel";
        bitrateLabel.Text = "Bitrate";
        //
        // bitratePanel
        //
        bitratePanel.AutoSize = true;
        bitratePanel.Controls.Add(bitrateComboBox);
        bitratePanel.Controls.Add(kbpsLabel);
        bitratePanel.Margin = new Padding(0);
        bitratePanel.Name = "bitratePanel";
        bitratePanel.TabIndex = 4;
        bitratePanel.WrapContents = false;
        //
        // bitrateComboBox
        //
        bitrateComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        bitrateComboBox.Name = "bitrateComboBox";
        bitrateComboBox.Size = new Size(80, 23);
        bitrateComboBox.TabIndex = 0;
        //
        // kbpsLabel
        //
        kbpsLabel.Anchor = AnchorStyles.Left;
        kbpsLabel.AutoSize = true;
        kbpsLabel.Name = "kbpsLabel";
        kbpsLabel.Text = "kbps";
        //
        // buttonsPanel
        //
        buttonsPanel.AutoSize = true;
        layout.SetColumnSpan(buttonsPanel, 3);
        buttonsPanel.Controls.Add(downloadButton);
        buttonsPanel.Controls.Add(cancelButton);
        buttonsPanel.Margin = new Padding(0, 8, 0, 0);
        buttonsPanel.Name = "buttonsPanel";
        buttonsPanel.TabIndex = 5;
        buttonsPanel.WrapContents = false;
        //
        // downloadButton
        //
        downloadButton.Name = "downloadButton";
        downloadButton.Size = new Size(110, 30);
        downloadButton.TabIndex = 0;
        downloadButton.Text = "Descargar";
        downloadButton.Click += downloadButton_Click;
        //
        // cancelButton
        //
        cancelButton.Enabled = false;
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(110, 30);
        cancelButton.TabIndex = 1;
        cancelButton.Text = "Cancelar";
        cancelButton.Click += cancelButton_Click;
        //
        // progressBar
        //
        progressBar.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        layout.SetColumnSpan(progressBar, 3);
        progressBar.Margin = new Padding(3, 12, 3, 3);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(554, 18);
        //
        // statusLabel
        //
        statusLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        statusLabel.AutoEllipsis = true;
        layout.SetColumnSpan(statusLabel, 2);
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(440, 23);
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // openFolderButton
        //
        openFolderButton.Anchor = AnchorStyles.Right;
        openFolderButton.AutoSize = true;
        openFolderButton.Enabled = false;
        openFolderButton.Name = "openFolderButton";
        openFolderButton.Padding = new Padding(6, 0, 6, 0);
        openFolderButton.TabIndex = 6;
        openFolderButton.Text = "Abrir carpeta";
        openFolderButton.Click += openFolderButton_Click;
        //
        // MainForm
        //
        AcceptButton = downloadButton;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(584, 246);
        Controls.Add(layout);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "YDownload";
        layout.ResumeLayout(false);
        layout.PerformLayout();
        modePanel.ResumeLayout(false);
        modePanel.PerformLayout();
        bitratePanel.ResumeLayout(false);
        bitratePanel.PerformLayout();
        buttonsPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel layout;
    private Label urlLabel;
    private TextBox urlTextBox;
    private FlowLayoutPanel modePanel;
    private RadioButton videoRadioButton;
    private RadioButton audioRadioButton;
    private Label folderLabel;
    private TextBox folderTextBox;
    private Button browseButton;
    private Label bitrateLabel;
    private FlowLayoutPanel bitratePanel;
    private ComboBox bitrateComboBox;
    private Label kbpsLabel;
    private FlowLayoutPanel buttonsPanel;
    private Button downloadButton;
    private Button cancelButton;
    private ProgressBar progressBar;
    private Label statusLabel;
    private Button openFolderButton;
}
