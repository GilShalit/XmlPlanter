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
using TeiEditor.Pages;
using TeiEditor.Shared;
using Microsoft.JSInterop;

namespace TeiEditor
{
    public enum enmTagChanges
    {
        DoNothing = 0,
        OpenTag,
        CloseTag
    }

    public enum enmStatusColor
    {
        Done = 0,
        Current = 1
    }

    public static class Helpers
    {
        static public List<string> validationErrors = new List<string>();
        static public XmlSchemaSet schemaSet = new XmlSchemaSet();
        static private string currentRangeId;
        static public KeyValuePair<string, BlazorMonaco.Range> currentDec;

        public static bool IsPosInRange(Position position,BlazorMonaco.Range range)
        {
            //before or after range lines
            if (position.LineNumber < range.StartLineNumber || position.LineNumber > range.EndLineNumber) return false;
            //on first range line before range start column 
            if (position.LineNumber == range.StartLineNumber && position.Column < range.StartColumn) return false;
            //on last range line after last range column
            if (position.LineNumber == range.EndLineNumber && position.Column > range.EndColumn) return false;
            return true; //all else
        }

        public static async Task markWholeTag(
            string tagName,
            EditorMouseEvent eventArg, MonacoEditor editor,
            Dictionary<string, BlazorMonaco.Range> sourceDecorations)
        {
            {
                if (eventArg.Event.LeftButton)
                {
                    Position pClick = eventArg.Target.Position;
                    KeyValuePair<string, BlazorMonaco.Range> dec = (from d in sourceDecorations
                                                                    where IsPosInRange(pClick,d.Value)
                                                                    select d).FirstOrDefault();

                    if (string.IsNullOrEmpty(dec.Key))
                    {
                        //clicking outside a marked tag - clear previous selection in currentRangeID
                        await Helpers.RemoveDecoration(editor, currentRangeId);
                        currentDec = new KeyValuePair<string, BlazorMonaco.Range>();
                    }
                    else
                    {
                        TextModel sourceModel = await editor.GetModel();
                        string tag = await sourceModel.GetValueInRange(dec.Value, EndOfLinePreference.CRLF);

                        Position pEnd;
                        if (IsClosedTag(tag)) pEnd = new Position() { Column = dec.Value.EndColumn, LineNumber = dec.Value.EndLineNumber };
                        else pEnd = await getEndOfTag(tagName, dec.Value, sourceModel);

                        BlazorMonaco.Range range = new BlazorMonaco.Range()
                        {
                            StartColumn = dec.Value.StartColumn,
                            StartLineNumber = dec.Value.StartLineNumber,
                            EndColumn = pEnd.Column,
                            EndLineNumber = pEnd.LineNumber
                        };
                        currentRangeId = await Helpers.ColorRange(editor, range, currentRangeId, enmStatusColor.Current);
                        currentDec = dec;
                    }

                }
            }

        }

        public static async Task Download(string filename, AppState AppState, IModalService Modal, MonacoEditor editor, IJSRuntime js, bool twoEditorForm)
        {
            AppState.Message = filename;
            ModalOptions options = new ModalOptions() { HideCloseButton = true };
            var FileNameForm = Modal.Show<FileNameForm>(title: "", options);
            var result = await FileNameForm.Result;
            if (result.Cancelled) return;

            if (await js.InvokeAsync<int>("isChrome") > -1)
            {
                if (twoEditorForm)
                {
                    var w21 = await js.InvokeAsync<string>("getWidth", "editor-source");
                    var h21 = await js.InvokeAsync<string>("getHeight", "editor-source");
                    await js.InvokeVoidAsync("setSize", "editor-source", w21, h21);
                    var w22 = await js.InvokeAsync<string>("getWidth", "editor-target");
                    var h22 = await js.InvokeAsync<string>("getHeight", "editor-target");
                    await js.InvokeVoidAsync("setSize", "editor-target", w22, h22);
                }
                else
                {
                    var w11 = await js.InvokeAsync<string>("getWidth", "editor-lookup");
                    var h11 = await js.InvokeAsync<string>("getHeight", "editor-lookup");
                    await js.InvokeVoidAsync("setSize", "editor-lookup", w11, h11);

                }
            }

            byte[] file = System.Text.Encoding.UTF8.GetBytes(await editor.GetValue());
            await js.InvokeVoidAsync("BlazorDownloadFile", result.Data?.ToString() ?? string.Empty, "text/xml", file);
        }

