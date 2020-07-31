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
        public string rfsgResourceName, instrResourceName;
        public double frequency, power;
        public List<PhaseAmplitudeOffset> PAOList;
    }
    public partial class frmBeamformerPavtController : Form
    {
        NIRfsg rfsg;
        RFmxInstrMX instr;
        private List<PhaseAmplitudeOffset> PAOResultList = new List<PhaseAmplitudeOffset>();
        Beamformer beamformer;
        RFmxSpecAnMX specAn;
        MeasurementConfig pavtConfig;
        InstrumentConfig instrConfig;

        public frmBeamformerPavtController()
        {
            InitializeComponent();
            LoadDeviceNames();
        }

        #region Type Defenitions
       
        #endregion
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
        private void lsvOffsets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CheckSelection(out int _))
            {
                btnDeleteOffset.Enabled = btnEditOffset.Enabled = true;
            }
            else
            {
                btnDeleteOffset.Enabled = btnEditOffset.Enabled = false;
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
            InitializeInstruments();
            StartGeneration();
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

            //Update RFSA resources
            instrConfig.instrResourceName = rfsaNameComboBox.Text;
            pavtConfig.mLength = (double)measurementLengthNumeric.Value;
            pavtConfig.mOffset = (double)measurementOffsetNumeric.Value;
            pavtConfig.externalAttenuation = 0;
        }
            public void InitializeInstruments()
        {          
            //Create and configure RFSA session
            instr = new RFmxInstrMX(instrConfig.instrResourceName, "");
            specAn = instr.GetSpecAnSignalConfiguration();
            instr.ConfigureFrequencyReference("", RFmxInstrMXConstants.OnboardClock, 10.0e6);
            specAn.ConfigureRF("", instrConfig.frequency, instrConfig.power, pavtConfig.externalAttenuation);
            specAn.ConfigureDigitalEdgeTrigger("", RFmxSpecAnMXConstants.PxiTriggerLine0, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising,0, true);

            //simulated DUT
            beamformer = new SimulatedSteppedBeamformer(rfsg);   
            
            //Any beamformer
            beamformer.ConnectDUT();
        }

        public void StartGeneration()
        {
            beamformer.Run();
            //This code only needs to exist until measurement class is created
            beamformer.StimulateDUT();
            //Replace StimulateDUT with the following code
            //measure.StimulateDUT();
            
            beamformer.DisconnectDUT();
        }

        
    }
}