
//using WinFormsScanSaraff.Twain;
using Saraff.Twain;

namespace WinFormsScanSaraff
{
    public partial class SelectSourceForm : Form
    {
        public SelectSourceForm()
        {
            InitializeComponent();
            this.SourceIndex = -1;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                this.sourceListBox.Items.Clear();
                if (this.Twain != null && this.Twain.SourcesCount > 0)
                {
                    for (int i = 0; i < this.Twain.SourcesCount; i++)
                    {
                        this.sourceListBox.Items.Add(this.Twain.GetSourceProductName(i));
                    }
                    this.sourceListBox.SelectedIndex = this.Twain.SourceIndex;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public Twain32? Twain
        {
            get;
            set;
        }

        public int SourceIndex
        {
            get;
            private set;
        }

        private void selectButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.SourceIndex = this.sourceListBox.SelectedIndex;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
