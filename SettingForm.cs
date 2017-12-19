using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;

namespace ArduinoMeteo
{
    public partial class SettingForm : Form
    {
        Settings settings;

        public SettingForm(Settings _settings)
        {
            InitializeComponent();
            settings = _settings;   //настройки


            
            cmbPort.Items.AddRange(SerialPort.GetPortNames());  //считываем массив ком портов и сразу задаём комбобокс

            cmbPort.SelectedIndex = cmbPort.Items.IndexOf(settings.getPortName().ToString());       //получаем ком порт из настроек, получаем индекс в элементе, задаём по индексу компорт
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(settings.getBaudRate().ToString());   //аналогично строке выше
            

            txtbxTime.Text = settings.getTimeUpdate();  //интервал получения данных

        }

        
        



        private void SettingForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            settings.setPortName(cmbPort.Text);                    //порт
            settings.setBaudRate(Int32.Parse(comboBox1.Text));      //скорость
            settings.setTimeUpdate(txtbxTime.Text);    //интервал полуения данных

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
