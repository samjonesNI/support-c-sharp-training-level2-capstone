using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments;
using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments.RFmx.InstrMX;
using NationalInstruments.RFmx.SpecAnMX;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;

namespace L2CapstoneProject
{
    class PavtMeasurement
    {
        public PavtMeasurement(NIRfsg rfsg, RFmxInstrMX instr)
        {
            Rfsg = rfsg;
            Instr = instr;
        }

        public NIRfsg Rfsg { get; set; }
        public RFmxInstrMX Instr { get; set; }

        public void StimulateDUT()
        {
            //Begin generation of CW

            Rfsg.Initiate();
            Rfsg.CheckGenerationStatus();

        }
    }
}
