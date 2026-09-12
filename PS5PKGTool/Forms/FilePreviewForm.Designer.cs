#nullable enable

namespace PS5PKGTool.Forms;

partial class FilePreviewForm
{
    private System.ComponentModel.IContainer? components = null;
    private PictureBox picturePreview = null!;
    private DarkUI.Controls.DarkRichTextBox textPreview = null!;

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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilePreviewForm));
        components = new System.ComponentModel.Container();
        picturePreview = new PictureBox();
        textPreview = new DarkUI.Controls.DarkRichTextBox();
        ((System.ComponentModel.ISupportInitialize)picturePreview).BeginInit();
        SuspendLayout();
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
        textPreview.DetectUrls = false;
        textPreview.Dock = DockStyle.Fill;
        textPreview.Font = new Font("Consolas", 10F);
        textPreview.Name = "textPreview";
        textPreview.ReadOnly = true;
        textPreview.TabIndex = 1;
        textPreview.Visible = false;
        textPreview.WordWrap = false;
        // 
        // FilePreviewForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(960, 680);
        Padding = new Padding(8);
        Controls.Add(textPreview);
        Controls.Add(picturePreview);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;
        MinimumSize = new Size(520, 380);
        Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        Name = "FilePreviewForm";
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "File Preview";
        ((System.ComponentModel.ISupportInitialize)picturePreview).EndInit();
        ResumeLayout(false);
    }
}
