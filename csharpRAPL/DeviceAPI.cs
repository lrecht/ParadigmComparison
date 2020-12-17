using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace csharpRAPL
{
    public abstract class DeviceAPI
    {
        private List<int> _socketIds;
        private List<string> _sysFiles;

        private List<int> get_cpus()
        {
            string api_file = "/sys/devices/system/cpu/present";
            List<int> cpu_list = new List<int>();
            Regex cpu_count_re = new Regex(@"\d+|-");
            MatchCollection cpu_matches = cpu_count_re.Matches(File.ReadAllText(api_file).Trim());

            for (int i = 0; i < cpu_matches.Count; i++)
            {
                if (cpu_matches[i].Value == "-")
                {
                    int before = int.Parse(cpu_matches[i - 1].Value);
                    int after = int.Parse(cpu_matches[i + 1].Value);
                    foreach (int j in Enumerable.Range(before, after - before))
                        cpu_list.Add(j);
                }
                else
                    cpu_list.Add(int.Parse(cpu_matches[i].Value));
            }

            return cpu_list;
        }

        private List<int> getSocketIds() 
        {
            List<int> socket_id_list = new List<int>();

            foreach (var cpuId in get_cpus())
            {
                string path = $"/sys/devices/system/cpu/cpu{cpuId}/topology/physical_package_id";
                socket_id_list.Add(int.Parse(File.ReadAllText(path).Trim()));
            }

            return socket_id_list.Distinct().ToList();
        }

        public DeviceAPI(List<int> socketIds = null)
        {
            List<int> allSocketIds = getSocketIds();
            if (socketIds == null){
                this._socketIds = allSocketIds;
            }
            else
            {
                foreach (var sid in socketIds) 
                {
                    if (allSocketIds.Contains(sid))
                        throw new Exception("PyRAPLBadSocketIdException"); //TODO: Proper exceptions

                    this._socketIds = socketIds;
                }
            }

            this._socketIds.Sort();
            
            this._sysFiles = this.openRAPLFiles();
        }

        public abstract List<string> openRAPLFiles();

        public virtual List<(string dirName, int raplId)> GetSocketDirectoryNames(){
            void addToResult((string dirName, int raplId) directoryInfo, List<(int, string, int)> result){
                string pkgStr = File.ReadAllText(directoryInfo.dirName + "/name").Trim();
                
                if(!pkgStr.Contains("package"))
                    return;
                var packageId = int.Parse(pkgStr.Split('-')[1]);
                
                if(this._socketIds != null && !this._socketIds.Contains(packageId)){
                    return;
                }

                result.Add((packageId, directoryInfo.dirName, directoryInfo.raplId));
            }

            var raplId = 0;
            var resultList = new List<(int packageId, string dirName, int raplId)>();
            
            while(Directory.Exists("/sys/class/powercap/intel-rapl/intel-rapl:" + raplId)){
                string dirName = "/sys/class/powercap/intel-rapl/intel-rapl:" + raplId;
                addToResult((dirName, raplId), resultList);
                raplId += 1;
            }

            if(resultList.Count != this._socketIds.Count)
                throw new Exception("PyRAPLCantInitDeviceAPI"); //TODO: Proper exceptions

            resultList.OrderBy(t => t.packageId);
            return resultList.Select(t => (t.dirName, t.raplId)).ToList();
        }
        
        public List<double> Energy(){
            var result = Enumerable.Range(0, this._socketIds.Count).Select(i => -1.0).ToList();
            for(int i = 0; i < _sysFiles.Count; i++){
                var deviceFile = this._sysFiles[i];
                //TODO: Test om der er mærkbar forskel ved at holde filen åben og læse linjen på ny
                double energyVal = 0.0;
                bool canConvert = Double.TryParse(File.ReadAllText(deviceFile), out energyVal);
                result[this._socketIds[i]] = canConvert ? energyVal : -1.0;
            }
            return result;
        }
    }

    public class PackageAPI : DeviceAPI
    {
        public PackageAPI(List<int> socket_ids = null) : base(socket_ids) {}

        override public List<string> openRAPLFiles()
        {
            List<(string, int)> socket_names = this.GetSocketDirectoryNames();
            List<string> rapl_files = new List<string>();

            foreach (var (dir, id) in socket_names)
            {
                rapl_files.Add(dir + "/energy_uj");
            }

            return rapl_files;
        }
    }

    public class DramAPI : DeviceAPI
    {
        public DramAPI(List<int> socket_ids = null) : base(socket_ids) {}

        override public List<string> openRAPLFiles()
        {
            List<(string, int)> socket_names = this.GetSocketDirectoryNames();
            
            string getDramFile(string directoryName, int raplSocketId){
                int rapl_device_id = 0;
                while (Directory.Exists(directoryName + "/intel-rapl:" + raplSocketId + ":" + rapl_device_id))
                {
                    var dirName = directoryName + "/intel-rapl:" + raplSocketId + ":" + rapl_device_id;
                    var content = File.ReadAllText(dirName + "/name").Trim();
                    if (content.Equals("dram"))
                        return dirName + "/energy_uj";
                    
                    rapl_device_id += 1;
                }
                
                throw new Exception("PyRAPLCantInitDeviceAPI"); //TODO: Proper exceptions
            }
        
            //LINE 167
            List<string> raplFiles = new List<string>();
            foreach(var (socketDirectoryName, raplSocketId) in socket_names) 
            {
                raplFiles.Add(getDramFile(socketDirectoryName, raplSocketId));
            }

            return raplFiles;
        }
    }

    public class TempAPI : DeviceAPI
    {

        override public List<string> openRAPLFiles()
        {
            string path = "/sys/class/thermal/";
            int thermal_id = 0;
            while(Directory.Exists(path + "/thermal_zone" + thermal_id))
            {
                string dirname = path + "/thermal_zone" + thermal_id;
                string type = File.ReadAllText(dirname + "/type").Trim();
                if (type.Contains("pkg_temp"))
                    return new List<string>() {dirname + "/temp"};
                thermal_id++;
            }
            throw new Exception("No thermal zone found for the package");
        }
    }
}
