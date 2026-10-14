using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Entity;

namespace _523_Mozgovaya
{
    public partial class EventPage : Page
    {
        private Event currentEvent;

        public EventPage(Event ev)
        {
            InitializeComponent();
            currentEvent = ev;
            LoadEventData();
        }

        private void LoadEventData()
        {
            
            var fullEvent = Core.entities.Event.Include(e => e.Genre).FirstOrDefault(e => e.id_event == currentEvent.id_event);

          

           
            this.DataContext = fullEvent;
        }
    }
}
