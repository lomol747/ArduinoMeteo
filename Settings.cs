




namespace ArduinoMeteo
{
    /// <summary>
    /// Класс настроек для  метеостанции
    /// </summary>
    public class Settings
    {


        private string portName = "COM13";  //наименование порта
        private int baudRate = 9600;        //скорость в бодах
        private string timeUpdate = "1000";      //время обновления данных. Посылается в микроконтролёр. Используется стринговое значение, т.к. в порт посылается этот тип данных

        //*************************************************************
        public string getPortName() { return portName; }
        public int getBaudRate() { return baudRate; }
        public string getTimeUpdate() { return timeUpdate; }

        public void setPortName(string _portName)   { portName = _portName; }
        public void setBaudRate(int _baudRate)      { baudRate = _baudRate; }
        public void setTimeUpdate(string _timeUpdate)   { timeUpdate = _timeUpdate; }
        //


    }
}
