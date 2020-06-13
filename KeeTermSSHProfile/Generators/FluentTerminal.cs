using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KeePassLib;
using KeeTermSSHProfile;

namespace KeeTermSSHProfile.Generators
{
    class FluentTerminal : AbstractGenerator
    {
        public override string Title { get { return "Fluent Terminal"; } }

        protected override bool DetectInstallation() {
            return IsInstalled("Fluent Terminal");
        }

        public override void Generate(List<Tuple<PwEntry, EntrySettings>> entries)
        {
        }

        public override UI.GenEntryFormGroup CreateOptionGroup(GenEntry entry)
        {
            return null;
        }
    }
}
