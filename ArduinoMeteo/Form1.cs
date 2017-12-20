﻿using System;
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
using System.Windows.Forms.DataVisualization.Charting;


namespace ArduinoMeteo
{
    public partial class Form1 : Form
    {

        Settings settings;  //настройки

        //Для таймер-пинга
        private bool fPing = true;
        int s = 0;
        //********************************
        private bool fChart = false;



        public Form1()
        {
            InitializeComponent();

            settings = new Settings();  //инициализация класса настроек

            initSerialPort();           //функция инициализации порта
            timer1.Start();             //старт таймера для пинга


            startCharts(chartCurrTemp, Color.Red);
            startCharts(chartCurrPressure, Color.Blue);
            //startChart();

        }

        private void initSerialPort ()
        {
            lblPort.Text = settings.getPortName();    //выводим текущий ком порт

            //
            try
            {
                if (serialPort1.IsOpen) //если ранее порт был открыт, то закрываем. Используется для изменения настроек
                {
                    serialPort1.DiscardInBuffer();  //их наличие под вопросом
                    serialPort1.DiscardOutBuffer(); //их наличие под вопросом

                    serialPort1.Close();
                    
                }

                serialPort1.PortName = settings.getPortName();  //имя порта
                serialPort1.BaudRate = settings.getBaudRate();  //скорость в бодах
                serialPort1.DtrEnable = true;                   //готовность для обмена данными
                serialPort1.Open();                             //открываем последовательное соединение

                serialPort1.DataReceived += SerialPort1_DataReceived;
                serialPort1.DataReceived += SerialPort1_DataReceived1;
                
                lblPort.ForeColor = Color.Green;    //текущий ком порт внизу окна
            }
            catch
            {
                lblPort.ForeColor = Color.Red;      //текущий копм порт внизу окна
                
                MessageBox.Show("Отсутствует подключение к устройству", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);  //исключение для ошибки
            }
        }


        //поток ком порта
        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e) //для давления
        {

            try
            {
                string pressure = serialPort1.ReadLine();   //получаем строку. Должна быть влажность
                

                this.BeginInvoke(new LineReceivedEvent(LineReceived), pressure);    //выполнение делегата

                label3.ForeColor = Color.Red;   //пинг
                label3.BackColor = Color.Red;
                fPing = true;                   //флаг для таймера
            

            }
            catch   //КОСТЫЛЬ!!! Будет выпадать ошибка, не выяснено почему, скорее всего что-то со строками и буфером. Не критично, код дальше продолжает выполняться
            {
                //MessageBox.Show("Ошибка получения давления", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);  //исключение для ошибки
            }
        }

