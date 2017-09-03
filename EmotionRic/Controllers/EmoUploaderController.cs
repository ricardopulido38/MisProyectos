using EmotionRic.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EmotionRic.Controllers
{



    public class EmoUploaderController : Controller
    {

        string serverFolderPath;
        string key;
        EmotionHelper emoHelper;
        EmotionRicContext db = new EmotionRicContext();

        public EmoUploaderController()
        {
            serverFolderPath = ConfigurationManager.AppSettings["UPLOAD_DIR"];
            key = ConfigurationManager.AppSettings["EMOTION_KEY"];
            emoHelper = new EmotionHelper(key);
        }

        // GET: EmoUpload
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(HttpPostedFileBase file)
        {
            if (file?.ContentLength > 0)
            {
                var pictureName = Guid.NewGuid().ToString();
                pictureName += Path.GetExtension(file.FileName);

                var route = Server.MapPath(serverFolderPath);
                route = route +"\\" + pictureName;

                file.SaveAs(route);

                var emoPicture = await emoHelper.DetectAndExtractFacesAsync(file.InputStream);

                emoPicture.Name = file.FileName;
                emoPicture.Path = $" {serverFolderPath} / {emoPicture.Name}";

                db.EmoPictures.Add(emoPicture);
                //await db.SaveChangesAsync();
                db.SaveChanges();

                return RedirectToAction("Details", "EmoPictures", new { id = emoPicture.Id});
            }
            return View();
        }
    }
}