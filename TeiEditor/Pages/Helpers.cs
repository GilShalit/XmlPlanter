using Blazored.Modal;
using Blazored.Modal.Services;
using BlazorMonaco;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeiEditor.Pages
{
    enum enmX2XMode
    {
        None = 0,
        CopyTagWithAttribs = 1

    }

    public static class Helpers
    {
        public static JsonElement TextToJson(string Text)
        {
            Text = Text.Replace("\"", "\\\"");
            var t= $"{{\"text\": \"{Text}\"}}";
            JsonDocument doc;
            try
            {
                 doc = JsonDocument.Parse(t);
            }
            catch (Exception e)
            {
              
                throw;
            }
            return doc.RootElement;
        }
        public static async Task<BlazorMonaco.Range> ExpandTagRange(BlazorMonaco.Range matchRange, TextModel model)
        {
            BlazorMonaco.Range newRange = new BlazorMonaco.Range()
            {
                StartColumn = matchRange.StartColumn,
                StartLineNumber = matchRange.StartLineNumber
            };

            int tagCloseLine = matchRange.EndLineNumber;
            string line = await model.GetLineContent(tagCloseLine);
            int tagCloseCol = line.IndexOf(">");
            if (tagCloseCol == -1) //not found
            {
                tagCloseLine++;
                line = await model.GetLineContent(tagCloseLine);
                tagCloseCol = line.IndexOf(">");
            }
            tagCloseCol = tagCloseCol + 2;

            newRange.EndColumn = tagCloseCol;
            newRange.EndLineNumber = tagCloseLine;

            return newRange;
        }

        public static async Task<List<string>> LoadXMLfromFile(MonacoEditor editor, IBrowserFile file)
        {
            byte[] bytesOfXML = new byte[file.Size];
            using (Stream strm = file.OpenReadStream())
            {
                await strm.ReadAsync(bytesOfXML);
            }
            string stringOfXML = Encoding.UTF8.GetString(bytesOfXML);
            TextModel model = await MonacoEditorBase.CreateModel(stringOfXML, "xml");
            await editor.SetModel(model);

            List<string> lLines = stringOfXML.Split("\n").ToList<string>();
            return lLines;
        }

        public static void ShowModal(string msg, IModalService Modal)
        {
            ModalOptions options = new ModalOptions() { HideCloseButton = true };
            Modal.Show<Confirm>(msg, options);
        }

        public static async Task loadSchema(string path, string nameSpace, XmlSchemaSet schemaSet, HttpClient _client)
        {
            byte[] byteArrayS = await _client.GetByteArrayAsync(path);
            //Console.WriteLine($"{path}: {byteArrayS.Length}");
            MemoryStream streamS = new MemoryStream(byteArrayS);
            XmlReader xmlSchemaReader = XmlReader.Create(streamS);
            schemaSet.Add(nameSpace, xmlSchemaReader);
        }
    }
}
