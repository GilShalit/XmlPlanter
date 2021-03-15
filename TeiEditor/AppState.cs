using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeiEditor
{
    public class AppState
    {
        public string hiddenClass { get; private set; }
        public string tagName { get; set; }
        public string attribName { get; set; }
        private int count;

        public event Action OnChange;

        public AppState()
        {
            hiddenClass = "hidden";
            tagName = "seg";
            attribName = "xml:id";

            count = 0;
            //Console.WriteLine($"CTOR {hiddenClass} {count}");
        }

        public void isWorking()
        {
            count = count + 1;
            hiddenClass = count > 0 ? "" : "hidden";
            NotifyStateChanged();
            //Console.WriteLine($"is Working {hiddenClass} {count}");
        }
        public void notWorking()
        {
            count = count - 1;
            hiddenClass = count > 0 ? "" : "hidden";
            NotifyStateChanged();
            //Console.WriteLine($"not Working {hiddenClass} {count}");
        }

        private void NotifyStateChanged()
        {
            OnChange?.Invoke();

            //Console.WriteLine("Onchange");
        }
    }
}
