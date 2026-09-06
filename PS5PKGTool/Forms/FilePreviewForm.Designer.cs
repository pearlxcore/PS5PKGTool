#nullable enable

namespace PS5PKGTool.Forms;

partial class FilePreviewForm
{
    private System.ComponentModel.IContainer? components = null;
    private Panel previewPanel = null!;
    private PictureBox picturePreview = null!;
    private DarkUI.Controls.DarkTextBox textPreview = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            picturePreview?.Image?.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        previewPanel = new Panel();
        picturePreview = new PictureBox();
        textPreview = new DarkUI.Controls.DarkTextBox();
        previewPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picturePreview).BeginInit();
        SuspendLayout();
        // 
        // previewPanel
        // 
        previewPanel.BackColor = Color.FromArgb(37, 37, 38);
        previewPanel.Controls.Add(textPreview);
        previewPanel.Controls.Add(picturePreview);
        previewPanel.Dock = DockStyle.Fill;
        previewPanel.Name = "previewPanel";
        previewPanel.Padding = new Padding(8);
        previewPanel.TabIndex = 0;
        // 
        // picturePreview
        // 
        picturePreview.BackColor = Color.FromArgb(20, 20, 20);
        picturePreview.Dock = DockStyle.Fill;
        picturePreview.Name = "picturePreview";
        picturePreview.SizeMode = PictureBoxSizeMode.Zoom;
        picturePreview.TabIndex = 0;
        picturePreview.TabStop = false;
        // 
        // textPreview
        // 
        textPreview.Dock = DockStyle.Fill;
        textPreview.Font = new Font("Consolas", 10F);
        textPreview.Multiline = true;
        textPreview.Name = "textPreview";
        textPreview.ReadOnly = true;
        textPreview.ScrollBars = ScrollBars.Both;
        textPreview.TabIndex = 1;
        textPreview.Visible = false;
        textPreview.WordWrap = false;
        // 
        // FilePreviewForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(960, 680);
        Controls.Add(previewPanel);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;
        MinimumSize = new Size(520, 380);
        Name = "FilePreviewForm";
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "File Preview";
        Controls.SetChildIndex(previewPanel, 0);
        previewPanel.ResumeLayout(false);
        previewPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picturePreview).EndInit();
        ResumeLayout(false);
    }
}
