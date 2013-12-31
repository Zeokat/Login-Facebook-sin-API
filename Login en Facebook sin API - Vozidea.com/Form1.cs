using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace Login_en_Facebook_sin_API___Vozidea.com
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Obtener el usuario y password de Facebook desde el texbox
            string user = Uri.EscapeDataString(textBox_user.Text);
            string pass = Uri.EscapeDataString(textBox_pass.Text);

            //Primera peticion GET para obtener las cookies tal y como si visitasemos la página
            //que nos muestra la opción para acceder a nuestra cuenta Facebook
            //las cookies las guardamos en un CookieContainer que usaremos en las siguientes peticiones HTTP
            string URL = "https://www.facebook.com/";
            CookieContainer cJar = new CookieContainer();
            ServicePointManager.Expect100Continue = false;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "GET";
            request.CookieContainer = cJar;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:23.0) Gecko/20100101 Firefox/23.0";
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string body = sr.ReadToEnd();
            sr.Close();
            response.Close();

            //Procesamos el código HTML para extraer todas las variables enviadas en la petición POST de login
            //Iremos extrayendo los inputs ocultos necesarios: lsd, lgnrnd, locale, lgnjs y timezone
            
            //<input type="hidden" name="lsd" value="AVrBXUrq" autocomplete="off" />
            string lsd = "";
            Regex search = new Regex(@"(?i)<input type=""hidden"" name=""lsd"" value=""(.*?)"".*?/>");
            Match find = search.Match(body);
            if (find.Success == true)
            {
                lsd = find.Groups[1].Captures[0].Value;
            }

            //<input type="hidden" name="lgnrnd" value="161023__Yuh" />
            string lgnrnd = "";
            search = new Regex(@"(?i)<input type=""hidden"" name=""lgnrnd"" value=""(.*?)"".*?/>");
            find = search.Match(body);
            if (find.Success == true)
            {
                lgnrnd = find.Groups[1].Captures[0].Value;
            }

            //<input type="hidden" autocomplete="off" id="locale" name="locale" value="es_ES" />
            string locale = "";
            search = new Regex(@"(?i)<input type=""hidden"" autocomplete=""off"" id=""locale"" name=""locale"" value=""(.*?)"".*?/>");
            find = search.Match(body);
            if (find.Success == true)
            {
                locale = find.Groups[1].Captures[0].Value;
            }

            //<input type="hidden" id="lgnjs" name="lgnjs" value="n" />   //El n es remplazado por timestamp
            //como es un timestamp lo generamos en el momento de la petición con la función CreateTimestamp()

            //<input type="hidden" autocomplete="off" name="timezone" value="" id="u_0_c" />
            //Obtiene el valor empleando el método javascript getTimezoneOffset() (en mi caso usa el valor -60 de España)
            //Este valor lo genera con javascript con la función tz_calculate(), la función esta reescrita en C#
            //http://www.w3schools.com/jsref/jsref_gettimezoneoffset.asp
            string timezone = getTimezoneOffset();

            //Formamos la postdata
            //Ejemplo: ////lsd=AVuLtinp&email=mimail%40gmail.com&pass=superpassword&persistent=1&default_persistent=0&timezone=-60&lgnrnd=194752_M67S&lgnjs=1388287072&locale=es_ES
            string postdata = "lsd=" + lsd + "&email=" + user + "&pass=" + pass +
                "&persistent=1&default_persistent=0&timezone=" + timezone + "&lgnrnd=" +
                lgnrnd + "&lgnjs=" + CreateTimestamp() + "&locale=" + locale;

            //Segunda petición HTTP con el método POST para logearse
            string loginURL = "https://www.facebook.com/login.php?login_attempt=1";
            request = (HttpWebRequest)WebRequest.Create(loginURL);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = cJar;
            request.AllowAutoRedirect = false;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:23.0) Gecko/20100101 Firefox/23.0";
            using (Stream stream = request.GetRequestStream())
            {
                byte[] content = ASCIIEncoding.ASCII.GetBytes(postdata);
                stream.Write(content, 0, content.Length);
            }
            response = request.GetResponse();
            sr = new StreamReader(response.GetResponseStream());
            body = sr.ReadToEnd();
            sr.Close();
            response.Close();

            //Ejemplo de manejo de la respuesta (comprobar si ha habido la redirecion de exito o no).
            if (((HttpWebResponse)response).StatusCode == HttpStatusCode.Redirect)
            {
                textBox1.Text = ((HttpWebResponse)response).GetResponseHeader("Location").ToString();
            }
            else
            {
                textBox1.Text = "Algo ha fallado :_(";
            }
        }

        private string CreateTimestamp()
        {
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
            return oauth_timestamp;
        }

        private string getTimezoneOffset()
        {
            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            double signed_offset = -offset.TotalMinutes;
            return signed_offset.ToString();
        }
    }
}