        public static async Task<Position> getEndOfTag(string tagName, BlazorMonaco.Range tagRange, TextModel model)
        {
            string endTag = $"</{tagName}>";
            int l = tagRange.EndLineNumber;
            List<string> lines = await model.GetLinesContent();
            int endTagCol = -1;
            while (l < lines.Count)
            {//problem if previous tag closes on the same line before the start of current tag
                if (l == tagRange.EndLineNumber) endTagCol = lines[l - 1].IndexOf(endTag, tagRange.EndColumn - 1);//searching on first line
                else endTagCol = lines[l - 1].IndexOf(endTag);

                if (endTagCol > -1 ) break;
                l++;
            }
            return new Position() { Column = endTagCol + endTag.Length + 1, LineNumber = l };
        }

        public static bool IsClosedTag(string tag)
        {
            return tag.IndexOf("/>") != -1;
        }

        public static async Task markTags(MonacoEditor editor, string tag,
            Dictionary<string, BlazorMonaco.Range> sourceDecorations)
        {
            sourceDecorations.Clear();
            TextModel sourceModel = await editor.GetModel();
            List<FindMatch> sourceMatches;
            await editor.ResetDeltaDecorations();
            List<ModelDeltaDecoration> lstDecorations = new List<ModelDeltaDecoration>();
            sourceMatches = await sourceModel.FindMatches($"<{tag}", false, false, false, null, true, 10000);
            if (sourceMatches.Count > 0)
            {
                foreach (FindMatch m in sourceMatches)
                {
                    m.Range = await Helpers.ExpandTagRange(m.Range, sourceModel);
                    lstDecorations.Add(new ModelDeltaDecoration
                    {
                        Range = m.Range,
                        Options = new ModelDecorationOptions
                        {
                            IsWholeLine = false,
                            ClassName = "decorationContent",
                            GlyphMarginClassName = "decorationGlyphMargin",
                            Minimap = new ModelDecorationMinimapOptions() { Position = MinimapPosition.Inline, Color = "#FFFF00" }//#90EE90 #FFFFFE
                        }
                    });
                }
                string[] decorations = await editor.DeltaDecorations(null, lstDecorations.ToArray());
                for (int i = 0; i < sourceMatches.Count; i++)
                {
                    sourceDecorations.Add(decorations[i], sourceMatches[i].Range);
                }
                await editor.RevealLineInCenter(sourceMatches[0].Range.StartLineNumber);
            }
        }

        public static async Task RemoveDecoration(MonacoEditor editor, string id)
        {
            string[] targetID = new string[] { id };
            List<ModelDeltaDecoration> emptyDec = new List<ModelDeltaDecoration>() { };
            await editor.DeltaDecorations(targetID, emptyDec.ToArray());
        }

        public static async Task<string> ColorRange(MonacoEditor editor, BlazorMonaco.Range range, string id = "", enmStatusColor statusColor = enmStatusColor.Done)
        {
            string contentClass = "";
            string GlyphMarginClass = "";
            string MiniMapColor = "";

            switch (statusColor)
            {
                case enmStatusColor.Done:
                    contentClass = "decorationContentDone";
                    GlyphMarginClass = "decorationGlyphMarginDone";
                    MiniMapColor = "#90EE90";
                    break;
                case enmStatusColor.Current:
                    contentClass = "decorationContentCurrent";
                    GlyphMarginClass = "decorationGlyphMarginCurrent";
                    MiniMapColor = "#bbfaf9";
                    break;
            }

            string[] targetID = new string[] { };
            if (!string.IsNullOrEmpty(id))
            {
                targetID = new string[] { id };
            }

            List<ModelDeltaDecoration> newDec = new List<ModelDeltaDecoration>(){
                new ModelDeltaDecoration
                {
                    Range = range,
                    Options = new ModelDecorationOptions
                    {
                        IsWholeLine = false,
                        ClassName = contentClass,
                        GlyphMarginClassName = GlyphMarginClass,
                        Minimap = new ModelDecorationMinimapOptions() { Position = MinimapPosition.Inline, Color = MiniMapColor }
                    }
                } };

            string[] decorations = await editor.DeltaDecorations(targetID, newDec.ToArray());
            return decorations[0];
        }

