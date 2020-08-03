using System;
using System.Windows.Forms;

namespace L2CapstoneProject
{
    public partial class frmOffset : Form
    {
        public enum Mode { Add, Edit }
        public double Phase { get; set; }
        public double Amplitude { get; set; }

        public Mode ViewMode { get; }

        public frmOffset(Mode mode)
        {
            InitializeComponent();
            ViewMode = mode;

            switch (ViewMode)
            {
                case Mode.Add:
                    this.Text = "Add New Offset";
                    break;
                case Mode.Edit:
                    this.Text = "Edit Offset";                                     
                    break;
            }
        }

        public frmOffset(Mode mode, double phase, double amp)
        {
            InitializeComponent();
            ViewMode = mode;

            switch (ViewMode)
            {
                case Mode.Add:
                    this.Text = "Add New Offset";
                    break;
                case Mode.Edit:
                    this.Text = "Edit Offset";
                    numAmp.Value = (decimal)amp;
                    numPhase.Value = (decimal)phase;
                    break;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                Amplitude = Convert.ToDouble(numAmp.Value);
                Phase = Convert.ToDouble(numPhase.Value);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Incorrect value: " + ex);
            }
            Close();
        }

        private void frmOffset_Load(object sender, EventArgs e)
        {

        }
    }
}
