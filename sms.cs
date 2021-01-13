using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace DataPort
{
    /// <summary>
    /// DataPort API'ı ile sms göndermeye yarayan sınıftır.
    /// </summary>
    public class sms
    {
        #region Tanımlamalar
        private static string _UserName;
        private static string _Password;
        private static string _Scope;
        private static SMSGonderiPaketi _SMSGonderiPaketi = new SMSGonderiPaketi();
        private class SMSGonderiPaketi
        {
            public string SessionID = "";
            public int Operator;
            public string GroupID = "";
            public string Orginator;
            public string ShortNumber;
            public int Isunicode;
            public string SendDate;
            public string DeleteDate;
            public MesajListesi MessageList = new MesajListesi();
        }
        private class MesajListesi
        {
            public List<Values> GSMList = new List<Values>();
            public List<Values> ContentList = new List<Values>();
        }
        /// <summary>
        /// Gönderimde kullanılacak operatör değerleri
        /// </summary>
        public static class OperatorTypes
        {
            /// <summary>
            /// Turkcell
            /// </summary>
            public const int Turkcell = 1;
            /// <summary>
            /// Avea
            /// </summary>
            public const int Avea = 2;
            /// <summary>
            /// Vodafone
            /// </summary>
            public const int Vodafone = 3;
            /// <summary>
            /// Superonline
            /// </summary>
            public const int Superonline = 4;
        }
        /// <summary>
        /// Gönderilecek olan SMS'lerin karakter kodlamasını seçebileceğiniz sınıf
        /// </summary>
        public static class UnicodeTypes
        {
            /// <summary>
            /// Türkçe karakter içermeyen metinler için
            /// </summary>
            public const int ingilizce = 0;
            /// <summary>
            /// Türkçe karakter içeren metinler için
            /// </summary>
            public const int turkce = 1;
        }
        private class Values
        {
            public string Value { get; set; }
        }
        #endregion
        /// <summary>
        /// DataPort SMS API'ı ile sms göndermek için gerekli sınıf oluşturucu
        /// </summary>
        /// <param name="UserName">Kullanıcı adı</param>
        /// <param name="Password">Şifre</param>
        /// <param name="Scope">Hesap numarası</param>
        /// <param name="Operator">Servis sağlayıcı operatör değeri. OperatorTypes sınıfından seçilebilir.</param>
        /// <param name="Orginator">Gönderici SMS Başlığı</param>
        /// <param name="ShortNumber">Servis sağlayıcı operatörün kısa numarası.</param>
        /// <param name="Isunicode">Gönderilecek olan SMS'lerin karakter kodlaması.</param>
        /// <param name="SendDate">Gönderimin başlayacağı tarih. İsteğe bağlıdır. Tarih formatı "yyyy-MM-dd HH:mm:ss" şeklinde olmalıdır.</param>
        /// <param name="DeleteDate">Gönderiminin biteceği tarih. İsteğe bağlıdır. Tarih formatı "yyyy-MM-dd HH:mm:ss" şeklinde olmalıdır.</param>
        public sms(string UserName, string Password, string Scope, int Operator, string Orginator, string ShortNumber, int Isunicode, string SendDate = "", string DeleteDate = "")
        {
            _UserName = UserName;
            _Password = Password;
            _Scope = Scope;

            _SMSGonderiPaketi.Operator = Operator;
            _SMSGonderiPaketi.Orginator = Orginator;
            _SMSGonderiPaketi.ShortNumber = ShortNumber;
            _SMSGonderiPaketi.Isunicode = Isunicode;
            _SMSGonderiPaketi.SendDate = SendDate;
            _SMSGonderiPaketi.DeleteDate = DeleteDate;
        }

        #region Metodlar
        /// <summary>
        /// DataPort API'sine sınıf oluştururken verilen parametrelerle oturum isteği gönderir.
        /// </summary>
        /// <returns>DataPort API'si tarafından oluşturulan Bearer Token döndürür.</returns>
        private string Oturum()
        {
            System.Net.WebClient istemci = new System.Net.WebClient();

            istemci.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            NameValueCollection degerler = new NameValueCollection
            {
                { "UserName", _UserName },
                { "Password", _Password },
                { "Scope", _Scope },
                { "grant_type", "password" }
            };

            var gonder = istemci.UploadValues("https://api.dataport.com.tr/restapi/Register", degerler);

            string veri = istemci.Encoding.GetString(gonder);

            dynamic json = JObject.Parse(veri);

            string session = json.access_token.ToString();

            return session;
        }
        /// <summary>
        /// SMS gönderilecek numaraları ve metinleri pakete ekleyen fonksiyon.
        /// Her pakette en az bir metin bulunması gerekir. Paket içerisinde tek metin varsa tüm numaralara aynı metin gönderilir.
        /// </summary>
        /// <param name="GSM">Telefon numarası</param>
        /// <param name="metin">Gönderilecek olan metin</param>
        /// <returns></returns>
        public bool AliciEkle(string GSM, string metin = null)
        {
            if (!yardimci.TelefonFormat(GSM)) { throw new Exception("Telefon formatı uygun değil."); }

            _SMSGonderiPaketi.MessageList.GSMList.Add(new Values { Value = GSM });

            if (metin != null)
            {
                _SMSGonderiPaketi.MessageList.ContentList.Add(new Values { Value = metin });
            }

            return true;
        }
        /// <summary>
        /// Pakete eklenen mesajlar DataPort API'ına gönderilir.
        /// </summary>
        /// <returns></returns>
        public dynamic SMSGonder()
        {
            var api = new Uri("https://api.dataport.com.tr/restapi/api/Messages/SendSMS");

            string paket = JsonConvert.SerializeObject(_SMSGonderiPaketi);

            HttpWebRequest istek = (HttpWebRequest)WebRequest.Create(api);
            istek.Method = "POST";
            istek.ContentType = "application/json; charset=UTF-8";
            istek.Accept = "application/json";
            istek.Headers.Add("Authorization", "Bearer " + Oturum());

            using (var streamWriter = new StreamWriter(istek.GetRequestStream()))
            {
                string json = paket;

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            string donus;

            var httpResponse = (HttpWebResponse)istek.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                donus = streamReader.ReadToEnd();
            }

            dynamic donus_json = JObject.Parse(donus);

            return donus_json;
        }
        #endregion
    }
}
