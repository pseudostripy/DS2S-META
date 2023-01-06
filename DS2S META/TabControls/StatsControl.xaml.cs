using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace DS2S_META
{
    /// <summary>
    /// Interaction logic for StatsControl.xaml
    /// </summary>
    public partial class StatsControl : METAControl
    {
        public List<IntegerUpDown> nudLevels => new List<IntegerUpDown>() 
            { nudVig, nudEnd, nudVit, nudAtt, nudStr, nudDex, nudAdp, nudInt, nudFth };

        public StatsControl()
        {
            InitializeComponent();
            foreach (DS2SClass charClass in DS2SClass.All)
                cmbClass.Items.Add(charClass);
            cmbClass.SelectedIndex = -1;
        }
        public void ReloadTab()
        {

        }
        private void cbmClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Hook.Loaded)
            {
                if (cmbClass.SelectedItem is not DS2SClass charClass)
                    throw new NullReferenceException("Null character class");

                Hook.Class = charClass.ID;
                nudVig.Minimum = charClass.Vigor;
                nudEnd.Minimum = charClass.Endurance;
                nudVit.Minimum = charClass.Vitality;
                nudAtt.Minimum = charClass.Attunement;
                nudStr.Minimum = charClass.Strength;
                nudDex.Minimum = charClass.Dexterity;
                nudAdp.Minimum = charClass.Adaptability;
                nudInt.Minimum = charClass.Intelligence;
                nudFth.Minimum = charClass.Faith;
            }
        }

        internal override void UpdateCtrl()
        {
            
        }

        internal override void ReloadCtrl()
        {
            cmbClass.SelectedItem = cmbClass.Items.Cast<DS2SClass>().FirstOrDefault(c => c.ID == Hook.Class);
            txtName.Text = Hook.Name;
        }

        internal override void EnableCtrls(bool enable)
        {
            cmbClass.IsEnabled = enable;
            txtName.IsEnabled = enable;
            btnGive.IsEnabled = enable;
            btnResetSoulMemory.IsEnabled = enable;
            nudGiveSouls.IsEnabled = enable;
            nudVig.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudEnd.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudVit.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudAtt.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudStr.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudDex.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudAdp.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudInt.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudFth.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudHollowLevel.IsEnabled = enable;
            btnReset.IsEnabled = enable && Properties.Settings.Default.EditStats;
            btnMax.IsEnabled = enable && Properties.Settings.Default.EditStats;
            btnRestoreHumanity.IsEnabled = enable;

            if (!enable)
                cmbClass.SelectedIndex = -1;
        }
        private void GiveSouls_Click(object sender, RoutedEventArgs e)
        {
            if (nudGiveSouls.Value.HasValue)
                Hook.GiveSouls(nudGiveSouls.Value.Value);
        }
        private void ResetSoulMemory_Click(object sender, RoutedEventArgs e)
        {
            Hook.ResetSoulMemory();
        }
        private void Name_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Hook.Name = txtName.Text;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            foreach (IntegerUpDown nudLev in nudLevels)
                nudLev.Value = nudLev.Minimum;
        }

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            foreach (IntegerUpDown nudLev in nudLevels)
                nudLev.Value = 99;
        }

        private void RestoreHumanity_Click(object sender, RoutedEventArgs e)
        {
            Hook.ApplySpecialEffect(100000010);
        }

        private void NewTestCharacter_Click(object sender, RoutedEventArgs e)
        {
            // Define character multi-items
            var lifegemid = 60010000;
            var oldradid = 60030000;
            var mushroomid = 60035000;
            var blessingid = 60105000;
            var effigyid = 60151000;
            var mossid = 60070000;
            var wiltherbid = 60060000;
            var oozeid = 60240000;
            var gprid = 60250000;
            var dprid = 60270000;
            var featherid = 60355000;
            var branchid = 60537000;
            var witchurnid = 60550000;
            var firebombid = 60570000;
            var blkfirebombid = 60575000;
            var dungid = 60595000;
            var poisonknifeid = 60590000;
            var greatheroid = 60720000;
            var odosid = 64320000;
            var skullsid = 60530000;
            var torchid = 60420000;
            //
            var titshardid = 60970000;
            var ltsid = 60975000;
            var chunkid = 60980000;
            var slabid = 60990000;
            var twinklingid = 61000000;
            var ptbid = 61030000;
            var boltstoneid = 61070000;
            var darkstoneid = 61090000;
            var rawstoneid = 61130000;
            var palestoneid = 61160000;
            var lockstoneid = 60536000;

            var multi_items = new List<int>() { lifegemid, oldradid, mushroomid, blessingid, effigyid, mossid, wiltherbid,
                                                oozeid, gprid, dprid, featherid, branchid, witchurnid, firebombid, blkfirebombid,
                                                dungid, poisonknifeid, greatheroid, odosid, skullsid, torchid,
                                                titshardid, ltsid, chunkid, slabid, twinklingid, ptbid, boltstoneid, darkstoneid,
                                                rawstoneid, palestoneid, lockstoneid};
            foreach (int id in multi_items)
                Hook.GiveItemSilently(id, 95, 0, 0);

            // ammo
            var woodarrowid = 60760000;
            var ironarrowid = 60770000;
            var magicarrowid = 60780000;
            var firearrowid = 60800000;
            var psnarrowid = 60820000;
            var heavyboltid = 60920000;

            var ammo_items = new List<int>() { woodarrowid, ironarrowid, magicarrowid, firearrowid, psnarrowid, heavyboltid };
            foreach (int id in ammo_items)
                Hook.GiveItemSilently(id, 950, 0, 0);

            // unupgraded stuff:
            var estusid = 60155000;
            var binoid = 6100000;
            var bucklerid = 11000000;
            var goldenshield = 11050000;
            var ironparmaid = 11020000;

            var clo1id = 40020001;
            var bladesid = 40160000;
            var catringid = 40420000;
            var soul1ringid = 40370001;
            var flynnsid = 41100000;

            var staffid = 3800000;
            var chimeid = 4010000;
            var pyroid = 5400000;
            var dwid = 34060000;
            var slbid = 32260000;

            var bflyskirtid = 21470103;
            var bflywingsid = 21470101;
            var tseldorahatid = 22460100;
            var tseldorabodyid = 22460101;
            var tseldoraglovesid = 22460102;
            var tseldorapantsid = 22460103;

            var single_items = new List<int>() { estusid, binoid, bucklerid, goldenshield, ironparmaid,
                                                clo1id, bladesid, catringid, soul1ringid, flynnsid,
                                                staffid, chimeid, pyroid, dwid, slbid,
                                                bflyskirtid, bflywingsid, tseldorahatid, tseldorabodyid, tseldoraglovesid, tseldorapantsid};
            foreach (int id in single_items)
                Hook.GiveItemSilently(id, 1, 0, 0);

            // upgraded speedrun weapons:
            var daggerid = 1000000;
            var rapierid = 1500000;
            var mace = 2410000;
            var shortbowid = 4200000;
            var lightcbwid = 4600000;
            var uchiid = 1700000;

            var upgr_weapons = new List<int>() { daggerid, rapierid, mace, shortbowid, lightcbwid, uchiid };
            foreach (int id in upgr_weapons)
                Hook.GiveItemSilently(id, 1, 10, 0);

            // Misc other weapons:
            Hook.GiveItemSilently(rapierid, 1, 0, 0);   // basic rapier
            Hook.GiveItemSilently(rapierid, 1, 10, 3);  // lightning rapier
            Hook.GiveItemSilently(rapierid, 1, 10, 4);  // dark rapier
            var ritbid = 5350000;
            Hook.GiveItemSilently(ritbid, 1, 10, 3);    // lightning RITB
            var dbgsid = 1990000;
            Hook.GiveItemSilently(dbgsid, 1, 5, 0);
            var decapitateid = 63017000; // :D

            // Keys:
            List<int> keylist = new();
            keylist.Add(0x03041840); // Soldier Key
            keylist.Add(0x03043F50); // Key to King's Passage
            keylist.Add(0x0304B480); // Weapon Smithbox
            keylist.Add(0x0304DB90); // Armor Smithbox
            keylist.Add(0x03072580); // Bastille Key
            keylist.Add(0x03074C90); // Iron Key
            keylist.Add(0x030773A0); // Forgotten Key
            keylist.Add(0x03079AB0); // Brightstone Key
            keylist.Add(0x0307C1C0); // Antiquated Key
            keylist.Add(0x0307E8D0); // Fang Key
            keylist.Add(0x03080FE0); // House Key
            keylist.Add(0x030836F0); // Lenigrast's Key
            keylist.Add(0x03085E00); // Smooth & Silky Stone
            keylist.Add(0x03087188); // Small Smooth & Silky Stone
            keylist.Add(0x03088510); // Rotunda Lockstone
            keylist.Add(0x0308AC20); // Giant's Kinship
            keylist.Add(0x0308D330); // Ashen Mist Heart
            keylist.Add(0x0308FA40); // Soul of a Giant
            keylist.Add(0x03092150); // Tseldora Den Key
            keylist.Add(0x0309BD90); // Undead Lockaway Key
            keylist.Add(0x030A0BB0); // Dull Ember
            keylist.Add(0x030A32C0); // Crushed Eye Orb
            keylist.Add(0x030AA7F0); // Aldia Key
            keylist.Add(0x03197500); // Dragon Talon
            keylist.Add(0x031AFBA0); // Heavy Iron Key
            keylist.Add(0x031C8240); // Frozen Flower
            keylist.Add(0x031E08E0); // Eternal Sanctum Key
            keylist.Add(0x031F8F80); // Tower Key
            keylist.Add(0x03211620); // Garrison Ward Key
            keylist.Add(0x03236010); // Dragon Stone
            foreach (int id in keylist)
                Hook.GiveItemSilently(id, 1, 0, 0);

            // Gestures:
            Hook.GiveItem_wrapper(decapitateid, 1, 0, 0);        // show visibly
            


            // Used to create a character with commonly useful things
            RestoreHumanity_Click(sender, e);   // human char
            Max_Click(sender, e);               // max levels
            Hook.GiveSouls(9999999);            // give souls
            Hook.UnlockBonfires();              // unlock all bonfire


            // to tidy:
            DS2SBonfire majula = new DS2SBonfire(168034304, 4650, "The Far Fire");
            Hook.LastBonfireID = majula.ID;
            Hook.LastBonfireAreaID = majula.AreaID;
            Hook.Warp(majula.ID);
        }

    }
}
