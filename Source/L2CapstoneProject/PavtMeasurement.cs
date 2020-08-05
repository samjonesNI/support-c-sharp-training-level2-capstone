using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NationalInstruments;
using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments.RFmx.InstrMX;
using NationalInstruments.RFmx.SpecAnMX;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;

namespace L2CapstoneProject
{
    class PavtMeasurement
    {
        public PavtMeasurement(InstrumentConfig instrConfig, MeasurementConfig pavtConfig)
        {
            PavtConfig = pavtConfig;
            InstrConfig = instrConfig;
        }
        RFmxSpecAnMX specAn;
        public NIRfsg Rfsg { get; set; }
        public RFmxInstrMX Instr { get; set; }
        public InstrumentConfig InstrConfig { get; set; }
        public MeasurementConfig PavtConfig { get; set; }
        
        private List<PhaseAmplitudeOffset> PAOResultList;
        

        public void Run()
        {
            try
            {
                InitializeInstr();
                ConfigureSpecAn();
                

            }
            catch (Exception ex)
            {
                DisplayError(ex);
                MessageBox.Show(ex.Message + "\n\n" + ex.GetType().ToString() + "\n" + ex.StackTrace, "Exception");
                
            }
            finally
            {
                /* Close session */
                CloseSession();
            }
        }

        private void DisplayError(Exception ex)
        {
            Console.WriteLine("ERROR:\n" + ex.GetType() + ": " + ex.Message);
        }

        public void InitializeInstr()
        {
            Instr = InstrConfig.rfmxSession;
            //Instr = new RFmxInstrMX(InstrConfig.instrResourceName, "");
            Instr.ConfigureFrequencyReference("", RFmxInstrMXConstants.OnboardClock, 10.0e6);
            //Instr.ConfigureAutomaticSGSASharedLO("",
        }

        private void ConfigureSpecAn()
        {
            specAn = Instr.GetSpecAnSignalConfiguration("Stepped");
            specAn.ConfigureRF("", InstrConfig.frequency, InstrConfig.power, PavtConfig.externalAttenuation);
            specAn.ConfigureDigitalEdgeTrigger("", RFmxSpecAnMXConstants.PxiTriggerLine0, RFmxSpecAnMXDigitalEdgeTriggerEdge.Rising, 0, true);
            specAn.SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.Pavt, true);
            specAn.Pavt.Configuration.ConfigureMeasurementLocationType("", RFmxSpecAnMXPavtMeasurementLocationType.Trigger);
            specAn.Pavt.Configuration.ConfigureNumberOfSegments("", InstrConfig.PAOList.Count);
            specAn.Pavt.Configuration.ConfigureMeasurementBandwidth("", 10.0e6);
            specAn.Pavt.Configuration.ConfigureMeasurementInterval("", PavtConfig.mOffset/10e6, PavtConfig.mLength/10e6);
            specAn.Initiate("", "");
            
        }

        public void StimulateDUT()
        {
            //Begin generation of CW

            Rfsg.Initiate();
            Rfsg.CheckGenerationStatus();
        }

        public List<PhaseAmplitudeOffset> RetrieveResults()
        {
            int NumberOfSegments = InstrConfig.PAOList.Count;
            double[] meanRelativePhase = new double[NumberOfSegments];                          /* (deg) */
            double[] meanRelativeAmplitude = new double[NumberOfSegments];                      /* (dB) */
            double[] meanAbsolutePhase = new double[NumberOfSegments];                          /* (deg) */
            double[] meanAbsoluteAmplitude = new double[NumberOfSegments];                      /* (dBm) */
           
            
            specAn.Pavt.Results.FetchPhaseAndAmplitudeArray("", 10, ref meanRelativePhase,
               ref meanRelativeAmplitude, ref meanAbsolutePhase, ref meanAbsoluteAmplitude);

            PAOResultList = new List<PhaseAmplitudeOffset>();
            for (int i = 0; i < NumberOfSegments; i++)
            {
                PAOResultList.Add(new PhaseAmplitudeOffset(meanRelativePhase[i],  meanRelativeAmplitude[i]));
            }
            return PAOResultList;
        }

        private void CloseSession()
        {
            //unused
        }
    }
}
