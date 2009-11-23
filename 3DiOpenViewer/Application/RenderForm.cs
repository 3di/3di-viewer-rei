using System.Windows.Forms;

namespace OpenViewer
{
    public class RenderForm : Form
    {
        public RenderForm()
        {
            this.Cursor = null;
        }

        protected override bool CanEnableIme { get { return true; } }
    }
}
