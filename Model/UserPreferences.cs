using _4RTools.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;

namespace _4RTools.Model
{
    public class UserPreferences : Action
    {
        private string ACTION_NAME = "UserPreferences";
        public string toggleStateKey { get; set; } = Keys.End.ToString();
        public string toggleStateHealKey { get; set; } = Keys.End.ToString();
        public List<EffectStatusIDs> autoBuffOrder { get; set; } = new List<EffectStatusIDs>();

        public bool stopBuffsCity { get; set; } = false;
        public bool stopBuffsRein { get; set; } = false;
        public bool stopHealCity { get; set; } = false;
        public bool getOffRein { get; set; } = false;
        public bool stopWithChat { get; set; } = false;
        public Key getOffReinKey { get; set; }

        public bool switchAmmo { get; set; } = false;
        public Key ammo1Key { get; set; }
        public Key ammo2Key { get; set; }

        // Impacto Explosivo Feature
        public Key ImpactItemKey { get; set; }      // tecla para equipar item que ativa o impacto
        public Key ImpactSkillKey { get; set; }     // tecla para usar o impacto
        public Key ImpactDefaultKey { get; set; }   // tecla para voltar ao item normal
        public int ImpactDelay { get; set; }        // delay entre as teclas


        public bool stopSpammersBot { get; set; } = false;

        public UserPreferences()
        {
        }

        public void Start() { }

        public void Stop() { }

        public string GetConfiguration()
        {
            return JsonConvert.SerializeObject(this);
        }

        public string GetActionName()
        {
            return ACTION_NAME;
        }
        public void SetAutoBuffOrder(List<EffectStatusIDs> buffs)
        {
            this.autoBuffOrder = buffs;
        }
    }
}
