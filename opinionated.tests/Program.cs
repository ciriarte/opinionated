using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opinionated.tests
{
    using log4net;
    using opinionated;

    class Program
    {
        static void Main(string[] args)
        {
            Opinionated.ConfigureLogging();

            var logger = LogManager.GetLogger(typeof(Program));

            logger.Info("Sup, World.");

            Console.ReadLine();
        }
    }
}
