using System;
using System.Collections.Generic;
using System.Windows.Forms;
using _4RTools.Utils;
using _4RTools.Model;
using System.Windows.Input;
using System.Web;
using System.Diagnostics.Tracing;

namespace _4RTools.Forms
{
    public partial class AHKForm : Form, IObserver
    {
        private AHK ahk;
        public AHKForm(Subject subject)
        {
            subject.Attach(this);
            InitializeComponent();
        }

        public void Update(ISubject subject)
        {
            switch ((subject as Subject).Message.code)
            {
                case MessageCode.PROFILE_CHANGED:
                    InitializeApplicationForm();
                    break;
                case MessageCode.TURN_ON:
                    this.ahk.Start();
                    break;
                case MessageCode.TURN_OFF:
                    this.ahk.Stop();
                    break;
            }
        }

        private void InitializeApplicationForm()
        {
            RemoveHandlers();
            FormUtils.ResetCheckboxForm(this);
            SetLegendDefaultValues();
            this.ahk = ProfileSingleton.GetCurrent().AHK;
            InitializeCheckAsThreeState();

            var prefs = ProfileSingleton.GetCurrent().UserPreferences;

            txtImpactKey1.Text = prefs.ImpactItemKey.ToString();
            txtImpactKey2.Text = prefs.ImpactSkillKey.ToString();
            txtImpactKey3.Text = prefs.ImpactDefaultKey.ToString();
            numImpactDelay.Value = prefs.ImpactDelay;

            // Adiciona eventos para salvar automaticamente
            txtImpactKey1.Leave += ImpactKey_Changed;
            txtImpactKey2.Leave += ImpactKey_Changed;
            txtImpactKey3.Leave += ImpactKey_Changed;
            numImpactDelay.ValueChanged += NumImpactDelay_ValueChanged;

            // Evento KeyDown para Impacto Explosivo
            txtImpactKey1.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Back)
                {
                    txtImpactKey1.Text = "None";
                    ProfileSingleton.GetCurrent().UserPreferences.ImpactItemKey = Key.None;
                }
                else
                {
                    txtImpactKey1.Text = e.KeyCode.ToString();
                    ProfileSingleton.GetCurrent().UserPreferences.ImpactItemKey = (Key)Enum.Parse(typeof(Key), e.KeyCode.ToString());
                }
            };

