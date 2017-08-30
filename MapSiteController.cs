using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Text;
using System.IO;
using Gear.Areas.Admin.Models;
using HtmlAgilityPack;
using System.Web.Security;
using Ionic.Zip;
using System.Threading;
using System.Xml;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Collections.Specialized;
using DataAccessLayer;
using System.Data;
using System.Threading.Tasks;
using System.Net;


namespace SiteMapBuilder.Controllers
{
    public class MapSiteController : Controller
    {
        int totalfiles = 0;

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View("Login");
            }
            return View();
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return View("Index");
        }

        public ActionResult Login(FormCollection frm)
        {
            if (System.Configuration.ConfigurationManager.AppSettings["LogInPassword"] == frm["pass"])
            {
                FormsAuthentication.SetAuthCookie("Admin", true);
                return View();
            }
            return RedirectToAction("Index");
        }

      


        //NewXml Tehila
        public void NewXml()
        {

             StringBuilder mapxml = new StringBuilder();

            string url = System.Configuration.ConfigurationManager.AppSettings["LocalUrl"];
            
           mapxml.AppendLine("<xml>");
           DataSet ds = RunStoredProcBase();

                    if (ds != null && ds.Tables.Count > 0)
                    {
                        DataTable dt = ds.Tables[0];
                        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                if (dr != null)
                                {
                                    string PruducerName = dr[0].ToString();
                                    string CategoryGroupName = dr[1].ToString();
                                    string CategoryName = dr[2].ToString();
                                    string Name = dr[3].ToString();
                                    string BodyTypeName = dr[4].ToString();
                                    string ModelName = dr[5].ToString();
                                    string GearModelId = dr[6].ToString();
                                    string GearCarId = dr[7].ToString();
                                    string CarName = dr[8].ToString();
                                  
                                    string price = RunStoredProcPrice(GearCarId);
                                    
                                   
                                    
                                    
                                    mapxml.AppendLine("<car mid=\"100\">");
                                    mapxml.AppendLine("<Manufacturer>" + PruducerName + "</Manufacturer>");
                                    mapxml.AppendLine("<ZCarModel>" +ModelName + " " + CarName+  "</ZCarModel>");
                                    string ss1 = "",ss2="";
                                    mapxml.AppendLine("<TestDrive value= \"" + ss1 + "\"" + ">" + "</TestDrive>");
                                    mapxml.AppendLine("<Leasing_Private value= \"" + ss2 + "\"" + ">" + "</Leasing_Private>");
                                   // mapxml.AppendLine("<Leasing_Private value=\"http://www.gear.co.il/seviceForm?utm_source=zap&amp;utm_medium=consulting&amp;utm_campaign=catalog&amp;formWidth=800&amp;car=786\">" + "</Leasing_Private>");
                                    mapxml.AppendLine("<Article>" + "</Article>");
                                    mapxml.AppendLine("<images>");
                                    DataSet dds = RunStoredProcImages(GearModelId);
                                    if (dds != null && dds.Tables.Count > 0)
                                    {
                                        DataTable ddt = dds.Tables[0];
                                        if (ddt != null && ddt.Rows != null && ddt.Rows.Count > 0)
                                        {
                                            foreach (DataRow ddr in ddt.Rows)
                                            {
                                                if (ddr != null)
                                                {
                                                    string ss = ddr[5].ToString(); 
                                                    mapxml.AppendLine("<Image url= \"" + ss + "\"" + ">" + "</Image>");
                                                }
                                            }
                                        }
                                    }
                                     
                                    mapxml.AppendLine("</images>");
                                    mapxml.AppendLine("<Price>" + price + "</Price>");
                                    mapxml.AppendLine("<Description>" + PruducerName + " " + ModelName + " " + CarName + "</Description>");
                                    mapxml.AppendLine("<params>");
                                   // mapxml.AppendLine("<param name= \""+Name+"\""  + " " + "value=\"\"" + " " + "unit=\"\">" + "</param>");
                                    dds = RunStoredProcBaseProperty(GearCarId);
                                    //int a = 5003;
                                   // dds = RunStoredProcBaseProperty(a.ToString());
                                    if (dds != null && dds.Tables.Count > 0)
                                    {
                                        DataTable ddt = dds.Tables[0];
                                        if (ddt != null && ddt.Rows != null && ddt.Rows.Count > 0)
                                        {
                                            foreach (DataRow ddr in ddt.Rows)
                                            {
                                                if (ddr != null)
                                                {
                                                    string ImagePropertyName = "";
                                                    string ImagePropertyValue = "";
                                                    try
                                                    {
                                                        ImagePropertyName =  ddr[2].ToString();
                                                        ImagePropertyValue = ddr[3].ToString();
                                                    }
                                                    catch { }
                                                    char err = '"';
                                                    string good = "'";
                                                    if(ImagePropertyName.Contains(err))
                                                    {

                                                        // ImagePropertyName = ImagePropertyName.Substring(0, ImagePropertyName.IndexOf(err)) + "'" + "'" + ImagePropertyName.Substring(ImagePropertyName.IndexOf(err) + 1);
                                                     ImagePropertyName=  ImagePropertyName.Replace(err.ToString(), good+good);
                                                    }

                                                    mapxml.AppendLine("<param name= \"" + ImagePropertyName + "\"" + " " + "value=\"" + ImagePropertyValue + "\"" + " " + "unit=\"\">" + "</param>");
                                                   // mapxml.AppendLine("<param name= \"" + ImagePropertyName + "\"" + " " + "value=\"" + ImagePropertyValue + "unit=\"\">" + "</param>");
                                                }
                                            }
                                        }
                                    }
                                    mapxml.AppendLine("</params>");
                                    mapxml.AppendLine("</car>");

                                  }

                                
                                
                               
                            }
                            
                        }
                        

               
               
           }
           mapxml.AppendLine("</xml>");

                System.IO.File.WriteAllText(System.Configuration.ConfigurationManager.AppSettings["LocalPatch"] + "mapxml.xml", mapxml.ToString());
               // return File(System.Configuration.ConfigurationManager.AppSettings["LocalPatch"] + "mapxml.xml", mapxml.ToString());
        }
        public DataSet RunStoredProcBase()
        {
            DataSet dataSet = new DataSet();
            string connStr = "Data Source=PC-500772\\SQLEXPRESS;initial catalog=gear_db;user id=sa;password=1212;multipleactiveresultsets=True;App=EntityFramework&quot";
            SqlConnection connection;
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            connection = null;
            connection = new SqlConnection(connStr);
            connection.Open();
            command.Connection = connection;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "dbo.[Base]";
            adapter = new SqlDataAdapter(command);
            try
            {
                adapter.Fill(dataSet);
            }
            catch (Exception ex)
            {

            }

            connection.Close();

            return dataSet;
        }
        public string RunStoredProcPrice(string GearCarId)
        {
            string year = DateTime.Now.Year.ToString();
            DataSet dataSet = new DataSet();
            string connStr = "Data Source=PC-500772\\SQLEXPRESS;initial catalog=gear_db;user id=sa;password=1212;multipleactiveresultsets=True;App=EntityFramework&quot";
            SqlConnection connection;
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            command.Parameters.Add(new SqlParameter("@ID", GearCarId));
            command.Parameters.Add(new SqlParameter("@Year", year));
            connection = null;
            connection = new SqlConnection(connStr);
            connection.Open();
            command.Connection = connection;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "dbo.[price]";
            adapter = new SqlDataAdapter(command);
            try
            {
                adapter.Fill(dataSet);
            }
            catch (Exception ex)
            {

            }

            connection.Close();

            string s = "";
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                DataTable dt = dataSet.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr != null)
                        {
                             s= dr[0].ToString();
                            return s;
                        }
                    }
                }
            }
            return s;
          }

        public DataSet RunStoredProcImages(string GearModelId)
        {
            
            DataSet dataSet = new DataSet();
            string connStr = "Data Source=PC-500772\\SQLEXPRESS;initial catalog=gear_db;user id=sa;password=1212;multipleactiveresultsets=True;App=EntityFramework&quot";
            SqlConnection connection;
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            command.Parameters.Add(new SqlParameter("@ID", GearModelId));
            connection = null;
            connection = new SqlConnection(connStr);
            connection.Open();
            command.Connection = connection;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "dbo.[Images]";
            adapter = new SqlDataAdapter(command);
            try
            {
                adapter.Fill(dataSet);
            }
            catch (Exception ex)
            {

            }

            connection.Close();
            return dataSet;


        }
        public DataSet RunStoredProcBaseProperty(string GearCarId)
        {

            DataSet dataSet = new DataSet();
            string connStr = "Data Source=PC-500772\\SQLEXPRESS;initial catalog=gear_db;user id=sa;password=1212;multipleactiveresultsets=True;App=EntityFramework&quot";
            SqlConnection connection;
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            command.Parameters.Add(new SqlParameter("@ID", GearCarId));
            connection = null;
            connection = new SqlConnection(connStr);
            connection.Open();
            command.Connection = connection;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "dbo.[ParamsQuery]";
            adapter = new SqlDataAdapter(command);
            try
            {
                adapter.Fill(dataSet);
            }
            catch (Exception ex)
            {

            }

            connection.Close();
            return dataSet;


        }
        public ActionResult SiteMap()
        {
            if (User.Identity.IsAuthenticated)
            {

               CreateMainSiteMap();

               CreatePriceListSiteMap();

                CreateCompareSiteMap();

                StringBuilder mapxml = new StringBuilder();

                string url = System.Configuration.ConfigurationManager.AppSettings["LocalUrl"];

                mapxml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                mapxml.AppendLine("<sitemapindex xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
                //main site map
                mapxml.AppendLine("<sitemap>");
                mapxml.AppendLine("<loc>" + url + "/sitemaps/sitemapmain.xml" + "</loc>");
                mapxml.AppendLine("<lastmod>" + DateTime.Now.ToString("yyyy-MM-dd") + "</lastmod>");
                mapxml.AppendLine("</sitemap>");
                //pricelist site map
                mapxml.AppendLine("<sitemap>");
                mapxml.AppendLine("<loc>" + url + "/sitemaps/sitemappricelist.xml" + "</loc>");
                mapxml.AppendLine("<lastmod>" + DateTime.Now.ToString("yyyy-MM-dd") + "</lastmod>");
                mapxml.AppendLine("</sitemap>");
                //compare site map
                //for (int i = 1; i <= totalfiles; i++)
               // {
                    mapxml.AppendLine("<sitemap>");
                    mapxml.AppendLine("<loc>" + url + "/sitemaps/sitemapcompare.xml" + "</loc>");
                    mapxml.AppendLine("<lastmod>" + DateTime.Now.ToString("yyyy-MM-dd") + "</lastmod>");
                    mapxml.AppendLine("</sitemap>");
              //  }


                mapxml.AppendLine("</sitemapindex>");
                System.IO.File.WriteAllText(System.Configuration.ConfigurationManager.AppSettings["LocalPatch"] + "mapxml.xml", mapxml.ToString());

                var zipfilefordelete = System.Configuration.ConfigurationManager.AppSettings["LocalPatch"] + "GearSiteMaps.zip";

                if ((System.IO.File.Exists(zipfilefordelete)))
                    System.IO.File.Delete(zipfilefordelete);


                using (ZipFile zip = new ZipFile())
                {
                    String[] filenames = System.IO.Directory.GetFiles(System.Configuration.ConfigurationManager.AppSettings["LocalPatch"]);

                    foreach (String filename in filenames)
                    {
                        ZipEntry e = zip.AddFile(filename);
                    }

                    zip.Save(System.Configuration.ConfigurationManager.AppSettings["LocalPatch"] + "GearSiteMaps.zip");
                }

                return File(System.Configuration.ConfigurationManager.AppSettings["LocalPatch"] + "GearSiteMaps.zip", "application/xml", "GearSiteMaps.zip");
            }

            return RedirectToAction("Index");
        }

        private void CreateMainSiteMap()
        {
            var dc = new gear_dbEntities();

            Directory.CreateDirectory(Server.MapPath("~/sitemaps/"));

            StringBuilder mapxml = new StringBuilder();

            HtmlDocument doc = new HtmlDocument();

            UrlHelper url = new UrlHelper(Request.RequestContext);

            mapxml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            mapxml.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd\" xmlns:image=\"http://www.google.com/schemas/sitemap-image/1.1\" xmlns:video=\"http://www.google.com/schemas/sitemap-video/1.1\">");
            //mapxml.AppendLine("<lastmod>" + DateTime.Now.ToString("yyyy-MM-dd") + "</lastmod>");
            //mapxml.AppendLine("<xsi:element name=\"tLastmod\" type=\"xsi:" + DateTime.Now.ToString("yyyy-MM-dd") + "\" />");
            mapxml.AppendLine("<url><loc>http://www.gear.co.il/</loc><changefreq>daily</changefreq><priority>1.0</priority></url>");
            mapxml.AppendLine("<url><loc>http://www.gear.co.il/קטלוג_רכב</loc><changefreq>monthly</changefreq><priority>0.6</priority></url>");
            mapxml.AppendLine("<url><loc>http://www.gear.co.il/השוואת_רכבים</loc><changefreq>daily</changefreq><priority>0.9</priority></url>");
            mapxml.AppendLine("<url><loc>http://www.gear.co.il/חיפוש_רכב_מתקדם</loc><changefreq>monthly</changefreq><priority>0.6</priority></url>");
            mapxml.AppendLine("<url><loc>http://www.gear.co.il/כתבות_רכב</loc><changefreq>daily</changefreq><priority>0.9</priority></url>");
            mapxml.AppendLine("<url><loc>http://www.gear.co.il/כתבות_וידאו</loc><changefreq>daily</changefreq><priority>0.9</priority></url>");
            mapxml.AppendLine("<url><loc>http://www.gear.co.il/דגמים_חדשים</loc><changefreq>daily</changefreq><priority>0.9</priority></url>");
            mapxml.AppendLine("<url><loc>http://www.gear.co.il/דגמים_נבחרים</loc><changefreq>daily</changefreq><priority>0.9</priority></url>");
            mapxml.AppendLine("<url><loc>http://www.gear.co.il/גלריה</loc><changefreq>daily</changefreq><priority>0.9</priority></url>");
            mapxml.AppendLine("<url><loc>http://www.gear.co.il/אודותינו</loc><changefreq>monthly</changefreq><priority>0.6</priority></url>");
            mapxml.AppendLine("<url><loc>http://www.gear.co.il/למה_כדאי</loc><changefreq>monthly</changefreq><priority>0.6</priority></url>");
            mapxml.AppendLine("<url><loc>http://www.gear.co.il/תנאי_השימוש</loc><changefreq>monthly</changefreq><priority>0.6</priority></url>");
            mapxml.AppendLine("<url><loc>http://www.gear.co.il/שאלות_נפוצות</loc><changefreq>monthly</changefreq><priority>0.6</priority></url>");
            mapxml.AppendLine("<url><loc>http://www.gear.co.il/כדאי_לפרסם_אצלינו</loc><changefreq>monthly</changefreq><priority>0.6</priority></url>");
            mapxml.AppendLine("<url><loc>http://www.gear.co.il/מחירון/ראשי</loc><changefreq>monthly</changefreq><priority>0.6</priority></url>");

            //makers
            foreach (var maker in dc.Makers.Include("MediaItem").Where(m => m.IsActive))
            {
                string URL = Gear.Models.Global.ActionDecoded(url, "Makers", "Catalog", new { id = maker.ID });
                if (!URL.ToLower().Contains("makers"))
                {
                    mapxml.AppendLine("<url><loc>http://www.gear.co.il" + URL + "</loc>"
                        + "<changefreq>daily</changefreq>"
                        + "<priority>0.9</priority>" +
                        (SystemParameter.GetValue("ImageInSiteMap") ? "<image:image>"
                        + "<image:loc>http://gear.co.il" + Gear.Models.MediaItem.LinkURL(maker.MediaItem) + "</image:loc>"
                        + "<image:caption>" + Path.GetFileNameWithoutExtension(maker.MediaItem.FileName) + "</image:caption>"
                        + "</image:image>" : "")
                        + "</url>");
                }
            }

            //Models Groups
            foreach (var modelgroup in dc.ModelsGroups.Where(m => !m.IsDeleted).OrderBy(m => m.OrderNum))
            {
                mapxml.AppendLine("<url><loc>http://www.gear.co.il" + Gear.Models.Global.ActionDecoded(url, "ModelGroup", "Catalog", new { modelgroup = modelgroup.Maker.HebName + "_" + modelgroup.Name.Replace(" ", "_") }) + "</loc>"
                            + "<changefreq>daily</changefreq>"
                            + "<priority>0.9</priority>"
                            + "</url>");
            }

            //models
            foreach (var model in dc.Models.Include("ModelImagesLookups").Where(m => !m.IsDeleted))
            {
                string URL = Gear.Models.Global.ActionDecoded(url, "Models", "Catalog", new { id = model.ID });
                if (!URL.ToLower().Contains("admin") && !URL.ToLower().Contains("models"))
                {
                    mapxml.AppendLine("<url><loc>http://www.gear.co.il" + URL + "</loc>" //Gear.Models.Global.ValidURL(model.HebName)
                         + "<lastmod>" + model.LastModified.ToString("yyyy-MM-dd") + "</lastmod>"
                         + "<changefreq>daily</changefreq>"
                         + "<priority>0.9</priority>");
                    if (SystemParameter.GetValue("ImageInSiteMap"))
                    {
                        foreach (var img in model.ModelImagesLookups)
                        {
                            mapxml.AppendLine("<image:image>"
                            + "<image:loc>http://www.gear.co.il" + Gear.Models.MediaItem.LinkURL(img.MediaItem) + "</image:loc>"
                            + "<image:caption>" + model.HebName + ' ' + model.ModelsGroup.Maker.HebName + "</image:caption>"
                            + "</image:image>");
                        }
                    }
                    mapxml.AppendLine("</url>");
                }
            }

            //cars
            foreach (var car in dc.Cars.Where(m => m.IsActive && !m.IsDeleted))
            {
                mapxml.AppendLine("<url><loc>http://www.gear.co.il" + Gear.Models.Global.ActionDecoded(url, "Cars", "Catalog", new { area = "", id = car.ID }) + "</loc>"
                    + "<lastmod>" + car.LastModified.ToString("yyyy-MM-dd") + "</lastmod>"
                    + "<changefreq>daily</changefreq>"
                    + "<priority>0.9</priority>"
                    + "</url>");
            }

            //CategoriesGroups
            foreach (var group in dc.CategoriesGroups.Where(m => m.IsActive))
            {
                mapxml.AppendLine("<url><loc>http://www.gear.co.il" + Gear.Models.Global.ActionDecoded(url, "CategoreisGroups", "Catalog", new { id = group.ID }) + "</loc>"
                    + "<changefreq>daily</changefreq>"
                    + "<priority>0.9</priority>"
                    + "</url>");
            }

            //Galleries
            foreach (var galery in Gear.Models.Global.Galleries)
            {
                mapxml.AppendLine("<url><loc>http://www.gear.co.il" + Gear.Models.Global.ActionDecoded(url, "Galleries", "Gallery", new { id = galery.ID }) + "</loc>"
                     + "<lastmod>" + galery.CreateDate.ToString("yyyy-MM-dd") + "</lastmod>"
                     + "<changefreq>monthly</changefreq>"
                     + "<priority>0.9</priority>");
                if (SystemParameter.GetValue("ImageInSiteMap"))
                {
                    mapxml.AppendLine("<image:image>"
                    + "<image:loc>http://www.gear.co.il" + Gear.Models.MediaItem.LinkURL(galery.MediaItem) + "</image:loc>"
                    + "<image:caption>" + galery.Name + "</image:caption>"
                    + "</image:image>");

                    foreach (var img in galery.GalleriesItems)
                    {
                        mapxml.AppendLine("<image:image>"
                        + "<image:loc>http://www.gear.co.il" + Gear.Models.MediaItem.LinkURL(img.MediaItem) + "</image:loc>"
                        + "<image:caption>" + img.HebName + "</image:caption>"
                        + "<image:title>" + img.EngName + "</image:title>"
                        + "</image:image>");
                    }
                }
                mapxml.AppendLine("</url>");
            }

            //ArticlesCategories
            foreach (var category in dc.ArticlesCategories.Where(a => a.IsActive))
            {
                if (category.IsSeo)
                {
                    mapxml.AppendLine("<url><loc>http://www.gear.co.il/" + Gear.Models.Global.ValidURL(category.SeoURL) + "</loc>"
                        + "<changefreq>daily</changefreq>"
                        + "<priority>0.9</priority>"
                        + "</url>");
                }
                else
                {
                    string URL = Gear.Models.Global.ActionDecoded(url, "Category", "Articles", new { id = category.ID, admin = 1 });
                    if (!URL.ToLower().Contains("articles/category"))
                    {
                        mapxml.AppendLine("<url><loc>http://www.gear.co.il" + URL + "</loc>"
                           + "<changefreq>daily</changefreq>"
                           + "<priority>0.9</priority>"
                           + "</url>");
                    }
                }
            }

            //Articles
            foreach (var article in dc.Articles.Where(m => m.IsActive && (!m.IsDeleted.HasValue || !m.IsDeleted.Value)))
            {
                doc.LoadHtml(article.ArticleContent);
                var nodes = doc.DocumentNode.SelectNodes("//img");
                mapxml.AppendLine("<url><loc>http://www.gear.co.il/" + Gear.Models.Global.ActionDecoded(url, "Read", "Articles", new { id = article.ID, area = "", URL = article.URL }) + "</loc>"
                     + "<lastmod>" + article.CreationDate.ToString("yyyy-MM-dd") + "</lastmod>"
                     + "<changefreq>monthly</changefreq>"
                     + "<priority>0.9</priority>");
                if (nodes != null && SystemParameter.GetValue("ImageInSiteMap"))
                {
                    foreach (var img in nodes)
                    {
                        if (img.Attributes["src"].Value.Contains("/Articles/"))
                        {
                            mapxml.AppendLine("<image:image>"
                            + "<image:loc>http://www.gear.co.il/" + img.Attributes["src"].Value.Substring(9) + "</image:loc>"
                            + "<image:caption>" + img.Attributes["alt"].Value + "</image:caption>"
                            + "</image:image>");
                        }
                    }
                }
                mapxml.AppendLine("</url>");
            }

            if (!string.IsNullOrWhiteSpace(mapxml.ToString()))
            {
                mapxml.AppendLine("</urlset>");
                System.IO.File.WriteAllText(System.Configuration.ConfigurationManager.AppSettings["LocalPatch"] + "sitemapmain.xml", mapxml.ToString());
            }
        }

        private void CreatePriceListSiteMap()
        {

            Directory.CreateDirectory(Server.MapPath("~/sitemaps/"));
            var dc = new gear_dbEntities();

            StringBuilder mapxml = new StringBuilder();

            UrlHelper url = new UrlHelper(Request.RequestContext);

            mapxml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            mapxml.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd\" xmlns:image=\"http://www.google.com/schemas/sitemap-image/1.1\" xmlns:video=\"http://www.google.com/schemas/sitemap-video/1.1\">");
            //mapxml.AppendLine("<xsi:element name=\"time\" type=\"xsi:" + DateTime.Now.ToString("yyyy-MM-dd") + "\" />");
            //mapxml.AppendLine("<lastmod>" + DateTime.Now.ToString("yyyy-MM-dd") + "</lastmod>");
            //PriceList Models
            List<string[]> allmodels = new List<string[]>();
            foreach (var model in dc.Models.Include("ModelImagesLookups").Where(m => !m.IsDeleted))
            {
                var models = Gear.Models.Global.Models().Where(m => m.ID == model.ID).ToList().Where(m => m.MinPrice != 0);
                var mm = models.Select(m => new { ID = m.ID, Name = m.HebName, Years = m.CheckCarYear(m.ID, m.MinYear.Value, m.MaxYear, false) });
                var allcarpricelists = models.Select(c => c.Cars.Where(m => m.IsActive && !m.IsDeleted).Select(n => n.PriceLists.OrderByDescending(o => o.ID)).FirstOrDefault());

                string lastpricedatecar = model.LastModified.ToString("yyyy-MM-dd");

                //HEW CODE !!!!!!!!!!
                var tempModels = Gear.Models.Global.Models().Where(md => md.ModelsGroup.ID == model.ModelsGroup.ID && md.HebName.Equals(model.HebName)).ToList().Where(md => md.MinPrice != 0).ToList();
                var usedIdModels = tempModels.Where(md => md.Cars.Any(b => b.PriceLists.Any(d => d.PriceListsDetails.Any(e => !e.Price.HasValue)))).Select(md => md.ID).ToList();

                //get new price car in this year
                var priceListTemp =
                    tempModels.Where(md => md.Cars.Any(c => c.CarPropertiesValues.Where(p => p.CarProperty.EngName == "car_prop_is_marketed").Any(v => v.Value == 1) &&
                            c.PriceLists.Any(pr => pr.PriceListsDetails.Any(y => y.Year == DateTime.Now.Year && y.IsLast && y.Price.HasValue))));

                //get price car in this year but not new 
                var usedPriceListTemp = tempModels.Where(md => md.Cars.Any(c => c.CarPropertiesValues.Where(p => p.CarProperty.EngName == "car_prop_is_marketed").Any(v => v.Value == 1) &&
                            c.PriceLists.Any(pr => pr.PriceListsDetails.Any(y => y.Year == DateTime.Now.Year && !y.IsLast && y.Price.HasValue))));

                //add new car
                foreach (var tmpmodel in priceListTemp.Select(md => md.ID).ToList())
                {
                    if (usedIdModels.Contains(tmpmodel))
                    { usedIdModels.Remove(tmpmodel); }
                }
                foreach (var tmpmodel in usedPriceListTemp.Select(md => md.ID).ToList())
                {
                    if (usedIdModels.Contains(tmpmodel))
                    { usedIdModels.Remove(tmpmodel); }
                }

                //remove model they have no price
                mm = mm.ToList().Where(n => !usedIdModels.Contains(n.ID));

                //END NEW CODE !!!!!!!!!!!!

                foreach (var m in mm)
                {
                    var years = m.Years.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (years.Count() > 0)
                    {
                        for (int i = 0; i < years.Length; ++i)
                        {
                            string URL = string.Empty;
                            //var usedpriceList = allcarpricelists.Where(c => c.Any(pr => pr.PriceListsDetails.Any(y => y.Year == DateTime.Now.Year && !y.IsLast && y.Price.HasValue)));

                            if (usedPriceListTemp.Any() && years[i] == DateTime.Now.Year.ToString())//have used price in year
                            {
                                URL = Gear.Models.Global.ActionDecoded(url, "ModelPriceList", "Pricelist", new { id = model.ID, year = years[i] });
                            }
                            if (years[i] != DateTime.Now.Year.ToString())
                            {
                                URL = Gear.Models.Global.ActionDecoded(url, "ModelPriceList", "Pricelist", new { id = model.ID, year = years[i] });
                            }

                            if (!URL.ToLower().Contains("admin") && !URL.ToLower().Contains("modelpricelist"))
                            {
                                string modelurl = ("<url><loc>http://www.gear.co.il" + URL + "</loc>"
                                        + "<lastmod>" + lastpricedatecar + "</lastmod>"
                                        + "<changefreq>daily</changefreq>"
                                        + "<priority>0.9</priority>"
                                        + "</url>");

                                if (!allmodels.Any(it => it[0].Equals(URL)))
                                {
                                    allmodels.Add(new string[2] { URL, modelurl });
                                }
                            }

                            if (Convert.ToInt32(years[i]) == DateTime.Now.Year && allcarpricelists != null && allcarpricelists.Count() > 0 && allcarpricelists.First().Count() > 0)
                            {
                                lastpricedatecar = allcarpricelists.Select(p => p.Max(a => a.UpdateDate)).FirstOrDefault().ToString("yyyy-MM-dd");
                                var newprice = Gear.Models.Global.AllModelsCarsPerYear((int)model.ModelsGroup.MakerID, model.ID.ToString(), DateTime.Now.Year).Where(y => y.Year == DateTime.Now.Year && y.IsLast).OrderByDescending(o => o.PriceListID).FirstOrDefault();
                                if (newprice != null)
                                {
                                    string newURL = Gear.Models.Global.ActionDecoded(url, "NewModelPriceList", "Pricelist", new { id = model.ID, year = newprice.Year });
                                    if (!newURL.ToLower().Contains("admin") && !newURL.ToLower().Contains("modelpricelist"))
                                    {
                                        string modelurl = ("<url><loc>http://www.gear.co.il" + newURL + "</loc>"
                                                + "<lastmod>" + lastpricedatecar + "</lastmod>"
                                                + "<changefreq>daily</changefreq>"
                                                + "<priority>0.9</priority>"
                                                + "</url>");
                                        if (!allmodels.Any(it => it[0].Equals(newURL)))
                                        {
                                            allmodels.Add(new string[2] { newURL, modelurl });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (var item in allmodels)
            {
                mapxml.AppendLine(item[1]);
            }

            //PriceList Cars
            List<string[]> allcars = new List<string[]>();
            foreach (var car in dc.Cars.Where(m => m.IsActive && !m.IsDeleted))
            {
                var mm = Gear.Models.Global.Models().Where(m => m.ID == car.Model.ID).ToList().Where(m => m.MinPrice != 0).Select(m => new { ID = m.ID,Name = m.HebName,Years = m.CheckCarYear(m.ID, m.MinYear.Value, m.MaxYear, false)});

                //new car only
                var newpricelist = car.PriceLists.OrderByDescending(o => o.ID).FirstOrDefault();

                foreach (var m in mm)
                {
                     var years = m.Years.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (years.Count() > 0)
                    {
                        for (int i = 0; i < years.Length; ++i)
                        {
                            var pricelist = car.PriceLists.OrderByDescending(o => o.ID).FirstOrDefault();

                            if (pricelist != null)
                            {
                                var pricelistdetails =
                                    pricelist.PriceListsDetails.LastOrDefault(pd => pd.Year == Convert.ToInt32(years[i]));

                                if (pricelistdetails != null)
                                {
                                    string URL = string.Empty;

                                    if (pricelistdetails.Year == DateTime.Now.Year && !pricelistdetails.IsLast && pricelistdetails.Price.HasValue)
                                    {
                                        URL = Gear.Models.Global.ActionDecoded(url, "CarPriceList", "Pricelist",
                                                                                      new { id = car.ID, year = years[i] });

                                    }
                                    if (pricelistdetails.Year != DateTime.Now.Year && pricelistdetails.Price.HasValue)
                                    {
                                        URL = Gear.Models.Global.ActionDecoded(url, "CarPriceList", "Pricelist",
                                                                                      new { id = car.ID, year = years[i] });
                                    }

                                    string carlurl = ("<url><loc>http://www.gear.co.il" + URL + "</loc>"
                                                          + "<lastmod>" +
                                                          pricelistdetails.PriceList.UpdateDate.ToString("yyyy-MM-dd") +
                                                          "</lastmod>"
                                                          + "<changefreq>daily</changefreq>"
                                                          + "<priority>0.9</priority>"
                                                          + "</url>");

                                    if (!allcars.Any(c => c[0].Equals(URL)))
                                    {
                                        allcars.Add(new string[2] { URL, carlurl });
                                    }
                                }
                            }

                            if (Convert.ToInt32(years[i]) == DateTime.Now.Year && newpricelist != null)
                            {
                                var newcar = newpricelist.PriceListsDetails.Any(p => p.IsLast && p.Year == DateTime.Now.Year);
                                if (newcar)
                                {
                                    string URL = Gear.Models.Global.ActionDecoded(url, "NewCarPriceList", "Pricelist",
                                                                                              new { id = car.ID, year = DateTime.Now.Year });
                                    string carlurl = ("<url><loc>http://www.gear.co.il" + URL + "</loc>"
                                                      + "<lastmod>" +
                                                      newpricelist.UpdateDate.ToString("yyyy-MM-dd") +
                                                      "</lastmod>"
                                                      + "<changefreq>daily</changefreq>"
                                                      + "<priority>0.9</priority>"
                                                      + "</url>");
                                    if (!allcars.Any(it => it[0].Equals(URL)))
                                    {
                                        allcars.Add(new string[2] { URL, carlurl });
                                    }
                                }
                            }
                        }
                    }
                }
            }


            foreach (var item in allcars)
            {
                mapxml.AppendLine(item[1]);
            }

            if (!string.IsNullOrWhiteSpace(mapxml.ToString()))
            {
                mapxml.AppendLine("</urlset>");
                System.IO.File.WriteAllText(System.Configuration.ConfigurationManager.AppSettings["LocalPatch"] + "sitemappricelist.xml", mapxml.ToString());
            }
        }

        private void CreateCompareSiteMap()
        {
            Directory.CreateDirectory(Server.MapPath("~/sitemaps/"));
            var dc = new gear_dbEntities();

            int urlcount = 0, filescount = 0;

            UrlHelper url = new UrlHelper(Request.RequestContext);

            StringBuilder mapxml = new StringBuilder();

            List<string> allcars = new List<string>();

            mapxml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            mapxml.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd\" xmlns:image=\"http://www.google.com/schemas/sitemap-image/1.1\" xmlns:video=\"http://www.google.com/schemas/sitemap-video/1.1\">");
            //mapxml.AppendLine("<xsi:element name=\"time\" type=\"xsi:" + DateTime.Now.ToString("yyyy-MM-dd") + "\" />");
            //mapxml.AppendLine("<lastmod>" + DateTime.Now.ToString("yyyy-MM-dd") + "</lastmod>");

            foreach (var categiry in dc.Categories.Where(c => c.IsActive))
            {
                //get models for this category
                var models = dc.Models.Where(m => !m.IsDeleted && m.CategoryID == categiry.ID);
                List<int> modelsId = models.Select(m => m.ID).ToList();
                foreach (var model in models.ToList())
                {
                    models = models.ToList().Where(m => (m.MaxYear >= model.MinYear && m.MaxYear <= model.MaxYear) || (m.MinYear >= model.MinYear && m.MinYear <= model.MaxYear)).AsQueryable();
                    modelsId.Remove(model.ID);
                    //get firs car from each model
                    var cars = models.Select(m => m.Cars.Where(c => c.IsActive && !c.IsDeleted).OrderByDescending(o => o.ID).ThenBy(o => o.OrderNum).FirstOrDefault());
                    //array for all cars id
                    List<int> carsID = cars.Where(c => c != null).Select(c => c.ID).ToList();

                    foreach (int carID in carsID.ToList())
                    {
                        //get first car
                        Car firstcar = dc.Cars.SingleOrDefault(c => c.ID == carID);

                        //remove car from list
                        carsID.Remove(carID);

                        foreach (int newcarID in carsID)
                        {
                            //get first car
                            Car secondcar = dc.Cars.SingleOrDefault(c => c.ID == newcarID);

                            string firstcarurl = firstcar.Model.ModelsGroup.Maker.HebName.Replace(" ", "_").Replace("&", "&amp;") + "_" + firstcar.Model.HebName.Replace(" ", "_").Replace("&", "&amp;") + "_" + firstcar.Model.MinYear + "_" + firstcar.HebName.Replace(" ", "_").Replace("&", "&amp;");
                            string secondcarurl = secondcar.Model.ModelsGroup.Maker.HebName.Replace(" ", "_").Replace("&", "&amp;") + "_" + secondcar.Model.HebName.Replace(" ", "_").Replace("&", "&amp;") + "_" + secondcar.Model.MinYear + "_" + secondcar.HebName.Replace(" ", "_").Replace("&", "&amp;");

                            string URL = "http://www.gear.co.il/השוואת_רכבים/" + firstcarurl + "_מול_" + secondcarurl + "";
                            string carlurl = ("<url><loc>" + URL.Replace(" ","_").Replace("+","_") +"</loc>"
                                + "<lastmod>" + DateTime.Now.ToString("yyyy-MM-dd") + "</lastmod>"
                                + "<changefreq>daily</changefreq>"
                                + "<priority>0.9</priority>"
                                + "</url>");

                            //mapxml.AppendLine("<url><loc>http://www.gear.co.il/השוואת_רכבים/" + firstcarurl + "_מול_" + secondcarurl + "</loc>"
                            //    + "<lastmod>" + DateTime.Now.ToString("yyyy-MM-dd") + "</lastmod>"
                            //    + "<changefreq>daily</changefreq>"
                            //    + "<priority>0.9</priority>"
                            //    + "</url>");

                            //urlcount++;

                            if (!allcars.Any(c => c.Contains(URL)))
                            {
                                urlcount++;
                                allcars.Add(carlurl);
                            }

                            if (urlcount > 49980)
                            {
                                urlcount = 0;
                                filescount++;

                                if (!string.IsNullOrWhiteSpace(mapxml.ToString()))
                                {
                                    mapxml.AppendLine("</urlset>");
                                    System.IO.File.WriteAllText(System.Configuration.ConfigurationManager.AppSettings["LocalPatch"] + "sitemapcompare" + filescount + ".xml", mapxml.ToString());

                                    //new file
                                    mapxml.Clear();
                                    mapxml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                                    mapxml.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd\" xmlns:image=\"http://www.google.com/schemas/sitemap-image/1.1\" xmlns:video=\"http://www.google.com/schemas/sitemap-video/1.1\">");
                                    mapxml.AppendLine("<xsi:element name=\"time\" type=\"xsi:" + DateTime.Now.ToString("yyyy-MM-dd") + "\" />");

                                }
                            }
                        }
                    }
                }
            }


            //new cars
            foreach (var categiry in dc.Categories.Where(c => c.IsActive))
            {
                //get models for this category
                var models = dc.Models.Where(m => !m.IsDeleted && m.CategoryID == categiry.ID);
                List<int> modelsId = models.Select(m => m.ID).ToList();
                foreach (var model in models.OrderByDescending(o=>o.ID).ToList())
                {
                    models = models.ToList().Where(m => (m.MaxYear >= model.MinYear && m.MaxYear <= model.MaxYear) || (m.MinYear >= model.MinYear && m.MinYear <= model.MaxYear)).AsQueryable();
                    modelsId.Remove(model.ID);
                    //get firs car from each model
                    var cars = models.Select(m => m.Cars.Where(c => c.IsActive && !c.IsDeleted).OrderByDescending(o => o.ID).ThenBy(o => o.OrderNum).FirstOrDefault());
                    //array for all cars id
                    List<int> carsID = cars.Where(c => c != null).Select(c => c.ID).ToList();

                    foreach (int carID in carsID.ToList())
                    {
                        //get first car
                        Car firstcar = dc.Cars.SingleOrDefault(c => c.ID == carID);

                        //remove car from list
                        carsID.Remove(carID);

                        foreach (int newcarID in carsID)
                        {
                            //get first car
                            Car secondcar = dc.Cars.SingleOrDefault(c => c.ID == newcarID);

                            string firstcarurl = firstcar.Model.ModelsGroup.Maker.HebName.Replace(" ", "_").Replace("&", "&amp;") + "_" + firstcar.Model.HebName.Replace(" ", "_").Replace("&", "&amp;") + "_" + firstcar.Model.MinYear + "_" + firstcar.HebName.Replace(" ", "_").Replace("&", "&amp;");
                            string secondcarurl = secondcar.Model.ModelsGroup.Maker.HebName.Replace(" ", "_").Replace("&", "&amp;") + "_" + secondcar.Model.HebName.Replace(" ", "_").Replace("&", "&amp;") + "_" + secondcar.Model.MinYear + "_" + secondcar.HebName.Replace(" ", "_").Replace("&", "&amp;");

                            string URL = "http://www.gear.co.il/השוואת_רכבים/" + firstcarurl + "_מול_" + secondcarurl + "";
                            string carlurl = ("<url><loc>" + URL.Replace(" ", "_").Replace("+", "_") + "</loc>"
                                + "<lastmod>" + DateTime.Now.ToString("yyyy-MM-dd") + "</lastmod>"
                                + "<changefreq>daily</changefreq>"
                                + "<priority>0.9</priority>"
                                + "</url>");

                            //mapxml.AppendLine("<url><loc>http://www.gear.co.il/השוואת_רכבים/" + firstcarurl + "_מול_" + secondcarurl + "</loc>"
                            //    + "<lastmod>" + DateTime.Now.ToString("yyyy-MM-dd") + "</lastmod>"
                            //    + "<changefreq>daily</changefreq>"
                            //    + "<priority>0.9</priority>"
                            //    + "</url>");


                            if (!allcars.Any(c => c.Contains(URL)))
                            {
                                urlcount++;
                                allcars.Add(carlurl);
                            }

                            if (urlcount > 49980)
                            {
                                urlcount = 0;
                                filescount++;

                                if (!string.IsNullOrWhiteSpace(mapxml.ToString()))
                                {
                                    mapxml.AppendLine("</urlset>");
                                    System.IO.File.WriteAllText(System.Configuration.ConfigurationManager.AppSettings["LocalPatch"] + "sitemapcompare" + filescount + ".xml", mapxml.ToString());

                                    //new file
                                    mapxml.Clear();
                                    mapxml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                                    mapxml.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd\" xmlns:image=\"http://www.google.com/schemas/sitemap-image/1.1\" xmlns:video=\"http://www.google.com/schemas/sitemap-video/1.1\">");
                                    mapxml.AppendLine("<xsi:element name=\"time\" type=\"xsi:" + DateTime.Now.ToString("yyyy-MM-dd") + "\" />");

                                }
                            }
                        }
                    }
                }
            }

            //

            foreach (var item in allcars)
            {
                mapxml.AppendLine(item);
            }

            if (urlcount != 0)
            {
                filescount++;
                if (!string.IsNullOrWhiteSpace(mapxml.ToString()))
                {
                    mapxml.AppendLine("</urlset>");
                    System.IO.File.WriteAllText(System.Configuration.ConfigurationManager.AppSettings["LocalPatch"] + "sitemapcompare.xml", mapxml.ToString());
                }
            }

            totalfiles = filescount;
        }
    }
}
