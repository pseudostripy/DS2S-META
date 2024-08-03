using DS2S_META.ViewModels;
using DS2S_META;
using PropertyHook;
using System.Collections.Generic;

public class AttrLvlDataVM : ViewModelBase
{
    private static readonly Dictionary<ATTR, string> LevelNames = new()
            {
                { ATTR.VGR, "Vigor" },
                { ATTR.END, "Endurance" },
                { ATTR.VIT, "Vitality" },
                { ATTR.ATN, "Attunement" },
                { ATTR.STR, "Strength" },
                { ATTR.DEX, "Dexterity" },
                { ATTR.ADP, "Adaptability" },
                { ATTR.INT, "Intelligence" },
        { ATTR.FTH, "Faith" }
            };
    public readonly ATTR Attr;
    public int AttrLvl
    {
        get => Hook?.DS2P.PlayerData.GetAttributeLevel(Attr) ?? 0;
        set => Hook?.DS2P.PlayerData.SetAttributeLevel(Attr, value);
    }
    public int AttrLvlMin
    {
        get
        {
            if (_playerClassId == null) return 1;
            var ds2class = DS2Resource.GetClassById((PLAYERCLASS)_playerClassId);
            return ds2class.ClassMinLevels[Attr];
        }
    }
    public string AttrName => LevelNames[Attr];

    // could maybe do this with dependencyProperty?
    private PLAYERCLASS? _playerClassId = null;
    private void RefreshClass()
    {
        var hookclass = Hook?.DS2P.PlayerData.Class;
        if (hookclass == _playerClassId) return; // same as previous
        _playerClassId = hookclass;             // update to new value
        OnPropertyChanged(nameof(AttrLvlMin));  // update UI
    }

    public AttrLvlDataVM(ATTR attr)
    {
        Attr = attr;
        OnPropertyChanged(nameof(AttrName));
    }
    public override void UpdateViewModel()
    {
        OnPropertyChanged(nameof(AttrLvl)); // only need to keep this updated
        OnPropertyChanged(nameof(AttrName));
        RefreshClass();
    }
    public void UpdateNewClassData()
    {
        OnPropertyChanged(nameof(AttrLvlMin));
    }
}