            txtImpactKey2.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Back)
                {
                    txtImpactKey2.Text = "None";
                    ProfileSingleton.GetCurrent().UserPreferences.ImpactSkillKey = Key.None;
                }
                else
                {
                    txtImpactKey2.Text = e.KeyCode.ToString();
                    ProfileSingleton.GetCurrent().UserPreferences.ImpactSkillKey = (Key)Enum.Parse(typeof(Key), e.KeyCode.ToString());
                }
            };

            txtImpactKey3.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Back)
                {
                    txtImpactKey3.Text = "None";
                    ProfileSingleton.GetCurrent().UserPreferences.ImpactDefaultKey = Key.None;
                }
                else
                {
                    txtImpactKey3.Text = e.KeyCode.ToString();
                    ProfileSingleton.GetCurrent().UserPreferences.ImpactDefaultKey = (Key)Enum.Parse(typeof(Key), e.KeyCode.ToString());
                }
            };



            RadioButton rdAhkMode = (RadioButton)this.groupAhkConfig.Controls[ProfileSingleton.GetCurrent().AHK.ahkMode];
            if (rdAhkMode != null) { rdAhkMode.Checked = true; };
            this.txtSpammerDelay.Text = ProfileSingleton.GetCurrent().AHK.AhkDelay.ToString();
            this.chkNoShift.Checked = ProfileSingleton.GetCurrent().AHK.noShift;
            this.chkMouseFlick.Checked = ProfileSingleton.GetCurrent().AHK.mouseFlick;
            this.DisableControlsIfSpeedBoost();

            Dictionary<string, KeyConfig> ahkClones = new Dictionary<string, KeyConfig>(ProfileSingleton.GetCurrent().AHK.AhkEntries);

            foreach (KeyValuePair<string, KeyConfig> config in ahkClones)
            {
                ToggleCheckboxByName(config.Key, config.Value.ClickActive);
            }
        }

        private void SaveImpactoExplosivoConfig()
        {
            try
            {
                ProfileSingleton.GetCurrent().UserPreferences.ImpactItemKey = (Key)Enum.Parse(typeof(Key), txtImpactKey1.Text);
                ProfileSingleton.GetCurrent().UserPreferences.ImpactSkillKey = (Key)Enum.Parse(typeof(Key), txtImpactKey2.Text);
                ProfileSingleton.GetCurrent().UserPreferences.ImpactDefaultKey = (Key)Enum.Parse(typeof(Key), txtImpactKey3.Text);
                ProfileSingleton.GetCurrent().UserPreferences.ImpactDelay = (int)numImpactDelay.Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar Impacto Explosivo: " + ex.Message);
            }
        }

        private void ImpactKey_Changed(object sender, EventArgs e)
        {
            SaveImpactoExplosivoConfig();
        }

        private void NumImpactDelay_ValueChanged(object sender, EventArgs e)
        {
            SaveImpactoExplosivoConfig();
        }

        private void txtImpactKey1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // Substitui o texto atual pela tecla pressionada
            txtImpactKey1.Text = e.KeyCode.ToString();
            e.SuppressKeyPress = true; // evita que a tecla seja digitada no TextBox
        }

        private void txtImpactKey2_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            txtImpactKey2.Text = e.KeyCode.ToString();
            e.SuppressKeyPress = true;
        }

        private void txtImpactKey3_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            txtImpactKey3.Text = e.KeyCode.ToString();
            e.SuppressKeyPress = true;
        }

        private void onCheckChange(object sender, EventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            bool haveMouseClick = checkbox.CheckState == CheckState.Checked ? true : false;

            if (checkbox.CheckState == CheckState.Checked || checkbox.CheckState == CheckState.Indeterminate)
            {
                Key key;
                if (checkbox.Tag != null)
                {
                    key = (Key)new KeyConverter().ConvertFromString(checkbox.Tag.ToString());
                }
                else
                {
                    key = (Key)new KeyConverter().ConvertFromString(checkbox.Text);
                }

                this.ahk.AddAHKEntry(checkbox.Name, new KeyConfig(key, haveMouseClick));
            }
            else
                this.ahk.RemoveAHKEntry(checkbox.Name);

            ProfileSingleton.SetConfiguration(this.ahk);
        }

        private void txtSpammerDelay_TextChanged(object sender, EventArgs e)
        {
            try
            {
                this.ahk.AhkDelay = Convert.ToInt16(this.txtSpammerDelay.Value);
                ProfileSingleton.SetConfiguration(this.ahk);
            }
            catch { }
        }

        private void ToggleCheckboxByName(string Name, bool state)
        {
            try
            {
                CheckBox checkBox = (CheckBox)this.Controls.Find(Name, true)[0];
                checkBox.CheckState = state ? CheckState.Checked : CheckState.Indeterminate;
                ProfileSingleton.SetConfiguration(this.ahk);
            }
            catch { }
        }

        private void RemoveHandlers()
        {
            foreach (Control c in this.Controls)
                if (c is CheckBox)
                {
                    CheckBox check = (CheckBox)c;
                    check.CheckStateChanged -= onCheckChange;
                }
            this.chkNoShift.CheckedChanged -= new System.EventHandler(this.chkNoShift_CheckedChanged);
        }


        private void InitializeCheckAsThreeState()
        {
            foreach (Control c in this.Controls)
                if (c is CheckBox)
                {
                    CheckBox check = (CheckBox)c;
                    if ((check.Name.Split(new[] { "chk" }, StringSplitOptions.None).Length == 2))
                    {
                        check.ThreeState = true;
                    };

                    if (check.Enabled)
                        check.CheckStateChanged += onCheckChange;
                }
            this.chkNoShift.CheckedChanged += new System.EventHandler(this.chkNoShift_CheckedChanged);
        }

        private void SetLegendDefaultValues()
        {
            this.cbWithNoClick.ThreeState = true;
            this.cbWithNoClick.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.cbWithNoClick.AutoCheck = false;
            this.cbWithClick.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbWithClick.ThreeState = true;
            this.cbWithClick.AutoCheck = false;
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.Checked)
            {
                this.ahk.ahkMode = rb.Name;
                ProfileSingleton.SetConfiguration(this.ahk);
                this.DisableControlsIfSpeedBoost();
            }
        }

        private void chkMouseFlick_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            this.ahk.mouseFlick = chk.Checked;
            ProfileSingleton.SetConfiguration(this.ahk);
        }

        private void chkNoShift_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            this.ahk.noShift = chk.Checked;
            ProfileSingleton.SetConfiguration(this.ahk);
        }

        private void DisableControlsIfSpeedBoost()
        {
            if (this.ahk.ahkMode == AHK.SPEED_BOOST)
            {
                this.chkMouseFlick.Enabled = false;
                this.chkNoShift.Enabled = false;
            } else
            {
                this.chkMouseFlick.Enabled = true;
                this.chkNoShift.Enabled = true;
            }
        }
    }
}
