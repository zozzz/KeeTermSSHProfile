using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeeTermSSHProfile
{
    public enum GeneratorType {
        NotSet = 0,
        WindowsTerminal = 1
    }

    public interface IGenerator {
        // void update();
    }

    public class Generator
    {
        public static IGenerator create(GeneratorType type) {
            switch (type) {
                case GeneratorType.WindowsTerminal:
                    return new Generators.WindowsTerminal();
            }

            throw new Exception("Invalid generator type");
        }
    }
}
