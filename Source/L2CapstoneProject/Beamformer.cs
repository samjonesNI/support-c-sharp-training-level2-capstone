using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments;



namespace L2CapstoneProject
{
    public abstract class Beamformer
    {
        //Overall flow method that runs the beamformer
        public abstract void Run();
        
        //DUT control methods
        public abstract bool ConnectDUT();
        public abstract void DisconnectDUT();

        //Used to change current pao for stepped beamformer
        public virtual void WriteOffset(PhaseAmplitudeOffset pao, double power) { }
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
            Rfsg.Triggers.StartTrigger.ExportedOutputTerminal = RfsgStartTriggerExportedOutputTerminal.PxiTriggerLine0;
        }

        public NIRfsg Rfsg { get; set; }
        public InstrumentConfig InstrConfig { get; set; }

        public override void Run()
        {
            ConnectDUT();
            Rfsg?.Initiate();
            Rfsg.CheckGenerationStatus();
            GenerateOffsets();
            DisconnectDUT();


        }

        

        public override bool ConnectDUT()
        {
            Rfsg = new NIRfsg(InstrConfig.rfsgResourceName, true, false);
            Rfsg.RF.Configure(InstrConfig.frequency, InstrConfig.power);
            // TODO: Add this code back to the main form so that warnings are properly wired through
            //Rfsg.DriverOperation.Warning += new EventHandler<RfsgWarningEventArgs>(DriverOperation_Warning);
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

        public void WriteOffset(PhaseAmplitudeOffset pao)
        {
            Rfsg.Abort();
            Rfsg.Initiate();
            Rfsg.RF.PowerLevel = InstrConfig.power + pao.Amplitude;
            Rfsg.RF.PhaseOffset = pao.Phase;
        }
        
        public void GenerateOffsets()
        {
            foreach (PhaseAmplitudeOffset pao in InstrConfig.PAOList)
            {
                WriteOffset(pao);
                //measure();
                System.Threading.Thread.Sleep(1000);
            }
        }
        
    }
}