        public static JsonElement TagToJson(string Text, enmTagChanges tagChange)
        {
            switch (tagChange)
            {
                case enmTagChanges.OpenTag:
                    Text = Text.Replace("/>", ">");
                    break;
                case enmTagChanges.CloseTag:
                    if (Text.IndexOf("/") == -1) Text = Text.Replace(">", "/>");
                    break;
                case enmTagChanges.DoNothing:
                    break;
            }
            Text = Text.Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
            var t = $"{{\"text\": \"{Text}\"}}";
            JsonDocument doc;
            doc = JsonDocument.Parse(t);
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
            Position endPos = await getClosingPosition(model, matchRange.EndLineNumber, matchRange.EndColumn - 1);
            newRange.EndColumn = endPos.Column;
            newRange.EndLineNumber = endPos.LineNumber;

            return newRange;

            //local non static recursive method 
            async Task<Position> getClosingPosition(TextModel model, int lineNum, int colNum)
            {
                Position pos = new Position();
                string line = await model.GetLineContent(lineNum);
                int tagCloseCol = line.IndexOf(">", colNum);
                if (tagCloseCol == -1) //not found
                {
                    pos = await getClosingPosition(model, lineNum + 1, 0);
                }
                else
                {
                    pos.Column = tagCloseCol + 2;
                    pos.LineNumber = lineNum;
                }
                return pos;
            }
        }


        public static async Task LoadXMLfromFile(MonacoEditor editor, IBrowserFile file)
        {
            byte[] bytesOfXML = new byte[file.Size];
            using (Stream strm = file.OpenReadStream())
            {
                await strm.ReadAsync(bytesOfXML);
            }
            string stringOfXML = Encoding.UTF8.GetString(bytesOfXML);
            TextModel model = await MonacoEditorBase.CreateModel(stringOfXML, "xml");
            await editor.SetModel(model);

            //List<string> lLines = stringOfXML.Split("\n").ToList<string>();
        }

        public static void ShowValModal(IModalService Modal)
        {
            if (validationErrors.Count > 0)
            {
                string msg = "";
                foreach (string s in Helpers.validationErrors)
                {
                    msg = msg + s + Environment.NewLine;
                }
                ShowModal(msg, Modal);
            }
            else ShowModal("XML is Valid", Modal);
        }

        public static void ShowModal(string msg, IModalService Modal)
        {
            ModalOptions options = new ModalOptions() { HideCloseButton = true };
            Modal.Show<Confirm>(msg, options);
        }

        public static async Task loadSchema(string path, string nameSpace, HttpClient _client)
        {
            byte[] byteArrayS = await _client.GetByteArrayAsync(path);
            //Console.WriteLine($"{path}: {byteArrayS.Length}");
            MemoryStream streamS = new MemoryStream(byteArrayS);
            XmlReader xmlSchemaReader = XmlReader.Create(streamS);
            schemaSet.Add(nameSpace, xmlSchemaReader);
        }

        public static async Task ensureLoadedSchemas(HttpClient _client)
        {
            if (schemaSet.Count == 0)
            {
                await loadSchema("data/tei_all.xsd", "http://www.tei-c.org/ns/1.0", _client);
                await loadSchema("data/tei_all_dcr.xsd", "http://www.isocat.org/ns/dcr", _client);
                await loadSchema("data/tei_all_teix.xsd", "http://www.tei-c.org/ns/Examples", _client);
                await loadSchema("data/tei_all_xml.xsd", "http://www.w3.org/XML/1998/namespace", _client);
            }
        }

        public static async Task ValidateXML(AppState appState, IModalService Modal,
            MonacoEditor editor, ValidationEventHandler eventHandler)
        {
            try
            {
                appState.isWorking();
                Helpers.validationErrors.Clear();
                await Task.Delay(1);

                byte[] byteArrayX = Encoding.ASCII.GetBytes(await editor.GetValue());
                MemoryStream streamX = new MemoryStream(byteArrayX);
                XmlReader reader = XmlReader.Create(streamX);

                XmlDocument document = new XmlDocument();
                document.Load(reader);
                document.Schemas = schemaSet;

                document.Validate(eventHandler);
            }
            catch (Exception e)
            {
                Helpers.validationErrors.Add(e.Message);
                //throw;
            }
            finally
            {
                appState.notWorking();
                ShowValModal(Modal);
            }
        }
    }
}
