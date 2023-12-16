using Microsoft.AspNetCore.Mvc;
using SaboteurRaptPillServer.DTO;
using System.Text.Json;

namespace SaboteurRaptPillServer.Controllers {
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DataController : ControllerBase {
        
        private readonly ILogger<DataController> _logger;
        private readonly IConfiguration _configuration;

        public DataController(ILogger<DataController> logger, IConfiguration configuration) {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        public void SaveData(MeasurementDTO data) {
            var deviceConfigurations = _configuration.GetSection(nameof(DeviceConfigurations)).Get<DeviceConfigurations>();

            var deviceConfiguration = deviceConfigurations.Devices.FirstOrDefault(x => x.DeviceId == data.DeviceId);
            if (deviceConfiguration == null) {
                throw new Exception("Configuration for device not found");
            }

            if (deviceConfiguration.Key != data.Key) {
                throw new Exception("Access denied");
            }

            // https://www.omnicalculator.com/math/angle-between-two-vectors
            // angle = arccos((xa​⋅xb​ + ya​⋅yb​ + za​⋅zb​) / ((xa2​ + ya2​ + za2​)^(1/2)​⋅(xb2​ + yb2​ + zb2​)^(1/2)​))​
            // measured 0 is  -9.425972,"Y":-0.1400608,"Z":-1.4221559
            var correction = deviceConfiguration.Correction;

            var X0 =  correction.X; //f;
            var Y0 = correction.Y; // - 0.1400608;
            var Z0 = correction.Z; // - 1.4221559f;

            var dotProduct = X0 * data.X + Y0 * data.Y + Z0 * data.Z;
            var mag1 = Math.Sqrt(X0 * X0 + Y0 * Y0 + Z0 * Z0);
            var mag2 = Math.Sqrt(data.X * data.X + data.Y * data.Y + data.Z * data.Z);

            var angle = (180 / Math.PI) * Math.Acos(dotProduct / (mag1 * mag2));
            var sgCoeficients = deviceConfiguration.SGCoefficients;
            var sg = sgCoeficients.C0 + sgCoeficients.C1 * angle + sgCoeficients.C2 * angle * angle + sgCoeficients.C3 *angle * angle * angle;

            var platoCoefficients = deviceConfiguration.PlatoCoefficients;
            var plato = platoCoefficients.C0 + platoCoefficients.C1 * angle + platoCoefficients.C2 * angle * angle + platoCoefficients.C3 * angle * angle * angle;

            var measurement = new {SG = sg, Plato = plato, Temperature = data.T, Angle = angle, X = data.X, Y = data.Y, Z = data.Z, Time = DateTime.Now};
            var dataDirectory = (string)AppDomain.CurrentDomain.GetData("DataDirectory");
            var path = Path.Combine(dataDirectory, $"{data.DeviceId}.json");
            var content = System.IO.File.ReadAllText(path);

            System.IO.File.WriteAllText(path, $"{JsonSerializer.Serialize(measurement)}{System.Environment.NewLine}" + content);

        }

        [HttpGet]
        public string GetData(string deviceId) {
            var dataDirectory = (string) AppDomain.CurrentDomain.GetData("DataDirectory");
            return System.IO.File.ReadAllText(Path.Combine(dataDirectory, $"{deviceId}.json"));

        }

    }
}