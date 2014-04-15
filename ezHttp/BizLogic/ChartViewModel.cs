using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ezHttp
{
    public class ChartViewModel
    {
        public ObservableCollection<ChartInfo> CpuInfo { get; private set; }
        public ObservableCollection<ChartInfo> ConnectionInfo { get; private set; }
        public ObservableCollection<ChartInfo> FileBufferInfo { get; private set; }
        public ChartViewModel()
        {
            ConnectionInfo = new ObservableCollection<ChartInfo>();
            CpuInfo = new ObservableCollection<ChartInfo>();
            FileBufferInfo = new ObservableCollection<ChartInfo>();

            CpuInfo.Add(new ChartInfo() { Category = "CPU Usage", Number = 1 });
            ConnectionInfo.Add(new ChartInfo() { Category = "Connection Pool", Number = 1 });
            FileBufferInfo.Add(new ChartInfo() { Category = "File Buffer Usage", Number = 1 });
        }

        private object selectedItem = null;
        public object SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                // selected item has changed
                selectedItem = value;
            }
        }
    }

    // class which represent a data point in the chart
    public class ChartInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int _number ;
        public string Category { get; set; }

        public int Number
        {
            get
            {
                return _number;
            }
            set
            {
                _number = value;
                if (PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Number"));
                }
            }
        }
    }
}