        private void SerialPort1_DataReceived1(object sender, SerialDataReceivedEventArgs e)    //для температуры
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    string temp = serialPort1.ReadLine();
                    this.BeginInvoke(new LineReceivedEvent1(LineReceived1), temp);
                }
            }
            catch   //КОСТЫЛЬ!!! Будет выпадать ошибка, не выяснено почему, скорее всего что-то со строками и буфером. Не критично, код дальше продолжает выполняться
            {
                //MessageBox.Show("Ошибка получения температуры", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);  //исключение для ошибки
            }
        }
        //////////////////////////////////////////////////



        //Запись в файл
        int countPressure = 0;
        private delegate void LineReceivedEvent(string pressure);   //Запись давления
        private void LineReceived(string pressure)
        {
            textBox1.Text = pressure;                       //отображение значения
            string path = settings.dbPressure();            //Наименование файла с логами
            string date = DateTime.Now.ToString();          //получаем текущую дату


            //chartCurrPressure.ChartAreas[0].AxisX.ScaleView.Zoom(0, countTemp);
            chartCurrPressure.Series[0].Points.AddXY(date, pressure);
            scaleChart(chartCurrPressure);
            //chart1.Series[0].Points.RemoveAt(0);
            countPressure++;

            using (StreamWriter sw = File.AppendText(path)) //директива для записи в файл
            {
                sw.WriteLine(pressure);     //запись давления
                sw.WriteLine(date);         //запись времени
            }
        }


        int countTemp = 0;
        private delegate void LineReceivedEvent1(string temp);  //Температура. Аналогично давлению
        private void LineReceived1(string temp)
        {
            textBox2.Text = temp;
            string path = "Лог температуры.txt";            //наименование файла с логами
            string date = DateTime.Now.ToString();


            //chartCurrTemp.ChartAreas[0].AxisX.ScaleView.Zoom(0, countTemp);
            chartCurrTemp.Series[0].Points.AddXY(date, temp);
            scaleChart(chartCurrTemp);
            //chart1.Series[0].Points.RemoveAt(0);
            countTemp++;

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(temp);
                sw.WriteLine(date);
            }
            
        }




        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)  //Работа с графиком. Нихера не понятно
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                chart1.ChartAreas[0].AxisX.ScaleView.Zoom(0, 100);
                chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
                chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
                chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
                chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;

                StreamReader streamReader = new StreamReader(openFileDialog1.FileName);
                chart1.Series[0].Points.Clear();

                while (!streamReader.EndOfStream)
                {
                    string Y = streamReader.ReadLine();
                    string X = streamReader.ReadLine();

                    chart1.Series[0].Color = Color.Red;
                    chart1.Series[0].BorderWidth = 1;
                    chart1.Series[0].Points.AddXY(X, Y);
                }
                streamReader.Close();
            }
        }

        private delegate void chartRefresh();

        

        private void startChart ()  //графики на второй вкладке
        {
            
            //chart1.ChartAreas[0].AxisX.ScaleView.Zoom(0, 200);
            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            chart1.ChartAreas[0].AxisY.Interval = 1;

            StreamReader streamReader = new StreamReader(settings.dbTemp());
            //chart1.Series[0].Points.Clear();

            while (!streamReader.EndOfStream)
            {
                string Y = streamReader.ReadLine();
                string X = streamReader.ReadLine();

                chart1.Series[0].Color = Color.Red;
                chart1.Series[0].BorderWidth = 1;
                chart1.Series[0].Points.AddXY(X, Y);
            }
            streamReader.Close();




            //chart3.ChartAreas[0].AxisX.ScaleView.Zoom(0, 200);
            chart3.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart3.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart3.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart3.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            //chart3.ChartAreas[0].AxisY.Interval = 0.5;

            streamReader = new StreamReader(settings.dbPressure());
            //chart3.Series[0].Points.Clear();

            while (!streamReader.EndOfStream)
            {
                string Y = streamReader.ReadLine();
                string X = streamReader.ReadLine();

                chart3.Series[0].Color = Color.Blue;
                chart3.Series[0].BorderWidth = 1;
                chart3.Series[0].Points.AddXY(X, Y);
            }
            streamReader.Close();

            //var points = chart1.Series[0].Points;
            //chart1.ChartAreas[0].AxisY.Minimum = Math.Floor(points.Min(x => x.YValues[0]));      //минимальное значение Y. Округление в меньшую сторону
            //chart1.ChartAreas[0].AxisY.Maximum = Math.Ceiling(points.Max(x => x.YValues[0]));    //макимальное значение Y. Округление в большую сторону
            //chart1.ChartAreas[0].AxisX.ScaleView.Zoom(0, points.Count());

            //chart1.chang
            scaleChart(chart1);
            scaleChart(chart3);
            fChart = true;

        }

        //************************ПИНГ - ТАЙМЕР
        private void timer1_Tick(object sender, EventArgs e)    //функция таймера
        {
            if (fPing && s <= 10)   
            {
                //label3.Text = s.ToString();
                s++;
            }
            else
            {
                fPing = false;
                s = 0;
                label3.ForeColor = Color.Black;
                label3.BackColor = Color.Black;
            }

            if (chart1.Visible)
                button3.Text = "Отображается";

            //if (fChart)
            //{
            //    scaleChart(chart1);
            //    scaleChart(chart3);
            //    fChart = false;
            //}

        }



        /// /////////////////////////////////////////////////////////////Меню
        /// //////////Кнопка настроек
        private void settingToolStripMenuItem_Click(object sender, EventArgs e) //кнопка 
        {
            SettingForm setForm = new SettingForm(settings);
            setForm.ShowDialog();                               //вызов окна
            if (setForm.DialogResult == DialogResult.OK)        //ожидание положительного ответа
            {
                initSerialPort();           //переинициализация ком порта

                //КОСТЫЛЯТИНА. Принудительная задержка, пока инициализируется порт и желать изволить принимать данные в порт.
                //var c = Task.Run(async delegate { await Task.Delay(1700); return true; }); //https://msdn.microsoft.com/ru-ru/library/hh194873(v=vs.110).aspx
                //c.Wait();
                //if (c.Result)

                System.Threading.Thread.Sleep(1650);        // упрощённая задержка по сравнению с верхними строками
                serialPort1.Write(settings.getTimeUpdate());    //сама передач в порт
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            startChart();
        }

        private void button3_Click(object sender, EventArgs e)  ///тестовая кнопка
        {
            //scaleChart(chart1); 
            //scaleChart(chart3);

            //chartRefresh cartRefresh = new chartRefresh(startChart);
            //cartRefresh();
            //chart1.BeginInvoke(new chartRefresh(startChart));
            chart1.Invoke(new Action(() => { startChart(); }));
        }

        //Таймер 2
        private void timer2_Tick(object sender, EventArgs e)
        {
            chart1.Series[0].Points.RemoveAt(0);


        }

        ///////////////Функции
        private void scaleChart (Chart chart)   //автомасштабированиеи автоскалирование графика
        {
            var points = chart.Series[0].Points;
            chart.ChartAreas[0].AxisY.Minimum = Math.Floor(points.Min(x => x.YValues[0]));      //минимальное значение Y. Округление в меньшую сторону
            chart.ChartAreas[0].AxisY.Maximum = Math.Ceiling(points.Max(x => x.YValues[0]));    //макимальное значение Y. Округление в большую сторону
            chart.ChartAreas[0].AxisX.ScaleView.Zoom(0, points.Count());                        //используется для автоскалированич
        }

        private void startCharts(Chart chart, Color color)  //стартование графика для онлайн отображения
        {
            //chart.ChartAreas[0].AxisX.ScaleView.Zoom(0, 200);
            chart.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            //chart.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            chart.ChartAreas[0].AxisY.Interval = 1;
            chart.ChartAreas[0].AxisX.Interval = 60;

            chart.Series[0].Color = color;
            chart.Series[0].BorderWidth = 1;
        }


    }
}
