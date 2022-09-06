using System.Configuration;
using System.Diagnostics;

namespace HardDiskMonitor
{
    public partial class Form1 : Form
    {
        System.Threading.CancellationTokenSource tokenSource = new CancellationTokenSource();
        int percent = 0;
        string hardName = "C:\\";
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            checkBox1.Checked = SettingHelper.IsExistKey(Process.GetCurrentProcess().MainModule.ModuleName);
            numericUpDown1.Value = percent = int.Parse(ConfigurationManager.AppSettings.Get("Percent") ?? "80");
            hardName = ConfigurationManager.AppSettings.Get("HardName") ?? hardName;
            var hards = DriveInfo.GetDrives().ToList();
            hards.ForEach(hard =>
            {
                this.comboBox1.Items.Add(hard.Name);
            });
            this.comboBox1.Text = hardName;
            Task.Run(async () =>
            {
                while (!tokenSource.IsCancellationRequested)
                {
                    int value = (int)(HardDiskHelper.GetUsedPercnet(hardName) * 100);
                    if (value > percent)
                        this.notifyIcon1.ShowBalloonTip(200000, "�澯", $"Ӳ��ʹ�������Ѿ��ﵽ{value}%\r\n�뼰ʱ����", ToolTipIcon.Warning);
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }, tokenSource.Token);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox ck)
            {
                SettingHelper.SetMeStart(ck.Checked);
                this.notifyIcon1.ShowBalloonTip(2000, "��Ϣ", ck.Checked ? "��������" : "�رտ�������", ToolTipIcon.Info);
            }
        }
        /// <summary>
        /// ����Ӳ�̼������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            SetValue(sender);

        }

        private static void SetValue(object sender)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            settings["Percent"].Value = (sender as NumericUpDown).Value.ToString("#0");
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }

        private void numericUpDown1_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.Enter)
            //{
            //    SetValue(sender);
            //}
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.notifyIcon1.ShowBalloonTip(2000, "��Ϣ", "�Ѿ���С������", ToolTipIcon.Info);
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            settings["HardName"].Value = (sender as ComboBox).Text;
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            hardName = this.comboBox1.Text;
        }

        private async void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Name == "toolStripMenuItemShow")
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
            else if (e.ClickedItem.Name == "toolStripMenuItemClose")
            {
                tokenSource.Cancel();
                this.notifyIcon1.ShowBalloonTip(2000, "��Ϣ", "ϵͳ�����˳�", ToolTipIcon.Info);
                await Task.Delay(2000);
                Application.Exit();
            }
        }
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        /// <summary>
        /// appconfig ӳ�䵽ʵ��
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="prefix"></param>
        private void ConfigToEntity<T>(T obj, string prefix = "") where T : class, new()
        {
            var properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                //��������
                if (property.PropertyType.IsPrimitive())
                {
                    var configvalue = ConfigurationManager.AppSettings.Get(prefix + property.Name);
                    var setValue = configvalue.ConvertValue(property.PropertyType);
                    property.SetValue(obj, setValue);
                }
                else
                {
                    var value = property.GetValue(obj);
                    if (value == null)
                        value = Activator.CreateInstance(property.PropertyType); //.Assembly.FullName, property.Name
                    property.SetValue(obj, value);
                    ConfigToEntity(value, property.Name + ".");
                }
            }
        }

        /// <summary>
        /// ���������ݻ�д��appconfig
        /// </summary>
        public void Save()
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            EntityToConfig(this, settings);
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);

        }

        /// <summary>
        /// ʵ��ӳ�䵽appcinfig
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="settings"></param>
        /// <param name="perfix"></param>
        private void EntityToConfig<T>(T obj, KeyValueConfigurationCollection settings, string perfix = "") where T : class, new()
        {
            var properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.PropertyType.IsPrimitive())
                {
                    if (settings.AllKeys.Contains(perfix + property.Name))
                        settings[perfix + property.Name].Value = property.GetValue(obj)?.ToString() ?? "";
                    else
                        settings.Add(perfix + property.Name, property.GetValue(obj)?.ToString() ?? "");
                }
                else
                {
                    var value = property.GetValue(obj);
                    if (value == null)
                        value = Activator.CreateInstance(property.PropertyType); //.Assembly.FullName, property.Name
                    EntityToConfig(value, settings, property.Name + ".");
                }
            }
        }


    }

    public static class Ext
    {
        public static bool IsPrimitive(this Type type)
        {
            if (type.IsPrimitive) return true;
            if (type == typeof(DateTime)) return true;
            if (type == typeof(string)) return true;
            if (type == typeof(decimal)) return true;
            //TODO:���ܻ�����δ֪������Ҫ����

            return false;
        }


        /// <summary>
        /// ����ǿת��ֻ����˲������ͣ�
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <param name="input"></param>
        /// <param name="outType"></param>
        /// <returns></returns>
        public static object ConvertValue<TIn>(this TIn input, Type outType)
        {
            if (input == null)
                return input;

            var value = Convert.ChangeType(input, outType);
            return value;
            //if (outType == typeof(string))
            //{
            //    return input?.ToString() ?? "";
            //}
            //else if (outType == typeof(int))
            //{
            //    int result;
            //    int.TryParse(input.ToString(), out result);
            //    return result;
            //}
            //else if (outType == typeof(UInt32))
            //{
            //    UInt32 result;
            //    UInt32.TryParse(input.ToString(), out result);
            //    return result;
            //}
            //else if (outType == typeof(long))
            //{
            //    long result;
            //    long.TryParse(input.ToString(), out result);
            //    return result;
            //}
            //else if (outType == typeof(short))
            //{
            //    short result;
            //    short.TryParse(input.ToString(), out result);
            //    return result;
            //}
            //else if (outType == typeof(ushort))
            //{
            //    ushort result;
            //    ushort.TryParse(input.ToString(), out result);
            //    return result;
            //}
            //else if (outType == typeof(Guid))
            //{
            //    Guid result;
            //    Guid.TryParse(input.ToString(), out result);
            //    return result;
            //}
            //else if (outType == typeof(DateTime))
            //{
            //    DateTime result;
            //    DateTime.TryParse(input.ToString(), out result);
            //    return result;
            //}
            //else if (outType == typeof(bool))
            //{
            //    bool result;
            //    bool.TryParse(input.ToString(), out result);
            //    return result;
            //}
            //return input;
        }
    }
}