using System.Collections.ObjectModel;

namespace Innowise.Music.Model
{
    public class EventGroup : ObservableCollection<MusicEvent>
    {
        public string Month { get; }

        public EventGroup(string month)
        {
            Month = month;
        }
    }
}
