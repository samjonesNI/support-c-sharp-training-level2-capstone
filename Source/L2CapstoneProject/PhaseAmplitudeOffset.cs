using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L2CapstoneProject
{
    public class PhaseAmplitudeOffset
    {
        public PhaseAmplitudeOffset()
        {
            this.Phase = 0.0;
            this.Amplitude = -10.0;
        }

        public PhaseAmplitudeOffset(double inputPhase, double inputAmplitude)
        {
            this.Phase = inputPhase;
            this.Amplitude = inputAmplitude;
        }

        public double Phase { get; set; }
        
        public double Amplitude { get; set; }
      
    }
}
