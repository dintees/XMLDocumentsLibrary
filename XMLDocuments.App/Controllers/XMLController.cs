using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using XMLDocumentLibrary;
using XMLDocumentLibrary.Models;
using XMLDocuments.App.Models.XML;

namespace XMLDocuments.App.Controllers
{
    public class XMLController : Controller
    {
        private readonly IXMLService _xService;
        public XMLController(IXMLService xService)
        {
            _xService = xService;
        }

        public ActionResult Index()
        {
            var allDocs = _xService.GetAllDocuments();
            return View(allDocs);
        }

        public ActionResult Edit(int id)
        {
            var doc = _xService.GetDocumentById(id);
            return View(doc);
        }

        public ActionResult View(int id)
        {
            try
            {
                var doc = _xService.GetDocumentById(id);
                // formatting XML document
                XmlDocument tmp = new XmlDocument();
                StringWriter sw = new StringWriter();
                tmp.LoadXml(doc.XMLDocument!);
                tmp.Save(sw);
                doc.XMLDocument = sw.ToString();
                return View(doc);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateXMLDocumentModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                string XMLContent;
                if (model.CreateFromFile)
                {
                    var res = new StringBuilder();
                    using (var reader = new StreamReader(model.File!.OpenReadStream()))
                    {
                        while (reader.Peek() >= 0)
                            res.AppendLine(reader.ReadLine());
                    }
                    XMLContent = res.ToString();
                }
                else XMLContent = model.XMLDocument!;
                int createdId = _xService.CreateDocument(model.Title!, model.Description!, XMLContent);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(XMLDoc doc)
        {
            if (doc != null)
            {
                try
                {
                    bool isModified = _xService.ModifyDocument(doc.Id, doc.Title, doc.Description, doc.XMLDocument);
                    if (isModified) return RedirectToAction("Index");
                    else
                    {
                        ViewBag.Error = "An error occured during editing";
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = ex.Message;
                    return View();
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            bool isDeleted = _xService.DeleteDocumentById(id);
            if (isDeleted) return RedirectToAction("Index");
            else
            {
                TempData["Error"] = "An error occured during deleting";
                return RedirectToAction("Index");
            }
        }

        public ActionResult XMLView()
        {
            var docs = _xService.GetAllDocuments();
            if (docs is not null)
            {
                List<SelectListItem> list = new List<SelectListItem>();
                foreach (var row in docs) list.Add(new SelectListItem { Text = row.Title, Value = row.Id.ToString() });
                ViewBag.XMLDocuments = list;
            }
            return View();
        }

        [HttpPost]
        public ActionResult XMLView(XMLViewModel model)
        {
            var docs = _xService.GetAllDocuments();
            if (docs is not null)
            {
                List<SelectListItem> list = new List<SelectListItem>();
                foreach (var row in docs) list.Add(new SelectListItem { Text = row.Title, Value = row.Id.ToString() });
                ViewBag.XMLDocuments = list;
            }


            // Select with functionalities
            string[] functionNames = { "CheckNodeIfExists", "GetNodeText", "GetNodes", "GetAllDocumentNodesQueries", "GetAllDocumentNodesValues", "GetAllAttributes", "GetValueOfAttribute", "GetNodesWithAttribute", "GetStructuredNodes", "AddNewNode", "DeleteNode", "AddAttributeToNode", "RemoveAttributeFromNode", "EditNodeText", "EditNodeName" };
            List<SelectListItem> functionsList = new List<SelectListItem>();
            foreach (var row in functionNames) functionsList.Add(new SelectListItem { Text = row, Value = row });
            ViewBag.FunctionsList = functionsList;


            if (model.SelectedFunction != null)
            {
                string? result = String.Empty;
                switch (model.SelectedFunction)
                {
                    case "CheckNodeIfExists":
                        result = _xService.CheckNodeIfExists(model.XMLDocumentId, model.XQuery!).ToString();
                        break;
                    case "GetNodeText":
                        result = _xService.GetNodeText(model.XMLDocumentId, model.XQuery!);
                        break;
                    case "GetNodes":
                        result = _xService.GetNodes(model.XMLDocumentId, model.XQuery!);
                        break;
                    case "GetAllDocumentNodesQueries":
                        var xmlString = _xService.GetAllDocumentNodesQueries(model.XMLDocumentId, model.XQuery!);
                        if (xmlString != null) foreach (var row in xmlString) result += $"-> {row}\n";
                        break;
                    case "GetAllDocumentNodesValues":
                        xmlString = _xService.GetAllDocumentNodesValues(model.XMLDocumentId, model.XQuery!);
                        if (xmlString != null) foreach (var row in xmlString) result += $"-> {row}\n";
                        break;
                    case "GetAllAttributes":
                        try
                        {
                            var dict = _xService.GetAllAttributes(model.XMLDocumentId, model.XQuery!);
                            if (dict != null) foreach (var row in dict) result += $"-> {row.Key} = {row.Value}\n";
                        }
                        catch (Exception ex) { result = ex.Message; }
                        break;
                    case "GetNodesWithAttribute":
                        xmlString = _xService.GetNodesWithAttribute(model.XMLDocumentId, model.XQuery!, model.AttributeName!, model.AttributeValue);
                        if (xmlString != null) foreach (var row in xmlString) result += $"-> {row}\n";
                        break;
                    case "GetValueOfAttribute":
                        try
                        {
                            result = _xService.GetValueOfAttribute(model.XMLDocumentId, model.XQuery!, model.AttributeName!);
                        }
                        catch (Exception ex) { result = ex.Message; }
                        break;
                    case "GetStructuredNodes":
                        try
                        {
                            var values = model.AttributeName!.Split(',');
                            var dictList = _xService.GetStructuredNodes(model.XMLDocumentId, model.XQuery!, values);
                            if (dictList != null)
                                foreach (var row in dictList)
                                {
                                    foreach (var d in values)
                                    {
                                        result += " -> " + d + " - " + row[d] + '\n';
                                    }
                                    result += '\n';
                                }
                        }
                        catch (Exception ex) { result = ex.Message; }
                        break;

                    // ---

                    case "AddNewNode":
                        try
                        {
                            result = _xService.AddNewNode(model.XMLDocumentId, model.XQuery!, model.AttributeName!) ? "Added new node" : "Some error occured while adding";
                        }
                        catch (Exception ex) { result = ex.Message; }
                        break;
                    case "AddAttributeToNode":
                        result = _xService.AddAttributeToNode(model.XMLDocumentId, model.XQuery!, model.AttributeName!, model.AttributeValue!) ? "Attribut added successfully" : "Some error occured while adding attribute";
                        break;
                    case "RemoveAttributeFromNode":
                        result = _xService.RemoveAttributeFromNode(model.XMLDocumentId, model.XQuery!, model.AttributeName!) ? "Attribute removed successfully" : "Some error occured while removing attribute";
                        break;
                    case "DeleteNode":
                        result = _xService.DeleteNode(model.XMLDocumentId, model.XQuery!) ? "Deleted successfully" : "Some error occured while deleting";
                        break;
                    case "EditNodeText":
                        result = _xService.EditNodeText(model.XMLDocumentId, model.XQuery!, model.AttributeName!) ? "Node text edited successfully" : "Some error occured while editing node text";
                        break;
                    case "EditNodeName":
                        try
                        {
                            result = _xService.EditNodeName(model.XMLDocumentId, model.XQuery!, model.AttributeName!) ? "Node text edited successfully" : "Some error occured while editing node name";
                        }
                        catch (Exception) { result = "Some errod occured while editing node name"; }
                        break;
                }

                model.Results = result;
            }

            var doc = _xService.GetDocumentById(model.XMLDocumentId);
            // formatting XML document
            XmlDocument tmp = new XmlDocument();
            StringWriter sw = new StringWriter();
            tmp.LoadXml(doc.XMLDocument!);
            tmp.Save(sw);
            model.XMLDocumentContent = sw.ToString();

            return View(model);
        }
    }
}
