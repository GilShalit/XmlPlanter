using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeiEditor
{
    public enum enmX2XMode
    {
        None = 0,
        CopyTagWithAttribs = 1,
        CopyTagWithNewAttribs = 2

    }

    public class AppState
    {
        public string hiddenClass { get; private set; }
        public string tagName { get; set; }
        public string attribName { get; set; }
        public enmX2XMode X2XMode { get; set; }
        private int count;

        public event Action OnChange;

        public AppState()
        {
            hiddenClass = "hidden";
            count = 0;

            tagName = "seg";
            attribName = "xml:id";
            X2XMode = enmX2XMode.None;
        }

        public void isWorking()
        {
            count = count + 1;
            hiddenClass = count > 0 ? "" : "hidden";
            NotifyStateChanged();
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
