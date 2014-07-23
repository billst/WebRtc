using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;
using System.IO;

namespace WebRTCServer2.Controllers
{

    public class wavfilesController : ApiController
    {
       
        
       // GET api/wavfiles
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/wavfiles/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/wavfiles
        public async Task<HttpResponseMessage> Post()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string root = HttpContext.Current.Server.MapPath("~/Download_Data");
           
            
          
           
            
            try
            {
                var provider = new MultipartFileStreamProvider(root) ;
                
               // Request.Content.ReadAsHttpRequestMessageAsync
                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);


                // This illustrates how to get the file names.
                foreach (MultipartFileData file in provider.FileData)
                {
                    string fileName_Browser = file.Headers.ContentDisposition.FileName;
                   fileName_Browser = fileName_Browser.Trim('"');
                    
                    string[] parts = fileName_Browser.Split('_');

                    Trace.WriteLine("fileName for the Metadata File : " + root + "/" + parts[0] + "_" + parts[1] + "_" + parts[2] + ".txt");
                    using (StreamWriter sw = new StreamWriter(root + @"\" +parts[0]+"_"+parts[1]+"_" + parts[2] + ".txt", true))
                    {
                        await sw.WriteLineAsync(parts[3] + ";" + file.LocalFileName +";" +parts[4]+";"+ parts[5] );
                    }
                    Trace.WriteLine(file.Headers.ContentDisposition.FileName);
                    Trace.WriteLine("Server file path: " + file.LocalFileName);
                }
                return Request.CreateResponse(HttpStatusCode.OK);


            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }

        }

        // PUT api/wavfiles/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/wavfiles/5
        public void Delete(int id)
        {
        }
    }
}
