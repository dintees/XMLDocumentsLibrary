using Microsoft.AspNetCore.Mvc;
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

        public ActionResult Create() {
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
                int createdId = _xService.CreateDocument(model.Title!, model.Description!, model.XMLDocument!);
            } catch(Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
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
                } catch(Exception ex)
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
    }
}
