using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L2CapstoneProject
{
    public abstract class Beamformer
    {
        //DUT control methods
        public abstract void ConnectDUT();
        public abstract void StimulateDUT();
        public abstract void DisconnectDUT();

        //Used to change current pao for stepped beamformer
        public abstract void WriteOffset(PhaseAmplitudeOffset pao);

        //Loads a list of pao's into memory for the sequenced beamformer
        public abstract void WriteSequence(List<PhaseAmplitudeOffset> paoList);
        public abstract void InitiateSequence();
        public abstract void AbortSequence();

        //Changes the duration of each subsequence when using sequenced beamformer
        public decimal SubsequenceLength { get; set; }
    }
    public class SimulatedBeamformer : Beamformer
    {                    
        public override void ConnectDUT()
        {
            //Simulated DUT, no connections
        }

        public override void DisconnectDUT()
        {
            //Simulated DUT, no connections
        }

        public override void InitiateSequence()
        {
            throw new NotImplementedException();
        }

        public override void StimulateDUT()
        {
            string resourceName;
            double frequency, amplitude;

            try
            {
                resourceName = rfsgNameComboBox
            }
        }

        public override void WriteOffset(PhaseAmplitudeOffset pao)
        {
            throw new NotImplementedException();
        }

        public override void WriteSequence(List<PhaseAmplitudeOffset> paoList)
        {
            throw new NotImplementedException();
        }
        public override void AbortSequence()
        {
            throw new NotImplementedException();
        }
    }
}
