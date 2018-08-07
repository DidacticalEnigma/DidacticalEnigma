using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DidacticalEnigma.ViewModels
{
    public class DataSourcePreviewVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DataSourcePreviewVM(UsageDataSourcePreviewVM parent, int index = -1)
        {
            Parent = parent;
            SelectedDataSourceIndex = index;
            if(index != -1)
            {
                SelectedDataSource = Parent.DataSources[index];
            }
        }

        public UsageDataSourcePreviewVM Parent { get; }

        private int selectedDataSourceIndex = -1;
        public int SelectedDataSourceIndex
        {
            get => selectedDataSourceIndex;
            set
            {
                if (selectedDataSourceIndex == value)
                    return;

                selectedDataSourceIndex = value;
                OnPropertyChanged();
            }
        }

        private DataSourceVM selectedDataSource = null;
        public DataSourceVM SelectedDataSource
        {
            get => selectedDataSource;
            set
            {
                if (selectedDataSource == value)
                    return;
                var oldSource = selectedDataSource;
                selectedDataSource = value;
                OnPropertyChanged();
                value.IsUsed = true;
                if(oldSource != null)
                    oldSource.IsUsed = false;
            }
        }
    }
}