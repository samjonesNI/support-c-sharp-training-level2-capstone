using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments;
using System.Windows.Forms;
using NationalInstruments.RFmx.SpecAnMX;
using NationalInstruments.RFmx.InstrMX;

namespace L2CapstoneProject
{
    public abstract class Beamformer
    {
        //Overall flow method that runs the beamformer
        public abstract void Run();
        
        //DUT control methods
        public abstract bool ConnectDUT();
        public abstract void DisconnectDUT();

        //Should implement as interfaces to have more intuative inheritance
        //Used to change current pao for stepped beamformer
        public virtual void WriteOffset(PhaseAmplitudeOffset pao) { }
        //Loads a list of pao's into memory for the sequenced beamformer
        public virtual void WriteSequence(List<PhaseAmplitudeOffset> paoList){ }
        public virtual void InitiateSequence() { }
        public virtual void AbortSequence() { }

        //Changes the duration of each subsequence when using sequenced beamformer
        public decimal SubsequenceLength { get; set; }
    }

    public class SimulatedSteppedBeamformer : Beamformer
    {
        public SimulatedSteppedBeamformer(InstrumentConfig instrConfig)
        {
            InstrConfig = instrConfig;

        }

        public NIRfsg Rfsg { get; set; }
        public RFmxSpecAnMX SpecAn {get; set;}
        public InstrumentConfig InstrConfig { get; set; }

        public ComplexWaveform<ComplexDouble> complexWaveform { get; set; }

        public override void Run()
        {
            try
            {
                ConnectDUT();
                //Rfsg?.Initiate();
                Rfsg.CheckGenerationStatus();
                GenerateOffsets();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.GetType().ToString() + "\n" + ex.StackTrace, "Exception");
            }
            finally
            {
                /* Close session */
                DisconnectDUT();
            }
        }

        public override bool ConnectDUT()
        {
            //InstrConfig.rfsgSession = new NIRfsg(InstrConfig.rfsgResourceName, true, false);
            Rfsg = InstrConfig.rfsgSession;
            Rfsg.RF.Configure(InstrConfig.frequency, InstrConfig.power);
            Rfsg.Triggers.StartTrigger.ExportedOutputTerminal = RfsgStartTriggerExportedOutputTerminal.PxiTriggerLine0;
            // TODO: Add this code back to the main form so that warnings are properly wired through
            //Rfsg.DriverOperation.Warning += new EventHandler<RfsgWarningEventArgs>(DriverOperation_Warning);
            SpecAn = InstrConfig.rfmxSession.GetSpecAnSignalConfiguration("Stepped");

            return true;
            
        }

        public override void DisconnectDUT()
        {
            if (Rfsg?.IsDisposed == false)
            {
                Rfsg.Abort();
            }
            //Close session
        }

        public override void WriteOffset(PhaseAmplitudeOffset pao)
        {
            //Rfsg.Abort();
            Rfsg.RF.PowerLevel = InstrConfig.power + pao.Amplitude;
            Rfsg.RF.PhaseOffset = pao.Phase;
            Rfsg.Utility.WaitUntilSettled(5);
            
            SpecAn.SendSoftwareEdgeTrigger();
           //Rfsg.Initiate();
        }
        
        public void GenerateOffsets()
        {
            foreach (PhaseAmplitudeOffset pao in InstrConfig.PAOList)
            {
                WriteOffset(pao);
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
