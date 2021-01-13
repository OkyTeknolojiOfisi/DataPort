using System;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace DataPort
{
    /// <summary>
    /// İleti Yönetim Sistemi'ne DataPort API'ı üzerinden veri kaydetmek için oluşturulmuş sınıftır.
    /// </summary>
    public class iys
    {
        #region Tanımlamalar
        private static string _UserName;
        private static string _Password;
        private static KayitIstek _KayitIstek = new KayitIstek();
        private static MessageList _MessageList = new MessageList();
        private static List<KayitIstekSonuc> _KayitIstekSonuclari = new List<KayitIstekSonuc>();
        /// <summary>
        /// İzin kaynağı seçenekleri
        /// </summary>
        public static class SourceTypes
        {
            #region Beyan Usulü
            /// <summary>
            /// İzin durumu, hizmet sağlayıcının kendi isteğiyle RET olarak belirlenmiştir.
            /// </summary>
            public const string red = "HS_KARAR";
            #endregion
            #region Fiziksel Ortam
            /// <summary>
            /// İzin, hizmet sağlayıcı tarafından fiziksel ortamda alınmıştır.
            /// </summary>
            public const string fiziksel = "HS_FIZIKSEL_ORTAM";
            /// <summary>
            /// İzin, alıcının bir formu veya anketi imzalaması üzerine alınmıştır.
            /// </summary>
            public const string islak_imza = "HS_ISLAK_IMZA";
            /// <summary>
            /// İzin, hizmet sağlayıcının düzenlediği bir etkinlikte alınmıştır.
            /// </summary>
            public const string etkinlik = "HS_ETKINLIK";
            /// <summary>
            /// İzin, hizmet sağlayıcıya ait yerleşik ATM cihazıyla alınmıştır.
            /// </summary>
            public const string atm = "HS_ATM";
            #endregion
            #region Elektronik Ortam
            /// <summary>
            /// İzin, hizmet sağlayıcıya ait bir elektronik ortamda alınmıştır.
            /// </summary>
            public const string elektronik_ortam = "HS_EORTAM";
            /// <summary>
            /// İzin, hizmet sağlayıcının web sitesi üzerinde yapılan bir işlemle alınmıştır.
            /// </summary>
            public const string web = "HS_WEB";
            /// <summary>
            /// İzin, hizmet sağlayıcıya ait mobil uygulama üzerinden alınmıştır.
            /// </summary>
            public const string mobil = "HS_MOBIL";
            /// <summary>
            /// İzin, hizmet sağlayıcıya ait kısa mesaj numarası üzerinden alınmıştır.
            /// </summary>
            public const string mesaj = "HS_MESAJ";
            /// <summary>
            /// İzin, hizmet sağlayıcıya ait e-posta vasıtasıyla alınmıştır.
            /// </summary>
            public const string eposta = "HS_EPOSTA";
            /// <summary>
            /// İzin, hizmet sağlayıcıya bağlı bir çağrı merkezinde sesle veya numara tuşlamayla alınmıştır.
            /// </summary>
            public const string cagri_merkezi = "HS_CAGRI_MERKEZI";
            /// <summary>
            /// İzin, hizmet sağlayıcıya ait sosyal medya aracı üzerinden alınmıştır.
            /// </summary>
            public const string sosyal_medya = "HS_SOSYAL_MEDYA";
            #endregion
        }
        /// <summary>
        /// Alıcı seçenekleri
        /// </summary>
        public static class RecipientTypes
        {
            /// <summary>
            /// Tacir veya esnaf kayıtlarında kullanılacak. Örneğin B2B sistemine üye bayiler.
            /// </summary>
            public const string tacir = "TACIR";
            /// <summary>
            /// Bireysel kayıtlarda kullanılacak. Son kullanıcı da denilebilir.
            /// </summary>
            public const string bireysel = "BIREYSEL";
        }
        /// <summary>
        /// ONAY / RET bilgisi
        /// </summary>
        public static class ContentTypes
        {
            /// <summary>
            /// ONAY
            /// </summary>
            public const string optin = "ONAY";
            /// <summary>
            /// RET
            /// </summary>
            public const string optout = "RET";
        }
        /// <summary>
        /// İzin seçenekleri
        /// </summary>
        public static class PermissionTypes
        {
            /// <summary>
            /// Arama izni
            /// </summary>
            public const string arama = "ARAMA";
            /// <summary>
            /// Mesaj gönderme izni
            /// </summary>
            public const string mesaj = "MESAJ";
            /// <summary>
            /// E-posta gönderme izni (Bu seçenek mevcut fakat DataPort API'si e-posta kabul etmiyor.
            /// </summary>
            public const string eposta = "EPOSTA";
        }
        private class KayitIstek
        {
            public string BrandName { get; set; }
            public string RecipientType { get; set; }
            public MessageList MessageList { get; set; }
        }
        private class MessageList
        {
            public List<Values> GSMList = new List<Values>();
            public List<Values> ContentList = new List<Values>();
            public List<Values> SourceList = new List<Values>();
            public List<Values> TypeList = new List<Values>();
            public List<Values> ConsentDateList = new List<Values>();
        }
        /// <summary>
        /// Kayıt isteğine API'ın verdiği cevapları tutan sınıf. KayitGonder fonksiyonunun geri dönüşü bu sınıfın listesi şeklindedir.
        /// </summary>
        public class KayitIstekSonuc
        {
            /// <summary>
            /// Kayıt için gönderilen cep telefonu numarası
            /// </summary>
            public string GSM { get; set; }
            /// <summary>
            /// Kaydı talep edilen izin türü
            /// </summary>
            public string PermissionType { get; set; }
            /// <summary>
            /// DataPort API'ından dönen işlem takip kodu
            /// </summary>
            public string MessageID { get; set; }
            /// <summary>
            /// İşlem durumu, 0 başarılı şekilde ulaştığını gösteriyor
            /// </summary>
            public int Status { get; set; }
        }
        private class Values
        {
            public string Value { get; set; }
        }
        #endregion

        /// <summary>
        /// Sınıf oluşturucu.
        /// </summary>
        /// <param name="UserName">DataPort kullanıcı adı</param>
        /// <param name="Password">DataPort şifresi</param>
        /// <param name="BrandName">IYS sistemine kayıt işlemi yapılacaksa girilmesi gereklidir.</param>
        /// <param name="RecipientType">Sisteme kaydedilecek alıcıların türleri, RecipientTypes numaralandırmasından seçenekler alınabilir.</param>
        public iys(string UserName, string Password, string BrandName = null, string RecipientType = RecipientTypes.bireysel)
        {
            _UserName = UserName;
            _Password = Password;

            if (BrandName != null)
            {
                _KayitIstek.BrandName = BrandName;
                _KayitIstek.RecipientType = RecipientType;
                _KayitIstek.MessageList = _MessageList;
            }
        }

        #region Metodlar
        /// <summary>
        /// Eklenecek olan alıcıları eklemeye yarayan fonksiyon.
        /// </summary>
        /// <param name="GSM">Telefon numarası</param>
        /// <param name="Source">İletişim bilgisinin kaynağı, SourceTypes numaralandırmasından seçenekler alınabilir.</param>
        /// <param name="ConsentDate">İletişim izninin alındığı tarih, "YYYY-MM-DD HH:mm:ss" formatında olmalı. Null gönderilmesi durumunda anlık tarih gönderilir.</param>
        /// <param name="PermissionType">Alınan iznin türü, belirtilmemesi halinde "mesaj" izni kaydedilir. Diğer seçenekler PermissionTypes numaralandırmasından alınabilir.</param>
        /// <param name="Content">İzin durumu, belirtilmemesi halinde "onay" izni kaydedilir.</param>
        /// <returns>Gönderim paketine eklenen veride True döner. Hatalı bir parametrede hata verir. Try Catch içerisinde kullanılması tavsiye edilir.</returns>
        public bool AliciEkle(string GSM, string Source, string ConsentDate = null, string PermissionType = PermissionTypes.mesaj, string Content = ContentTypes.optin)
        {
            if (_MessageList.GSMList.Count == 1000) { throw new Exception("Tek pakette 1000 numara gönderilebilir."); }

            if (!yardimci.TelefonFormat(GSM)) { throw new Exception("Telefon formatı uygun değil."); }

            if (Source is null) { throw new Exception("İletişim bilgisi kaynağı belirtilmeli. SourceTypes sınıfından değerler alınabilir."); }

            if (ConsentDate != null)
            {
                bool c = DateTime.TryParseExact(ConsentDate, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime d);
                if (!c) { throw new Exception("Tarih yyyy-MM-dd HH:mm:ss formatında olmalıdır."); }
            }
            else
            {
                ConsentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (PermissionType is null) { throw new Exception("Alınan izin türü belirtilmeli. PermissionTypes sınıfından değerler alınabilir."); }

            if (Content is null) { throw new Exception("Numaraya iletişim izni mi verilecek yoksa red mi edilecek belirtilmeli. ContentTypes sınıfından değerler alınabilir."); }

            _MessageList.GSMList.Add(new Values { Value = GSM });
            _MessageList.SourceList.Add(new Values { Value = Source });
            _MessageList.ConsentDateList.Add(new Values { Value = ConsentDate });
            _MessageList.TypeList.Add(new Values { Value = PermissionType });
            _MessageList.ContentList.Add(new Values { Value = Content });

            _KayitIstekSonuclari.Add(new KayitIstekSonuc { GSM = GSM, PermissionType = PermissionType });

            return true;
        }

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
                { "grant_type", "password" }
            };

            var gonder = istemci.UploadValues("https://iys.dataport.com.tr/iysrestapi/Register", degerler);

            string veri = istemci.Encoding.GetString(gonder);

            dynamic json = JObject.Parse(veri);

            string session = json.access_token.ToString();

            return session;
        }

        /// <summary>
        /// Pakete eklenen kayıtları DataPort İYS sistemine gönderir.
        /// </summary>
        /// <returns>Kayıt isteğinde bulunulan telefon numaraları, istenen izin türü, DataPort kayıt işlemine ait benzersiz MessageId ve durumu belirtir Status döndürür.</returns>
        public List<KayitIstekSonuc> KayitGonder()
        {
            var api = new Uri("https://iys.dataport.com.tr/iysrestapi/api/IYSSender/IYSSender");

            string paket = JsonConvert.SerializeObject(_KayitIstek);

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

            int i = 0;

            foreach (KayitIstekSonuc kis in _KayitIstekSonuclari)
            {
                kis.MessageID = donus_json.Results[i].MessageID;
                kis.Status = donus_json.Results[i].Status;
                i++;
            }

            return _KayitIstekSonuclari;
        }
        #endregion
    }
}
