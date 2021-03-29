using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeiEditor
{
    public enum enmOpMode
    {
        [Display(Name = "None")]
        [Description("None")]
        None = 0,
        [Display(Name = "CopyTagWithAttribs")]
        [Description("Copy tag with attributes")]
        CopyTagWithAttribs = 1,
        [Display(Name = "CopyTagWithNewAttribs")]
        [Description("Copy tag with new attribute")]
        CopyTagWithNewAttribs = 2,

        [Display(Name = "NewAttribLookup")]
        [Description("Add new attribute based on a different attribute lookup")]
        NewAttribLookup = 3,
        [Display(Name = "ChangeAttribLookup")]
        [Description("Change attribute value based on existing value lookup")]
        ChangeAttribLookup = 4,
        [Display(Name = "AttribLookupContents")]
        [Description("Add new attribute based on a tag contents lookup")]
        AttribLookupContents = 5
    }

    public enum enmPage
    {
        X2X = 0,
        X2T2X = 1
    }

    public class AppState
    {
        public string hiddenClass { get; private set; }
        public string tagName { get; set; }
        public string attribName { get; set; }
        public string newAttribName { get; set; }
        public int attribStartVal { get; set; }
        public enmOpMode OpMode { get; set; }
        public enmPage PageMode { get; set; }
        public string Message { get; set; }
        private int processCount;


        public event Action OnChange;

        public AppState()
        {
            hiddenClass = "hidden";
            processCount = 0;

            tagName = "placeName";
            attribName = "xml:id";
            newAttribName = "ref";
            attribStartVal = 1;
        }

        public void isWorking()
        {
            processCount = processCount + 1;
            hiddenClass = processCount > 0 ? "" : "hidden";
            NotifyStateChanged();
        }
        public void notWorking()
        {
            processCount = processCount - 1;
            hiddenClass = processCount > 0 ? "" : "hidden";
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
