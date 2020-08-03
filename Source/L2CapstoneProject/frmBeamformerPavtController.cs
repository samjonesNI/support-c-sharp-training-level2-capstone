using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;
using NationalInstruments.RFmx.InstrMX;
using NationalInstruments.RFmx.SpecAnMX;
using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace L2CapstoneProject
{

    public struct MeasurementConfig
    {
        public double mLength, mOffset, externalAttenuation;
    }
    public struct InstrumentConfig
    {
        public NIRfsg rfsgSession;
        public RFmxInstrMX rfmxSession;
        public string rfsgResourceName, instrResourceName;
        public double frequency, power;
        public List<PhaseAmplitudeOffset> PAOList;
    }

    public struct ResultsLists
    {
        public List<PhaseAmplitudeOffset> PAOResultList;
    }

    public partial class frmBeamformerPavtController : Form
    {
        NIRfsg rfsg;
        RFmxInstrMX instr;
        Beamformer beamformer;
        PavtMeasurement rfmxMeasure;
        MeasurementConfig pavtConfig;
        InstrumentConfig instrConfig;
        ResultsLists beamformerResults;

        public frmBeamformerPavtController()
        {
            InitializeComponent();
            LoadDeviceNames();
           //Initialize config objects
            pavtConfig = new MeasurementConfig();
            instrConfig = new InstrumentConfig();
            beamformerResults = new ResultsLists();
            beamformerResults.PAOResultList = new List<PhaseAmplitudeOffset>();
            instrConfig.PAOList = new List<PhaseAmplitudeOffset>();

            //initiate defaults
            instrConfig.PAOList.Add(new PhaseAmplitudeOffset(0, 0));
            instrConfig.PAOList.Add(new PhaseAmplitudeOffset(0, -5));
            instrConfig.PAOList.Add(new PhaseAmplitudeOffset(0, -10));
            UpdateListBox();
            
        }


        private void LoadDeviceNames()
        {
            ModularInstrumentsSystem rfsgSystem = new ModularInstrumentsSystem("NI-Rfsg");
            foreach (DeviceInfo device in rfsgSystem.DeviceCollection)
                rfsgNameComboBox.Items.Add(device.Name);
            if (rfsgSystem.DeviceCollection.Count > 0)
                rfsgNameComboBox.SelectedIndex = 0;

            ModularInstrumentsSystem rfmxSystem = new ModularInstrumentsSystem("NI-Rfsa");
            foreach (DeviceInfo device in rfmxSystem.DeviceCollection)
                rfsaNameComboBox.Items.Add(device.Name);
            if (rfsgSystem.DeviceCollection.Count > 0)
                rfsaNameComboBox.SelectedIndex = 0;
        }
        #region UI Events
        private void btnAddOffset_Click(object sender, EventArgs e)
        {
            AddOffset();
            
        }

        private void EditListViewItem(object sender, EventArgs e)
        {
            if (CheckSelection(out int selected))
            {
                EditOffset(selected);
                
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (CheckSelection(out int selected))
            {
                RemoveOffset(selected);
            }
        }

        
        
        private void lsvOffsets_KeyDown(object sender, KeyEventArgs e)
        {
            if (CheckSelection(out int selected))
            {
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        EditOffset(selected);
                        break;
                    case Keys.Delete:
                        RemoveOffset(selected);
                        break;
                }
            }
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            SetButtonState(true);
            
            InitializeConfigurations();           
            StartMeasurement();
            StartGeneration();

            UpdateResults();

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            AbortGeneration();
        }

        private void frmBeamformerPavtController_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseInstruments();
        }

        #endregion
        #region Program Functions
        private void AbortGeneration()
        {
            SetButtonState(false);

            if (rfsg?.IsDisposed == false)
            {
                rfsg.Abort();
            }
        }
       
        private void CloseInstruments()
        {
            AbortGeneration();
            rfsg?.Close();

            instr?.Close();
        }
        private void SetButtonState(bool started)
        {
            btnStart.Enabled = !started;
            btnStop.Enabled = started;
        }
        void ShowError(string functionName, Exception exception)
        {
            AbortGeneration();
            errorTextBox.Text = "Error in " + functionName + ": " + exception.Message;
        }
        void SetStatus(string statusMessage)
        {
            errorTextBox.Text = statusMessage;
        }
        #endregion
        #region Offset Functions

        private void AddOffset()
        {
            frmOffset dialog = new frmOffset(frmOffset.Mode.Add);
            DialogResult r = dialog.ShowDialog();

            if (r == DialogResult.OK)
            {
                instrConfig.PAOList.Add(new PhaseAmplitudeOffset(dialog.Phase, dialog.Amplitude));
                UpdateListBox();
            }
        }

        private void EditOffset(int selected)
        {
            // Will need to pass in the currently selected item
            frmOffset dialog = new frmOffset(frmOffset.Mode.Edit);

            DialogResult r = dialog.ShowDialog();

            if (r == DialogResult.OK)
            {
                instrConfig.PAOList[selected].Phase = dialog.Phase;
                instrConfig.PAOList[selected].Amplitude = dialog.Amplitude;
                UpdateListBox();
            }
        }

        private void UpdateListBox()
        {
            lsvOffsets.Items.Clear();
            foreach (PhaseAmplitudeOffset currentPAO in instrConfig.PAOList)
            {
                string subitem1 = currentPAO.Phase.ToString();
                string subitem2 = currentPAO.Amplitude.ToString();
                ListViewItem newItem = new ListViewItem(subitem1);
                newItem.SubItems.Add(subitem2);
                lsvOffsets.Items.Add(newItem);
            }
        }

        private void UpdateResults()
        {
            beamformerResults.PAOResultList = rfmxMeasure.RetrieveResults();
            lsvResults.Items.Clear();
            for (int i = 0; i < beamformerResults.PAOResultList.Count; i++)
            {
                string subitem1 = i.ToString();
                string subitem2 = beamformerResults.PAOResultList[i].Phase.ToString();
                string subitem3 = beamformerResults.PAOResultList[i].Amplitude.ToString();
                ListViewItem newItem = new ListViewItem(subitem1);
                newItem.SubItems.Add(subitem2);
                newItem.SubItems.Add(subitem3);
                lsvResults.Items.Add(newItem);
            }
            
            
        }

        ///
        private void RemoveOffset(int selected)
        {
            lsvOffsets.Items.RemoveAt(selected);
            instrConfig.PAOList.RemoveAt(selected);
            UpdateListBox();
        }
        #endregion
        #region Utility Functions

        /// <summary>
        /// Validates that the listview has at least one value selected. Optionally returns the selected index.
        /// </summary>
        /// <param name="selectedIndex">Current selected index in the list view.</param>
        /// <returns></returns>
        private bool CheckSelection(out int selectedIndex)
        {
            if (lsvOffsets.SelectedItems.Count == 1)
            {
                selectedIndex = lsvOffsets.SelectedIndices[0];
                return true;
            }
            else
            {
                selectedIndex = -1;
                return false;
            }
        }
        void DriverOperation_Warning(object sender, RfsgWarningEventArgs e)
        {
            // Display the rfsg warning
            errorTextBox.Text = e.Message;
        }

        #endregion

        public void InitializeConfigurations()
        {            
            //Update RFSG resources
            instrConfig.rfsgResourceName = rfsgNameComboBox.Text;
            instrConfig.frequency = (double)frequencyNumeric.Value;
            instrConfig.power = (double)powerLevelNumeric.Value;
            instrConfig.rfsgSession = new NIRfsg(instrConfig.rfsgResourceName, true, false);

            //Update RFmx resources
            instrConfig.instrResourceName = rfsaNameComboBox.Text;
            pavtConfig.mLength = (double)measurementLengthNumeric.Value;
            pavtConfig.mOffset = (double)measurementOffsetNumeric.Value;
            pavtConfig.externalAttenuation = 0;
            instrConfig.rfmxSession = new RFmxInstrMX(instrConfig.instrResourceName, "");

        }

        public void StartGeneration()
        {
            beamformer = new SimulatedSteppedBeamformer(instrConfig);
            beamformer.Run();
            beamformer.DisconnectDUT();
        }
        private void StartMeasurement()
        {
            rfmxMeasure = new PavtMeasurement(instrConfig, pavtConfig);
            rfmxMeasure.Run();
        }
    }
}