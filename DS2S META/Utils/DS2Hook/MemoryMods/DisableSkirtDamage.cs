using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.DS2Hook.MemoryMods
{
    public class DisableSkirtDamage : MemoryModification
    {
        private Inject? Inj1;

        public DisableSkirtDamage(DS2SHook hook) : base(hook)
        {
        }

        public override void Install()
        {
            var inj1_code = new byte[] { 0x89, 0x84, 0x8B, 0xC4, 0x01, 0x00, 0x00 };
            var inj1_disabled = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 };
            Inj1 = new Inject(Hook, Hook.DS2P.Func.DisableSkirtDamage.Resolve(), inj1_code, inj1_disabled);
            Inj1.Install();
            IsInstalled = true;
        }

        public override void Uninstall()
        {
            Inj1?.Uninstall();
            IsInstalled = false;
        }


    }
}
