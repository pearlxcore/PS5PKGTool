using DarkUI.Forms;

namespace PS5PKGTool.Forms;

public partial class TextPromptForm : DarkForm
{
    public TextPromptForm()
    {
        InitializeComponent();
    }

    public TextPromptForm(string title, string prompt, string initial) : this()
    {
        Text = title;
        lblPrompt.Text = prompt;
        txtValue.Text = initial;
        txtValue.SelectAll();
    }

    public string Value => txtValue.Text;
}
