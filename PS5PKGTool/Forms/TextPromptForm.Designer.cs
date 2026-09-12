#nullable enable

namespace PS5PKGTool.Forms;

partial class TextPromptForm
{
    private System.ComponentModel.IContainer? components = null;
    private DarkUI.Controls.DarkLabel lblPrompt = null!;
    private DarkUI.Controls.DarkTextBox txtValue = null!;
    private DarkUI.Controls.DarkButton btnOk = null!;
    private DarkUI.Controls.DarkButton btnCancel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextPromptForm));
        lblPrompt = new DarkUI.Controls.DarkLabel();
        txtValue = new DarkUI.Controls.DarkTextBox();
        btnOk = new DarkUI.Controls.DarkButton();
        btnCancel = new DarkUI.Controls.DarkButton();
        SuspendLayout();
        // 
        // lblPrompt
        // 
        lblPrompt.AutoSize = true;
        lblPrompt.Location = new Point(12, 12);
        lblPrompt.Name = "lblPrompt";
        lblPrompt.Size = new Size(60, 15);
        lblPrompt.TabIndex = 0;
        lblPrompt.Text = "Enter a value:";
        // 
        // txtValue
        // 
        txtValue.Location = new Point(12, 40);
        txtValue.Name = "txtValue";
        txtValue.Size = new Size(436, 23);
        txtValue.TabIndex = 1;
        // 
        // btnOk
        // 
        btnOk.DialogResult = DialogResult.OK;
        btnOk.Location = new Point(292, 82);
        btnOk.Name = "btnOk";
        btnOk.Size = new Size(76, 28);
        btnOk.TabIndex = 2;
        btnOk.Text = "OK";
        // 
        // btnCancel
        // 
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Location = new Point(374, 82);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(76, 28);
        btnCancel.TabIndex = 3;
        btnCancel.Text = "Cancel";
        // 
        // TextPromptForm
        // 
        AcceptButton = btnOk;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(60, 63, 65);
        CancelButton = btnCancel;
        ClientSize = new Size(460, 150);
        Controls.Add(lblPrompt);
        Controls.Add(txtValue);
        Controls.Add(btnOk);
        Controls.Add(btnCancel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        Name = "TextPromptForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "PS5 PKG Tool";
        ResumeLayout(false);
        PerformLayout();
    }
}
