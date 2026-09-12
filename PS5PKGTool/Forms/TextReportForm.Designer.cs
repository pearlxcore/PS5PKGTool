#nullable enable

namespace PS5PKGTool.Forms;

partial class TextReportForm
{
    private System.ComponentModel.IContainer? components = null;
    private DarkUI.Controls.DarkRichTextBox txtReport = null!;
    private DarkUI.Controls.DarkButton btnClose = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextReportForm));
        txtReport = new DarkUI.Controls.DarkRichTextBox();
        btnClose = new DarkUI.Controls.DarkButton();
        SuspendLayout();
        // 
        // txtReport
        // 
        txtReport.DetectUrls = false;
        txtReport.Dock = DockStyle.Fill;
        txtReport.Font = new Font("Consolas", 9F);
        txtReport.Location = new Point(0, 0);
        txtReport.Name = "txtReport";
        txtReport.ReadOnly = true;
        txtReport.Size = new Size(760, 428);
        txtReport.TabIndex = 0;
        txtReport.WordWrap = false;
        // 
        // btnClose
        // 
        btnClose.DialogResult = DialogResult.OK;
        btnClose.Dock = DockStyle.Bottom;
        btnClose.Location = new Point(0, 428);
        btnClose.Name = "btnClose";
        btnClose.Size = new Size(760, 32);
        btnClose.TabIndex = 1;
        btnClose.Text = "Close";
        // 
        // TextReportForm
        // 
        AcceptButton = btnClose;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(60, 63, 65);
        CancelButton = btnClose;
        ClientSize = new Size(760, 460);
        Controls.Add(txtReport);
        Controls.Add(btnClose);
        MinimizeBox = false;
        MinimumSize = new Size(520, 320);
        Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        Name = "TextReportForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "PS5 PKG Tool";
        ResumeLayout(false);
    }
}
