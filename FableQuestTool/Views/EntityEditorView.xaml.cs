using FableQuestTool.ViewModels;

namespace FableQuestTool.Views;

public partial class EntityEditorView : System.Windows.Controls.UserControl
{
    public EntityEditorView()
    {
        InitializeComponent();
        DataContext = new EntityEditorViewModel();
    }
}
