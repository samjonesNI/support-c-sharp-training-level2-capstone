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
        //DUT control methods
        public abstract bool ConnectDUT();
        public abstract void DisconnectDUT();

        //Used to change current pao for stepped beamformer
        public virtual void WriteOffset(PhaseAmplitudeOffset pao, double power) { }
        public abstract void StimulateDUT();
        //Loads a list of pao's into memory for the sequenced beamformer
        public virtual void WriteSequence(List<PhaseAmplitudeOffset> paoList){ }
        public virtual void InitiateSequence() { }
        public virtual void AbortSequence() { }

        //Changes the duration of each subsequence when using sequenced beamformer
        public decimal SubsequenceLength { get; set; }
    }
    public class SimulatedSteppedBeamformer : Beamformer
    {
        public SimulatedSteppedBeamformer(NIRfsg rfsg)
        {
            Rfsg = rfsg;
            Rfsg.Triggers.StartTrigger.ExportedOutputTerminal = RfsgStartTriggerExportedOutputTerminal.PxiTriggerLine0;
        }

        public NIRfsg Rfsg { get; set; }

        public override bool ConnectDUT()
        {
            //Open session, set up configuration
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

        public override void StimulateDUT()
        {
            //Begin generation of CW
            
            Rfsg?.Initiate();
            Rfsg.CheckGenerationStatus();

        }

        public override void WriteOffset(PhaseAmplitudeOffset pao, double power)
        {
            Rfsg.Abort();
            Rfsg.Initiate();
            Rfsg.RF.PowerLevel = power + pao.Amplitude;
            Rfsg.RF.PhaseOffset = pao.Phase;
        }      
        
    }
}
