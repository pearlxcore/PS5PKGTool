using System.Drawing;
using System.Windows.Forms;

namespace PS5PKGTool.Forms;

partial class AppMessageBox
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppMessageBox));
        lblTitle = new DarkUI.Controls.DarkLabel();
        lblMessage = new DarkUI.Controls.DarkLabel();
        btnOK = new DarkUI.Controls.DarkButton();
        btnYes = new DarkUI.Controls.DarkButton();
        btnNo = new DarkUI.Controls.DarkButton();
        btnCancel = new DarkUI.Controls.DarkButton();
        btnCopy = new DarkUI.Controls.DarkButton();
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblTitle.Location = new Point(20, 15);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(420, 22);
        // 
        // lblMessage
        // 
        lblMessage.Font = new Font("Segoe UI", 9F);
        lblMessage.Location = new Point(20, 42);
        lblMessage.Name = "lblMessage";
        lblMessage.Size = new Size(420, 60);
        // 
        // btnOK
        // 
        btnOK.Font = new Font("Segoe UI", 9F);
        btnOK.Name = "btnOK";
        btnOK.Padding = new Padding(5);
        btnOK.Size = new Size(100, 30);
        btnOK.TabIndex = 0;
        btnOK.Text = "OK";
        btnOK.Visible = false;
        btnOK.Click += btnOK_Click;
        // 
        // btnYes
        // 
        btnYes.Font = new Font("Segoe UI", 9F);
        btnYes.Name = "btnYes";
        btnYes.Padding = new Padding(5);
        btnYes.Size = new Size(100, 30);
        btnYes.TabIndex = 1;
        btnYes.Text = "Yes";
        btnYes.Visible = false;
        btnYes.Click += btnYes_Click;
        // 
        // btnNo
        // 
        btnNo.Font = new Font("Segoe UI", 9F);
        btnNo.Name = "btnNo";
        btnNo.Padding = new Padding(5);
        btnNo.Size = new Size(100, 30);
        btnNo.TabIndex = 2;
        btnNo.Text = "No";
        btnNo.Visible = false;
        btnNo.Click += btnNo_Click;
        // 
        // btnCancel
        // 
        btnCancel.Font = new Font("Segoe UI", 9F);
        btnCancel.Name = "btnCancel";
        btnCancel.Padding = new Padding(5);
        btnCancel.Size = new Size(100, 30);
        btnCancel.TabIndex = 3;
        btnCancel.Text = "Cancel";
        btnCancel.Visible = false;
        btnCancel.Click += btnCancel_Click;
        // 
        // btnCopy
        // 
        btnCopy.Font = new Font("Segoe UI", 9F);
        btnCopy.Name = "btnCopy";
        btnCopy.Padding = new Padding(5);
        btnCopy.Size = new Size(100, 30);
        btnCopy.TabIndex = 4;
        btnCopy.Text = "Copy";
        btnCopy.Visible = false;
        btnCopy.Click += btnCopy_Click;
        // 
        // AppMessageBox
        // 
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(460, 150);
        Controls.Add(btnCancel);
        Controls.Add(btnNo);
        Controls.Add(btnYes);
        Controls.Add(btnCopy);
        Controls.Add(btnOK);
        Controls.Add(lblMessage);
        Controls.Add(lblTitle);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        Name = "AppMessageBox";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ResumeLayout(false);
    }

    private DarkUI.Controls.DarkLabel lblTitle = null!;
    private DarkUI.Controls.DarkLabel lblMessage = null!;
    private DarkUI.Controls.DarkButton btnOK = null!;
    private DarkUI.Controls.DarkButton btnYes = null!;
    private DarkUI.Controls.DarkButton btnNo = null!;
    private DarkUI.Controls.DarkButton btnCancel = null!;
    private DarkUI.Controls.DarkButton btnCopy = null!;
}
