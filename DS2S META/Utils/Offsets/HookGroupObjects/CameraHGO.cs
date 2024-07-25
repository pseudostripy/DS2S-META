using DS2S_META.Utils.DS2Hook;
using DS2S_META.Utils.Offsets.OffsetClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DS2S_META.Utils.Offsets.HookGroupObjects
{
    public class CameraHGO : HGO
    {
        // Direct Hook properties
        public float CamX
        { 
            get => PHCamX?.ReadSingle() ?? 0.0f;
            set
            {
                PHCamX?.WriteSingle(value);
                OnPropertyChanged();
            }
        }
        public float CamY
        {
            get => PHCamY?.ReadSingle() ?? 0.0f;
            set
            {
                PHCamY?.WriteSingle(value);
                OnPropertyChanged();
            }
        }
        public float CamZ
        {
            get => PHCamZ?.ReadSingle() ?? 0.0f;
            set
            {
                PHCamZ?.WriteSingle(value);
                OnPropertyChanged();
            }
        }

        // REDataDefinitionInterfaces
        private PHLeaf? PHCamX;
        private PHLeaf? PHCamY;
        private PHLeaf? PHCamZ;

        public CameraHGO(DS2SHook hook, Dictionary<string, PHLeaf?> cameraGrp) : base(hook)
        {
            foreach (var kvp in cameraGrp)
            {
                string propkey = TranslateRENameToPHLeafPropName(kvp.Key);
                SetProperty(propkey, kvp.Value);
            }
        }

        public override void UpdateProperties()
        {
            OnPropertyChanged(nameof(CamX));
            OnPropertyChanged(nameof(CamY));
            OnPropertyChanged(nameof(CamZ));
        }
    }
}
