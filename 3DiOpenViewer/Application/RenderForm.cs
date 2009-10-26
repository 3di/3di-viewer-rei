using System.Windows.Forms;

namespace OpenViewer
{
    public class RenderForm : Form
    {
        private Viewer viewer = null;

        public RenderForm(Viewer viewer)
        {
            this.viewer = viewer;
            this.Cursor = null;
        }

        protected override bool CanEnableIme { get { return true; } }
    }
}
