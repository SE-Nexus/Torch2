using IgniteSE1.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Services
{
    public class ServiceManager
    {

        public List<ServiceBase> Services { get; private set; } = new List<ServiceBase>();

        public ServiceManager(IEnumerable<ServiceBase> services) 
        { 
        
            Console.WriteLine($"ServiceManager initialized with services: {services.Count()}");
        }



    }
}
