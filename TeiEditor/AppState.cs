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
        
        //Xml2Xml
        [Display(Name = "CopyTagWithAttribs")]
        [Description("Copy tag with attributes")]
        CopyTagWithAttribs = 1,
        
        [Display(Name = "CopyTagWithNewAttribs")]
        [Description("Copy tag with new attribute")]
        CopyTagWithNewAttribs = 2,
        
        [Display(Name = "AlignSelectedTag")]
        [Description("Align clicked tag on both windows according to attribute value")]
        AlignSelectedTag = 7,

        [Display(Name = "FindNoCorresponding")]
        [Description("Highlight tags that don't have a corresponding value")]
        FindNoCorresponding = 8,

        [Display(Name = "CopyAttribute")]
        [Description("Copy attribute from tag on source to tag on target")]
        CopyAttribute = 9,

        //Xml2Tsv2Xml
        [Display(Name = "NewAttribLookup")]
        [Description("Add a new attribute with values based on the value of a selected attribute")]
        NewAttribLookup = 3,
        
        [Display(Name = "ChangeAttribLookup")]
        [Description("Replace the value of a selected attribute in a selected element with a new value")]
        ChangeAttribLookup = 4,
        
        [Display(Name = "AttribLookupContents")]
        [Description("Add a new attribute with values based on the content of a selected element")]
        AttribLookupContents = 5,

        //xml:id
        [Display(Name = "AddUniqueAttribVal")]
        [Description("Add a new attribute with unique values")]
        AddUniqueAttribVal = 6
    }

    public enum enmPage
    {
        X2X = 0,
        X2T2X = 1,
        XmlId = 2
    }

    public class AppState
    {
        public List<string> Alerts { get; set; }
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
            
            Alerts = new List<string>();
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
