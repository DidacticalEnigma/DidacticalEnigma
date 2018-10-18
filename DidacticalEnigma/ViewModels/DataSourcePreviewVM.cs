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
        }

        public UsageDataSourcePreviewVM Parent { get; }

        private int selectedDataSourceIndex = -1;
        private DataSourceVM selectedDataSource;

        public int SelectedDataSourceIndex
        {
            get => selectedDataSourceIndex;
            set
            {
                if (selectedDataSourceIndex == value)
                    return;

                var oldSource = SelectedDataSource;
                selectedDataSourceIndex = value;
                SelectedDataSource = value != -1
                    ? Parent.DataSources[value]
                    : null;
                OnPropertyChanged();

                SelectedDataSource.IsUsed = true;
                if (oldSource != null)
                    oldSource.IsUsed = false;
            }
        }

        public DataSourceVM SelectedDataSource
        {
            get => selectedDataSource;
            private set
            {
                if (ReferenceEquals(value, selectedDataSource))
                {
                    return;
                }

                selectedDataSource = value;
                OnPropertyChanged();
            }
        }
    }
}