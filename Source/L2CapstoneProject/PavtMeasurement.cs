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
        RFmxSpecAnMX specAn;
        public NIRfsg Rfsg { get; set; }
        public RFmxInstrMX Instr { get; set; }

        public void Run()
        {
            try
            {
                
                InitializeInstr();
                ConfigureSpecAn();
                RetrieveResults();
                UpdateResults();
            }
            catch (Exception ex)
            {
                DisplayError(ex);
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
            throw new NotImplementedException();
        }

        

        private void ConfigureSpecAn()
        {
            specAn = Instr.GetSpecAnSignalConfiguration();
            Instr.ConfigureFrequencyReference("", RFmxInstrMXConstants.OnboardClock, 10.0e6);
        }

        public void StimulateDUT()
        {
            //Begin generation of CW

            Rfsg.Initiate();
            Rfsg.CheckGenerationStatus();

        }
        private void UpdateResults()
        {
            throw new NotImplementedException();
        }

        private void RetrieveResults()
        {
            throw new NotImplementedException();
        }
        private void CloseSession()
        {
            throw new NotImplementedException();
        }
    }
}
