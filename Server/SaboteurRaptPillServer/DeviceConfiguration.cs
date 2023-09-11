namespace SaboteurRaptPillServer {


    public class DeviceConfigurations {
        public List<DeviceConfiguration> Devices{ get; set; }
    }

    public class DeviceConfiguration {

        public string DeviceId { get; set; }
        public string Key { get; set; }
        public Correction Correction { get; set; }
        public Coefficient PlatoCoefficients { get; set; }
        public Coefficient SGCoefficients { get; set; }
    }


    public class Correction {
        public float X {get; set;}
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public class Coefficient {
        public float C0 {get; set;}
        public float C1 { get; set; }
        public float C2 { get; set; }
        public float C3 { get; set; }
    }

}

