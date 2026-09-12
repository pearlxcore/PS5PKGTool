using System.Drawing;
using System.Windows.Forms;

namespace PS5PKGTool.Forms;

partial class AboutForm
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
        picAppIcon = new PictureBox();
        lblTitle = new DarkUI.Controls.DarkLabel();
        lblVersion = new DarkUI.Controls.DarkLabel();
        lblCopyright = new DarkUI.Controls.DarkLabel();
        lblCredits = new DarkUI.Controls.DarkLabel();
        lblLicense = new DarkUI.Controls.DarkLabel();
        btnGitHub = new DarkUI.Controls.DarkButton();
        btnKofi = new DarkUI.Controls.DarkButton();
        btnBug = new DarkUI.Controls.DarkButton();
        btnClose = new DarkUI.Controls.DarkButton();
        ((System.ComponentModel.ISupportInitialize)picAppIcon).BeginInit();
        SuspendLayout();
        // 
        // picAppIcon
        // 
        picAppIcon.Location = new Point(20, 20);
        picAppIcon.Name = "picAppIcon";
        picAppIcon.Size = new Size(64, 64);
        picAppIcon.SizeMode = PictureBoxSizeMode.Zoom;
        picAppIcon.TabIndex = 0;
        picAppIcon.TabStop = false;
        // 
        // lblTitle
        // 
        lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTitle.Location = new Point(100, 18);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(300, 26);
        lblTitle.TabIndex = 9;
        lblTitle.Text = "PS5 PKG Tool";
        // 
        // lblVersion
        // 
        lblVersion.Font = new Font("Segoe UI", 9F);
        lblVersion.Location = new Point(100, 46);
        lblVersion.Name = "lblVersion";
        lblVersion.Size = new Size(300, 20);
        lblVersion.TabIndex = 8;
        lblVersion.Text = "Version 1.0.0";
        // 
        // lblCopyright
        // 
        lblCopyright.Font = new Font("Segoe UI", 9F);
        lblCopyright.Location = new Point(100, 66);
        lblCopyright.Name = "lblCopyright";
        lblCopyright.Size = new Size(300, 20);
        lblCopyright.TabIndex = 7;
        lblCopyright.Text = "Copyright (c) pearlxcore";
        // 
        // lblCredits
        // 
        lblCredits.Font = new Font("Segoe UI", 9F);
        lblCredits.Location = new Point(20, 104);
        lblCredits.Name = "lblCredits";
        lblCredits.Size = new Size(380, 46);
        lblCredits.TabIndex = 6;
        lblCredits.Text = "Credit to Robin Perris, SvenGDK, PSBrew, Renan Barreto,\r\nkerrdec97, strongt1me, Drakmor, Sony <3";
        // 
        // lblLicense
        // 
        lblLicense.Font = new Font("Segoe UI", 9F);
        lblLicense.Location = new Point(20, 152);
        lblLicense.Name = "lblLicense";
        lblLicense.Size = new Size(380, 20);
        lblLicense.TabIndex = 5;
        lblLicense.Text = "Licensed under the GNU General Public License v3.0";
        // 
        // btnGitHub
        // 
        btnGitHub.Font = new Font("Segoe UI", 9F);
        btnGitHub.Location = new Point(20, 182);
        btnGitHub.Name = "btnGitHub";
        btnGitHub.Size = new Size(85, 30);
        btnGitHub.TabIndex = 1;
        btnGitHub.Text = "GitHub";
        btnGitHub.Click += btnGitHub_Click;
        // 
        // btnKofi
        // 
        btnKofi.Font = new Font("Segoe UI", 9F);
        btnKofi.Location = new Point(112, 182);
        btnKofi.Name = "btnKofi";
        btnKofi.Size = new Size(85, 30);
        btnKofi.TabIndex = 2;
        btnKofi.Text = "Ko-fi";
        btnKofi.Click += btnKofi_Click;
        // 
        // btnBug
        // 
        btnBug.Font = new Font("Segoe UI", 9F);
        btnBug.Location = new Point(204, 182);
        btnBug.Name = "btnBug";
        btnBug.Size = new Size(95, 30);
        btnBug.TabIndex = 3;
        btnBug.Text = "Report Bug";
        btnBug.Click += btnBug_Click;
        // 
        // btnClose
        // 
        btnClose.Font = new Font("Segoe UI", 9F);
        btnClose.Location = new Point(306, 182);
        btnClose.Name = "btnClose";
        btnClose.Size = new Size(85, 30);
        btnClose.TabIndex = 4;
        btnClose.Text = "Close";
        btnClose.Click += btnClose_Click;
        // 
        // AboutForm
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(411, 224);
        Controls.Add(btnClose);
        Controls.Add(btnBug);
        Controls.Add(btnKofi);
        Controls.Add(btnGitHub);
        Controls.Add(lblLicense);
        Controls.Add(lblCredits);
        Controls.Add(lblCopyright);
        Controls.Add(lblVersion);
        Controls.Add(lblTitle);
        Controls.Add(picAppIcon);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        Icon = (Icon)resources.GetObject("$this.Icon");
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "AboutForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "About PS5 PKG Tool";
        ((System.ComponentModel.ISupportInitialize)picAppIcon).EndInit();
        ResumeLayout(false);
    }

    private PictureBox picAppIcon = null!;
    private DarkUI.Controls.DarkLabel lblTitle = null!;
    private DarkUI.Controls.DarkLabel lblVersion = null!;
    private DarkUI.Controls.DarkLabel lblCopyright = null!;
    private DarkUI.Controls.DarkLabel lblCredits = null!;
    private DarkUI.Controls.DarkLabel lblLicense = null!;
    private DarkUI.Controls.DarkButton btnGitHub = null!;
    private DarkUI.Controls.DarkButton btnKofi = null!;
    private DarkUI.Controls.DarkButton btnBug = null!;
    private DarkUI.Controls.DarkButton btnClose = null!;
}
