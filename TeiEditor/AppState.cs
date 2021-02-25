using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeiEditor
{
    public class AppState
    {
        public string hiddenClass { get; set; }
        private int count;

        public event Action OnChange;

        public  AppState()
        {
            hiddenClass = "hidden";
            count = 0;
        }

        public void isWorking()
        {
            count++;
            hiddenClass = count > 0 ? "" : "hidden";
            NotifyStateChanged();
        }
        public void notWorking()
        {
            count--;
            hiddenClass = count > 0 ? "" : "hidden";
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
