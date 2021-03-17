using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeiEditor
{
    public enum enmX2XMode
    {
        [Display(Name = "None")]
        [Description("None")]
        None = 0,
        [Display(Name = "CopyTagWithAttribs")]
        [Description("Copy tag with attributes")]
        CopyTagWithAttribs = 1,
        [Display(Name = "CopyTagWithNewAttribs")]
        [Description("Copy tag with new attribute")]
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
